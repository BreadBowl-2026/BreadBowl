using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using BreadBowl.Dough;

namespace BreadBowl.Bowling
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Loaf : MonoBehaviour
    {
        private Mesh mesh;
        private Material material;
        private CoACD coacd;
        private readonly List<MeshCollider> colliders = new List<MeshCollider>();
        private readonly List<Mesh> colliderMeshes = new List<Mesh>();

        public Rigidbody Body { get; private set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public async Task BuildAsync(Mesh bakedMesh, Material bakedMaterial, Vector3 scale)
        {
            mesh = bakedMesh;
            material = bakedMaterial;

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            transform.localScale = scale;

            coacd = GetComponent<CoACD>();
            if (coacd == null)
            {
                coacd = gameObject.AddComponent<CoACD>();
            }

            await DoughCoACDCollider.GenerateCollidersAsync(
                gameObject,
                mesh,
                coacd,
                colliders,
                colliderMeshes);
        }

        private void OnDestroy()
        {
            foreach (MeshCollider meshCollider in colliders)
            {
                if (meshCollider != null)
                {
                    Destroy(meshCollider);
                }
            }

            foreach (Mesh colliderMesh in colliderMeshes)
            {
                if (colliderMesh != null)
                {
                    Destroy(colliderMesh);
                }
            }

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
