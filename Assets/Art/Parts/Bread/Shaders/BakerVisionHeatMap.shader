Shader "Custom/BakerVisionHeatmapOverlay"
{
    Properties
    {
        [Header(Base Bread Settings)]
        [MainTexture] _BaseMap ("Base Bread Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Baker Vision Heatmap)]
        _DataTex ("Data Texture (RFloat)", 2D) = "black" {}
        _BoundsRect ("Local Bounds (minX, minZ, sizeX, sizeZ)", Vector) = (0, 0, 1, 1)
        _TopHalfThresholdY ("Local Top Half Split Y", Float) = 0.0
        _TransitionSoftness ("Top Half Blend Softness", Range(0.001, 0.5)) = 0.05
        _HeatmapOpacity ("Heatmap Overlay Opacity", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0; 
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionOS   : TEXCOORD0;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_DataTex);
            SAMPLER(sampler_DataTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _BoundsRect;
                float _TopHalfThresholdY;
                float _TransitionSoftness;
                float _HeatmapOpacity;
            CBUFFER_END

            float3 GetSmoothGradient(float val)
            {
                val = clamp(val, 0.0, 5.0);

                const float3 c0 = float3(1.0, 0.0, 0.0); // 0.0: Red
                const float3 c1 = float3(1.0, 1.0, 0.0); // 1.0: Yellow
                const float3 c2 = float3(0.0, 1.0, 0.0); // 2.0: Green
                const float3 c3 = float3(0.0, 0.0, 1.0); // 3.0: Blue
                const float3 c4 = float3(0.5, 0.0, 0.5); // 4.0 - 5.0: Purple

                if (val < 1.0)
                    return lerp(c0, c1, val);
                else if (val < 2.0)
                    return lerp(c1, c2, val - 1.0);
                else if (val < 3.0)
                    return lerp(c2, c3, val - 2.0);
                else if (val < 4.0)
                    return lerp(c3, c4, val - 3.0);
                else
                    return c4;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Sample regular bread diffuse/albedo
                float4 breadAlbedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // 2. Local-space top half mask
                float topHalfMask = smoothstep(
                    _TopHalfThresholdY - _TransitionSoftness,
                    _TopHalfThresholdY + _TransitionSoftness,
                    input.positionOS.y
                );

                // 3. Planar projection for the 2D grid
                float2 planarUV = (input.positionOS.xz - _BoundsRect.xy) / max(_BoundsRect.zw, float2(0.0001, 0.0001));

                // 4. Bounding box check
                float inBounds = step(0.0, planarUV.x) * step(planarUV.x, 1.0) * 
                                 step(0.0, planarUV.y) * step(planarUV.y, 1.0);

                // 5. Read float value and evaluate gradient
                float dataValue = SAMPLE_TEXTURE2D(_DataTex, sampler_DataTex, planarUV).r;
                float3 heatmapColor = GetSmoothGradient(dataValue);

                // 6. Alpha overlay blend
                float effectiveOpacity = topHalfMask * inBounds * _HeatmapOpacity;
                float3 finalColor = lerp(breadAlbedo.rgb, heatmapColor, effectiveOpacity);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}