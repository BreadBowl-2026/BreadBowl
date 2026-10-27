using UnityEngine;

namespace BreadBowl.Bowling
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Loaf : MonoBehaviour
    {
        private Mesh mesh;
        private Material material;

        public Rigidbody Body { get; private set; }
        public float RestHeight => -mesh.bounds.min.y * transform.localScale.y;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void Build(Mesh bakedMesh, Material bakedMaterial, Vector3 scale)
        {
            mesh = bakedMesh;
            material = bakedMaterial;

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            transform.localScale = scale;
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
        }
    }
}
