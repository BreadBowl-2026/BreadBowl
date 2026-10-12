
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class BreadBaker : MonoBehaviour
{
    [SerializeField] private float bakeTime = 8f;
    [Range(0f, 2f)]
    [SerializeField] private float kneadQuality = 0.7f;

    [SerializeField] private Vector3 rawSize = new Vector3(3f, 0.8f, 3f);
    [SerializeField] private Vector3 bakedSize = new Vector3(3.3f, 2.5f, 3.3f);

    [SerializeField] private Slider bakingProgress;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text qualityText;
    [SerializeField] private TMP_Text statusText;

    private Transform dough;
    private Material doughMaterial;
    private Mesh doughMesh;
    private Vector3[] originalVertices;
    private Vector3 startPosition;
    private bool isBaking = false;

    void Start()
    {
        GameObject bread = GameObject.Find("Dough");

        dough = bread.transform;
        doughMaterial = bread.GetComponent<Renderer>().material;

        MeshFilter filter = bread.GetComponent<MeshFilter>();
        doughMesh = Instantiate(filter.sharedMesh);
        filter.mesh = doughMesh;
        originalVertices = doughMesh.vertices;

        dough.localScale = rawSize;
        startPosition = dough.localPosition;

        bakingProgress.value = 0f;
        timerText.text = bakeTime.ToString("F1") + "s";
        qualityText.text = "Kneading Quality: " + kneadQuality.ToString("F1");
        statusText.text = "Press SPACE to Bake";

        doughMaterial.color = new Color(0.93f, 0.85f, 0.65f);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            !isBaking)
        {
            StartCoroutine(Bake());
        }
    }

    IEnumerator Bake()
    {
        isBaking = true;

        float time = 0f;

        float quality = 1f - Mathf.Abs(kneadQuality - 1f);
        float height = Mathf.Lerp(1.3f, bakedSize.y, quality);

        Vector3 finalSize = new Vector3(bakedSize.x, height, bakedSize.z);

        Color rawColor = new Color(0.93f, 0.85f, 0.65f);
        Color bakedColor = new Color(0.78f, 0.55f, 0.28f);

        while (time < bakeTime)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / bakeTime);

           
            dough.localScale = Vector3.Lerp(rawSize, finalSize, progress);

            float rise = dough.localScale.y - rawSize.y;
            dough.localPosition = startPosition + Vector3.up * rise / 2f;

    
            float browning = Mathf.Clamp01((progress - 0.3f) / 0.7f);
            doughMaterial.color = Color.Lerp(rawColor, bakedColor, browning);

           
            ChangeShape(progress);

            bakingProgress.value = progress;
            timerText.text = Mathf.Max(0f, bakeTime - time).ToString("F1") + "s";
            statusText.text = "Baking...";

            yield return null;
        }

        
        dough.localScale = finalSize;
        dough.localPosition = startPosition + Vector3.up * (finalSize.y - rawSize.y) / 2f;
        doughMaterial.color = bakedColor;
        ChangeShape(1f);

        bakingProgress.value = 1f;
        timerText.text = "0.0s";
        statusText.text = "Bread is Ready!";
        isBaking = false;
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
