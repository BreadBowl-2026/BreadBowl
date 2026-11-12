using UnityEngine;
using UnityEngine.EventSystems;
using BreadBowl.Dough;

namespace BreadBowl.Bowling
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Loaf : MonoBehaviour, IPointerClickHandler
    {
        private Mesh mesh;
        private Material material;
        private PhysicsMaterial physicsMaterial;

        public Rigidbody Body { get; private set; }
        public KneadResult Knead { get; private set; }

        public float RestHeight => -GetComponent<MeshFilter>().sharedMesh.bounds.min.y * transform.localScale.y;

        public event System.Action<Loaf> Clicked;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void Build(
            Mesh bakedMesh, 
            Material bakedMaterial, 
            Vector3 scale, 
            KneadResult knead, 
            float dynamicFriction, 
            float staticFriction
        ){
            mesh = bakedMesh;
            material = bakedMaterial;
            Knead = knead;

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            
            GetComponent<MeshRenderer>().sharedMaterial = material;
            transform.localScale = scale;

            physicsMaterial = GetComponent<MeshCollider>().material;
            physicsMaterial.dynamicFriction = dynamicFriction;
            physicsMaterial.staticFriction = staticFriction;
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }

            if (material != null)
            {
                Destroy(material);
            }

            if (physicsMaterial != null)
            {
                Destroy(physicsMaterial);
            }
        }

    #if UNITY_EDITOR
        [ContextMenu("Save As Test Loaf")]
        private void SaveAsTestLoaf()
        {
            string prefabPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(
                $"Assets/Prefabs/Parts/Bread/TestLoaves/{TestLoafName()}.prefab");

            Mesh savedMesh = Instantiate(mesh);
            Material savedMaterial = new Material(material);
            UnityEditor.AssetDatabase.CreateAsset(savedMesh, System.IO.Path.ChangeExtension(prefabPath, ".asset"));
            UnityEditor.AssetDatabase.CreateAsset(savedMaterial, System.IO.Path.ChangeExtension(prefabPath, ".mat"));

            GameObject copy = Instantiate(gameObject);
            copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.GetComponent<MeshFilter>().sharedMesh = savedMesh;
            copy.GetComponent<MeshCollider>().sharedMesh = savedMesh;
            copy.GetComponent<MeshRenderer>().sharedMaterial = savedMaterial;

            UnityEditor.PrefabUtility.SaveAsPrefabAsset(copy, prefabPath);
            Destroy(copy);
        }

        private string TestLoafName()
        {
            if (Knead == null)
            {
                return "TestLoaf";
            }

            return $"TestLoaf U{Mathf.RoundToInt(Knead.UnderFraction * 100f)}" +
                $" W{Mathf.RoundToInt(Knead.WellFraction * 100f)}" +
                $" O{Mathf.RoundToInt(Knead.OverFraction * 100f)}";
        }
    #endif
    }
}
