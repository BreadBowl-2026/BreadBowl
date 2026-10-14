
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class BreadBaker : MonoBehaviour
{
    [Header("Baking Times")]
    [SerializeField] private float bakeTime = 8f;
    [SerializeField] private float perfectEndTime = 11f;
    [SerializeField] private float maxBakeTime = 15f;

    [Range(0f, 2f)]
    [SerializeField] private float kneadQuality = 0.7f;

    [SerializeField] private Vector3 rawSize = new Vector3(3f, 0.8f, 3f);
    [SerializeField] private Vector3 bakedSize = new Vector3(3.3f, 2.5f, 3.3f);

    [Header("UI")]
    [SerializeField] private Slider bakingProgress;

    [SerializeField] private Button startButton;
    [SerializeField] private Button takeOutButton;

    [SerializeField] private OvenDoor ovenDoor;

    private Transform dough;
    private Material doughMaterial;
    private Mesh doughMesh;
    private Vector3[] originalVertices;
    private Vector3 startPosition;

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

        dough.localScale = rawSize;
        startPosition = dough.localPosition;

        bakingProgress.value = 0f;

        doughMaterial.color = new Color(0.93f, 0.85f, 0.65f);

        startButton.onClick.AddListener(StartBaking);
        takeOutButton.onClick.AddListener(TakeOutBread);

        takeOutButton.gameObject.SetActive(false);
    }

    public void StartBaking()
    {
        if (isBaking) return;

        if (ovenDoor == null)
            ovenDoor = FindFirstObjectByType<OvenDoor>();

        if (ovenDoor != null)
            ovenDoor.CloseDoor();

        elapsedTime = 0f;
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

    IEnumerator Bake()
    {
        float quality = 1f - Mathf.Abs(kneadQuality - 1f);
        float height = Mathf.Lerp(1.3f, bakedSize.y, quality);

        Vector3 finalSize = new Vector3(bakedSize.x, height, bakedSize.z);

        Color rawColor = new Color(0.93f, 0.85f, 0.65f);
        Color bakedColor = new Color(0.78f, 0.55f, 0.28f);
        Color burntColor = new Color(0.35f, 0.18f, 0.08f);

        while (elapsedTime < maxBakeTime)
        {
            elapsedTime += Time.deltaTime;


            float progress = Mathf.Clamp01(elapsedTime / bakeTime);

            dough.localScale = Vector3.Lerp(rawSize, finalSize, progress);

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


    }

    void ChangeShape(float progress)
    {
        Vector3[] vertices = new Vector3[originalVertices.Length];

        float lumpiness = Mathf.Abs(kneadQuality - 1f) * 0.15f * progress;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            float noise = Mathf.PerlinNoise(vertex.x * 3f, vertex.z * 3f) - 0.5f;

            vertex.x += noise * lumpiness;
            vertex.y += noise * 0.04f * progress;
            vertex.z += noise * lumpiness;

            vertices[i] = vertex;
        }

        doughMesh.vertices = vertices;
        doughMesh.RecalculateNormals();
        doughMesh.RecalculateBounds();
    }
}


