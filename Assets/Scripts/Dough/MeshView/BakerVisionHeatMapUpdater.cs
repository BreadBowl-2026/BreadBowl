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
    private Bounds restBounds;
    private bool hasRestBounds;

    private void Awake()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        // Create one instance of the heatmap material
        if (bakerVisionMaterial != null)
        {
            bakerVisionInstance = new Material(bakerVisionMaterial);
        }

        // get component here instead of start so it exists before OnEnable subscribes to GridChanged
        if (dough != null)
        {
            kneadableDough = dough.GetComponent<KneadableDough>();
        }
    }

    // subscribed baker vision to the GridChanged event so that it doesn't have check
    // for cell updates each frame, it will be notified when cells change instead
    private void OnEnable()
    {
        if (kneadableDough != null)
        {
            kneadableDough.GridChanged += OnGridChanged;
        }
    }

    private void OnDisable()
    {
        if (kneadableDough != null)
        {
            kneadableDough.GridChanged -= OnGridChanged;
        }
    }

    private void Start()
    {
        // capture the rest shape once. the dough mesh deforms during dents and folds,
        // using the current bounds would misalign the heat map after a fold
        // the restBounds also don't change after they are established
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            restBounds = meshFilter.sharedMesh.bounds;
            hasRestBounds = true;
        }

        // Apply initial material state
        SetBakerVision(isBakerVisionActive);
    }

    // called by KneadableDough.GridChanged event. prevents doing work behind the scenes
    // when bakervision is disabled
    private void OnGridChanged()
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
            // grid might have changed while bakervision was off so update it now
            UpdateHeatmap();
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

        // switched to use restBounds instead of current mesh bounds to match
        // how DoughMeshView maps presses onto the mesh
        if (hasRestBounds)
        {
            Vector4 localProjectionRect = new Vector4(
                restBounds.min.x,
                restBounds.min.z,
                restBounds.size.x,
                restBounds.size.z
            );

            bakerVisionInstance.SetVector("_BoundsRect", localProjectionRect);
            bakerVisionInstance.SetFloat("_TopHalfThresholdY", restBounds.center.y);
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