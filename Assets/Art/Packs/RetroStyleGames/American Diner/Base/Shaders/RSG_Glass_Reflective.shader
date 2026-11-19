// RSG clear shop-window glass for BRP / URP / HDRP (explicit pipeline subshaders sharing identical unlit shading).
// The scene behind (the interior card) stays visible; a sharp reflection from the pack's own baked cubemap is ADDED on top
// with a Fresnel falloff, so reflections never eat the transmission (unlike metallic Lit glass).
// _Exposure: 1 for BRP / URP; HDRP materials use a larger value because HDRP lights scenes in physical units.
Shader "RSG/Glass_Reflective"
{
    Properties
    {
        [NoScaleOffset] _ReflectionCube ("Reflection Cubemap", Cube) = "black" {}
        _ReflectionTint ("Reflection Tint", Color) = (0.88, 0.93, 1, 1)
        _ReflectionFacing ("Reflection Facing Camera", Range(0, 1)) = 0.35
        _ReflectionStrength ("Reflection Strength", Range(0, 4)) = 2.2
        _FresnelPower ("Grazing Falloff", Range(1, 8)) = 4
        _InteriorVisibility ("Interior Visibility", Range(0, 1)) = 0.75
        _GrazingHidesInterior ("Grazing Hides Interior", Range(0, 1)) = 0.7
        _Smoothness ("Smoothness", Range(0, 1)) = 1
        _CubeRotation ("Cubemap Rotation (deg)", Range(0, 360)) = 0
        _Exposure ("Pipeline Exposure", Float) = 1
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    samplerCUBE _ReflectionCube;
    CBUFFER_START(UnityPerMaterial)
    float4 _ReflectionCube_HDR;
    half4 _ReflectionTint;
    half _ReflectionFacing, _ReflectionStrength, _FresnelPower, _InteriorVisibility, _GrazingHidesInterior, _Smoothness, _CubeRotation, _Exposure;
    CBUFFER_END

    struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
    struct v2f { float4 pos : SV_POSITION; float3 worldPos : TEXCOORD0; float3 worldNormal : TEXCOORD1; };

    v2f vert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.worldNormal = UnityObjectToWorldNormal(v.normal);
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        float3 n = normalize(i.worldNormal);
        float3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
        half nv = saturate(dot(n, v));
        half fresnel = _ReflectionFacing + (1 - _ReflectionFacing) * pow(1 - nv, _FresnelPower);

        float3 r = reflect(-v, n);
        float a = radians(_CubeRotation), s = sin(a), c = cos(a);
        r = float3(c * r.x - s * r.z, r.y, s * r.x + c * r.z);
        half4 cube = texCUBElod(_ReflectionCube, float4(r, (1 - _Smoothness) * 7));
        half3 env = DecodeHDR(cube, _ReflectionCube_HDR);

        half3 color = env * _ReflectionStrength * fresnel * _ReflectionTint.rgb * _Exposure;
        half alpha = 1 - _InteriorVisibility * (1 - fresnel * _GrazingHidesInterior);
        return half4(color, alpha);
    }
    ENDCG

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Tags { "LightMode" = "Always" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
    Fallback Off
}
