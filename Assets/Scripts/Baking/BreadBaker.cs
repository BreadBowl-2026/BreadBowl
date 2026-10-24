
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using BreadBowl.Dough;

public class BreadBaker : MonoBehaviour
{
    [Header("Baking Times")]
    [SerializeField] private float bakeTime = 8f;
    [SerializeField] private float perfectEndTime = 11f;
    [SerializeField] private float maxBakeTime = 15f;

    [Header("Knead")]
    [SerializeField] private AnimationCurve riseByKnead = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(1f, 1f), new Keyframe(2f, 1f),
        new Keyframe(3f, 0.3f));
    [SerializeField] private AnimationCurve lumpByKnead = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(1f, 0f), new Keyframe(2f, 0f),
        new Keyframe(3f, 0.4f));
    [SerializeField] private float minRiseFraction = 0.5f;
    [SerializeField] private float maxLumpiness = 0.15f;
    [Range(0f, 3f)]
    [SerializeField] private float fallbackKneadPosition = 1.5f;
    // [Range(0f, 2f)]
    // [SerializeField] private float kneadQuality = 0.7f;

    [SerializeField] private Vector3 rawSize = new Vector3(3f, 0.8f, 3f);
    [SerializeField] private Vector3 bakedSize = new Vector3(3.3f, 2.5f, 3.3f);


    [Header("UI")]
    [SerializeField] private Slider bakingProgress;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button takeOutButton;

    [SerializeField] private OvenDoor ovenDoor;

    public event System.Action Finished;

    public Mesh BakedMesh => doughMesh;
    public Material BakedMaterial => doughMaterial;

    private Transform dough;
    private Material doughMaterial;
    private Mesh doughMesh;
    private Vector3[] originalVertices;
    private Vector3 startPosition;
    private Bounds originalBounds;
    private KneadResult kneadResult;
    private float[] vertexRise;
    private float[] vertexLump;

    private bool isBaking = false;
    private float elapsedTime = 0f;
    private Coroutine bakingCoroutine;

    void Start()
    {
        
        dough = transform;
        doughMaterial = GetComponent<Renderer>().material;

        MeshFilter filter = GetComponent<MeshFilter>();

        if (filter.sharedMesh == null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            filter.sharedMesh = sphere.GetComponent<MeshFilter>().sharedMesh;
            Destroy(sphere);
        }

        doughMesh = Instantiate(filter.sharedMesh);
        filter.mesh = doughMesh;
        originalVertices = doughMesh.vertices;
        originalBounds = doughMesh.bounds;

        dough.localScale = rawSize;
        startPosition = dough.localPosition;

        bakingProgress.value = 0f;

        doughMaterial.color = new Color(0.93f, 0.85f, 0.65f);

        startButton.onClick.AddListener(StartBaking);
        takeOutButton.onClick.AddListener(TakeOutBread);

        takeOutButton.gameObject.SetActive(false);

        UpdateTimer();

        if (statusText != null)
            statusText.text = "Ready to Bake";
    }

    public void SetKneadResult(KneadResult result)
    {
        kneadResult = result;
    }

    public void StartBaking()
    {
        if (isBaking) return;

        CacheKneadValues();

        if (ovenDoor == null)
            ovenDoor = FindFirstObjectByType<OvenDoor>();

        if (ovenDoor != null)
            ovenDoor.CloseDoor();

        elapsedTime = 0f;

        UpdateTimer();

        if (statusText != null)
            statusText.text = "Baking...";

        dough.localScale = rawSize;
        dough.localPosition = startPosition;
        doughMaterial.color = new Color(0.93f, 0.85f, 0.65f);

        ChangeShape(0f);

        bakingProgress.value = 0f;

        isBaking = true;

        startButton.gameObject.SetActive(false);
        takeOutButton.gameObject.SetActive(true);

        bakingCoroutine = StartCoroutine(Bake());
    }

    public void ResetBake()
    {
        if (doughMesh == null)
        {
            return;
        }

        isBaking = false;
        bakingCoroutine = null;
        kneadResult = null;
        elapsedTime = 0f;

        dough.localScale = rawSize;
        doughMaterial.color = new Color(0.93f, 0.85f, 0.65f);
        doughMesh.vertices = originalVertices;
        doughMesh.RecalculateNormals();
        doughMesh.RecalculateBounds();

        bakingProgress.value = 0f;
        startButton.gameObject.SetActive(true);
        takeOutButton.gameObject.SetActive(false);
        UpdateTimer();

        if (statusText != null)
        {
            statusText.text = "Ready to Bake";
        }
    }

    IEnumerator Bake()
    {
        // float quality = 1f - Mathf.Abs(kneadQuality - 1f);
        // float height = Mathf.Lerp(1.3f, bakedSize.y, quality);

        // Vector3 finalSize = new Vector3(bakedSize.x, height, bakedSize.z);

        Color rawColor = new Color(0.93f, 0.85f, 0.65f);
        Color bakedColor = new Color(0.78f, 0.55f, 0.28f);
        Color burntColor = new Color(0.35f, 0.18f, 0.08f);

        while (elapsedTime < maxBakeTime)
        {
            elapsedTime += Time.deltaTime;

            elapsedTime = Mathf.Min(elapsedTime, maxBakeTime);
            UpdateTimer();

            float progress = Mathf.Clamp01(elapsedTime / bakeTime);

            dough.localScale = Vector3.Lerp(rawSize, bakedSize, progress);

            float rise = dough.localScale.y - rawSize.y;
            dough.localPosition = startPosition + Vector3.up * rise / 2f;


            float browning = Mathf.Clamp01((progress - 0.3f) / 0.7f);
            doughMaterial.color = Color.Lerp(rawColor, bakedColor, browning);

            if (elapsedTime > perfectEndTime)
            {
                float overbake = Mathf.InverseLerp(
                    perfectEndTime, maxBakeTime, elapsedTime
                );

                doughMaterial.color = Color.Lerp(
                    bakedColor, burntColor, overbake
                );
            }

            ChangeShape(progress);

            bakingProgress.value = Mathf.Clamp01(
                elapsedTime / maxBakeTime
            );

          

            yield return null;
        }

        elapsedTime = maxBakeTime;
        FinishBaking(true);
    }

    public void TakeOutBread()
    {
        if (!isBaking) return;

        if (bakingCoroutine != null)
        {
            StopCoroutine(bakingCoroutine);
            bakingCoroutine = null;
        }

        FinishBaking(false);
    }

    void FinishBaking(bool burnt)
    {
        isBaking = false;
        bakingCoroutine = null;

        if (ovenDoor == null)
            ovenDoor = FindFirstObjectByType<OvenDoor>();

        if (ovenDoor != null)
            ovenDoor.OpenDoor();

        takeOutButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);

        UpdateTimer();

        if (statusText != null)
        {
            if (burnt)
                statusText.text = "Burnt!";
            else if (elapsedTime < bakeTime)
                statusText.text = "Underbaked!";
            else if (elapsedTime <= perfectEndTime)
                statusText.text = "Perfect Bake!";
            else
                statusText.text = "Overbaked!";
        }

        Finished?.Invoke();
    }

    void ChangeShape(float progress)
    {
        Vector3[] vertices = new Vector3[originalVertices.Length];
        float bottom = originalBounds.min.y;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            float rise = Mathf.Lerp(1f, Mathf.Lerp(minRiseFraction, 1f, vertexRise[i]), progress);
            vertex.y = bottom + (vertex.y - bottom) * rise;

            float noise = Mathf.PerlinNoise(vertex.x * 3f, vertex.z * 3f) - 0.5f;
            float lumpiness = vertexLump[i] * maxLumpiness * progress;

            vertex.x += noise * lumpiness;
            vertex.y += noise * 0.04f * progress;
            vertex.z += noise * lumpiness;

            vertices[i] = vertex;
        }

        doughMesh.vertices = vertices;
        doughMesh.RecalculateNormals();
        doughMesh.RecalculateBounds();
    }

    void CacheKneadValues()
    {
        vertexRise = new float[originalVertices.Length];
        vertexLump = new float[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            float kneadPosition = KneadPositionAt(originalVertices[i]);
            vertexRise[i] = riseByKnead.Evaluate(kneadPosition);
            vertexLump[i] = lumpByKnead.Evaluate(kneadPosition);
        }
    }

    float KneadPositionAt(Vector3 vertex)
    {
        if (kneadResult == null)
        {
            return fallbackKneadPosition;
        }

        Vector2 uv = new Vector2(
            Mathf.InverseLerp(originalBounds.min.x, originalBounds.max.x, vertex.x),
            Mathf.InverseLerp(originalBounds.min.z, originalBounds.max.z, vertex.z)
        );

        return kneadResult.Settings.KneadPosition(kneadResult.Sample(uv));
    }

    void UpdateTimer()
    {
        if (timerText == null) return;

        int seconds = Mathf.FloorToInt(elapsedTime);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }
}


