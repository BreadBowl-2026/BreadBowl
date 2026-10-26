using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreadBowl.Dough
{
    [RequireComponent(typeof(CoACD))]
    public class DoughCoACDCollider : MonoBehaviour
    {
        [SerializeField] private Transform visual;

        private readonly List<MeshCollider> colliders = new List<MeshCollider>();
        private readonly List<Mesh> generatedMeshes = new List<Mesh>();
        private CoACD coacd;

        private void Awake()
        {
            coacd = GetComponent<CoACD>();

            if (visual == null)
            {
                Debug.LogError("Assign the dough visual before generating its CoACD colliders.", this);
                enabled = false;
                return;
            }

            MeshFilter meshFilter = visual.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("The dough visual must have a MeshFilter with a mesh.", visual);
                enabled = false;
                return;
            }

            GenerateColliders(meshFilter.sharedMesh);
        }

        private void GenerateColliders(Mesh sourceMesh)
        {
            List<Mesh> meshes;
            try
            {
                meshes = coacd.RunACD(sourceMesh);
            }
            catch (DllNotFoundException exception)
            {
                Debug.LogError($"CoACD native library could not be loaded: {exception.Message}", this);
                enabled = false;
                return;
            }
            catch (EntryPointNotFoundException exception)
            {
                Debug.LogError($"CoACD native entry point is unavailable: {exception.Message}", this);
                enabled = false;
                return;
            }

            if (meshes == null || meshes.Count == 0)
            {
                Debug.LogError("CoACD did not generate any convex hulls for the dough mesh.", this);
                enabled = false;
                return;
            }

            foreach (Mesh mesh in meshes)
            {
                MeshCollider meshCollider = visual.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = mesh;
                colliders.Add(meshCollider);
                generatedMeshes.Add(mesh);
            }
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

            foreach (Mesh mesh in generatedMeshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }
        }
    }
}
