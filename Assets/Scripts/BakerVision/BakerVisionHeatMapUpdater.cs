using BreadBowl.Dough;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SourdoughHeatmapUpdater : MonoBehaviour
{
    [SerializeField] private GameObject dough;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Material bakerVisionMaterial;
    [SerializeField] private Material breadMaterial;

    private float[] grid;
    private Texture2D dataTexture;
    private Material bakerVisionInstance;
    private bool isBakerVisionActive = false;
    private KneadableDough kneadableDough;
    private int gridSize;

    private void Awake()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        // Create exactly ONE instance of the heatmap material
        if (bakerVisionMaterial != null)
        {
            bakerVisionInstance = new Material(bakerVisionMaterial);
        }
    }

    private void Start()
    {
        if (dough != null)
        {
            kneadableDough = dough.GetComponent<KneadableDough>();
            gridSize = kneadableDough.GetResolution();
        }

        // Apply initial material state
        SetBakerVision(isBakerVisionActive);

        if (isBakerVisionActive)
        {
            UpdateHeatmap();
        }
    }

    private void Update()
    {
        if (isBakerVisionActive)
        {
            UpdateHeatmap();
        }
    }

    private void UpdateHeatmap()
    {
        if (kneadableDough == null && dough != null)
        {
            kneadableDough = dough.GetComponent<KneadableDough>();
        }

        if (kneadableDough == null) return;

        grid = kneadableDough.GetCells();
        gridSize = kneadableDough.GetResolution();
        if (grid == null || grid.Length == 0) return;


        ApplyFlatGridToMesh(grid, gridSize, gridSize);
    }

    public void ApplyFlatGridToMesh(float[] flatGrid, int width, int height)
    {
        if (flatGrid == null || flatGrid.Length != width * height) return;

        if (dataTexture == null || dataTexture.width != width || dataTexture.height != height)
        {
            dataTexture = new Texture2D(width, height, GraphicsFormat.R32_SFloat, TextureCreationFlags.None)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        dataTexture.SetPixelData(flatGrid, 0);
        dataTexture.Apply();

        SyncShaderProperties();
    }

    public void SetBakerVision(bool on)
    {
        isBakerVisionActive = on;

        if (on)
        {
            meshRenderer.sharedMaterial = bakerVisionInstance;
            SyncShaderProperties();
        }
        else
        {
            meshRenderer.sharedMaterial = breadMaterial;
        }
    }

    private void SyncShaderProperties()
    {
        if (bakerVisionInstance == null) return;

        if (dataTexture != null)
        {
            bakerVisionInstance.SetTexture("_DataTex", dataTexture);
        }

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds localBounds = meshFilter.sharedMesh.bounds;
            Vector4 localProjectionRect = new Vector4(
                localBounds.min.x,
                localBounds.min.z,
                localBounds.size.x,
                localBounds.size.z
            );

            bakerVisionInstance.SetVector("_BoundsRect", localProjectionRect);
            bakerVisionInstance.SetFloat("_TopHalfThresholdY", localBounds.center.y);
        }
    }

    private void OnDestroy()
    {
        if (bakerVisionInstance != null)
        {
            Destroy(bakerVisionInstance);
        }
        if (dataTexture != null)
        {
            Destroy(dataTexture);
        }
    }
}