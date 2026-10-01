using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleBreadBaker : MonoBehaviour
{
    [Header("Baking")]
    [SerializeField] private float bakeDuration = 8f;

    [Header("Size (x, y, z)")]
    [SerializeField] private Vector3 rawSize   = new Vector3(3f, 1f, 3f);
    [SerializeField] private Vector3 bakedSize = new Vector3(3.3f, 2.5f, 3.3f);

    [SerializeField] private Gradient crustGradient;
    [SerializeField] private float rawShininess  = 0.4f;
    [SerializeField] private float bakedShininess = 0.05f;

    private Transform dough;
    private Material doughMat;
    private bool isBaking = false;

    private void Reset()
    {
        crustGradient = new Gradient();
        crustGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.93f, 0.85f, 0.65f), 0.00f), // pale dough
                new GradientColorKey(new Color(0.90f, 0.70f, 0.38f), 0.35f), // light gold
                new GradientColorKey(new Color(0.80f, 0.52f, 0.22f), 0.65f), // golden brown
                
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            });
    }

    private void Start()
    {
        CreateTin();
        CreateDough();
        ApplyLook(0f);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isBaking)
        {
            StartCoroutine(Bake());
        }
    }

    private void CreateTin()
    {
        GameObject tin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tin.name = "Tin";
        tin.transform.SetParent(transform, false);
        tin.transform.localScale = new Vector3(5f, 0.1f, 5f);
        tin.transform.localPosition = new Vector3(0f, -0.1f, 0f);

        Material tinMat = tin.GetComponent<Renderer>().material;
        tinMat.color = new Color(0.22f, 0.22f, 0.24f);
        tinMat.SetFloat("_Smoothness", 0.5f);  
        tinMat.SetFloat("_Glossiness", 0.5f);
    }

    private void CreateDough()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "Dough";
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = rawSize;
        dough = obj.transform;

        Renderer r = obj.GetComponent<Renderer>();
        doughMat = r.material;
    }

    private IEnumerator Bake()
    {
        isBaking = true;
        float timePassed = 0f;

        while (timePassed < bakeDuration)
        {
            timePassed += Time.deltaTime;
            float progress = Mathf.Clamp01(timePassed / bakeDuration);

            float riseProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.6f));
            dough.localScale = Vector3.Lerp(rawSize, bakedSize, riseProgress);

            ApplyLook(progress);

            yield return null;
        }

        dough.localScale = bakedSize;
        ApplyLook(1f);
        isBaking = false;
    }

    private void ApplyLook(float progress)
    {
        doughMat.color = crustGradient.Evaluate(progress);

        float shine = Mathf.Lerp(rawShininess, bakedShininess, progress);
        if (doughMat.HasProperty("_Smoothness")) doughMat.SetFloat("_Smoothness", shine); 
        if (doughMat.HasProperty("_Glossiness")) doughMat.SetFloat("_Glossiness", shine);
    }
}