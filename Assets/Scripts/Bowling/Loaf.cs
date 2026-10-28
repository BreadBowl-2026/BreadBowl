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

        public Rigidbody Body { get; private set; }
        public KneadResult Knead { get; private set; }

        public float RestHeight => -mesh.bounds.min.y * transform.localScale.y;

        public event System.Action<Loaf> Clicked;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void Build(Mesh bakedMesh, Material bakedMaterial, Vector3 scale, KneadResult knead)
        {
            mesh = bakedMesh;
            material = bakedMaterial;
            Knead = knead;

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
