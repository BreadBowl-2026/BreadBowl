using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace BreadBowl.Dough
{
    public static class DoughCoACDCollider
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMesh
        {
            public IntPtr vertices;
            public ulong vertexCount;
            public IntPtr triangles;
            public ulong triangleCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMeshArray
        {
            public IntPtr meshes;
            public ulong meshCount;
        }

        private sealed class HullData
        {
            public Vector3[] vertices;
            public int[] triangles;
        }

        [DllImport("lib_coacd", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CoACD_run")]
        private static extern NativeMeshArray Run(
            ref NativeMesh mesh,
            double threshold,
            int maxConvexHull,
            int preprocessMode,
            int preprocessResolution,
            int sampleResolution,
            int mctsNodes,
            int mctsIteration,
            int mctsMaxDepth,
            bool pca,
            bool merge,
            uint seed);

        [DllImport("lib_coacd", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CoACD_freeMeshArray")]
        private static extern void Free(NativeMeshArray meshes);

        public static async Task<bool> GenerateCollidersAsync(
            GameObject host,
            Mesh sourceMesh,
            CoACD coacd,
            List<MeshCollider> colliders,
            List<Mesh> generatedMeshes)
        {
            ClearColliders(host, colliders, generatedMeshes);

            if (sourceMesh == null)
            {
                Debug.LogError("Cannot generate CoACD colliders without a source mesh.", host);
                return false;
            }

            Vector3[] vertices = sourceMesh.vertices;
            int[] triangles = sourceMesh.triangles;
            CoACD.Parameters parameters = coacd != null ? coacd.parameters : CoACD.Parameters.Init();

            List<HullData> hulls;
            try
            {
                hulls = await Task.Run(() => Decompose(
                    vertices,
                    triangles,
                    parameters));
            }
            catch (DllNotFoundException exception)
            {
                Debug.LogError($"CoACD native library could not be loaded: {exception.Message}", host);
                return false;
            }
            catch (EntryPointNotFoundException exception)
            {
                Debug.LogError($"CoACD native entry point is unavailable: {exception.Message}", host);
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"CoACD failed to decompose the mesh: {exception.Message}", host);
                return false;
            }

            if (host == null || hulls == null || hulls.Count == 0)
            {
                Debug.LogError("CoACD did not generate any convex hulls for the mesh.", host);
                return false;
            }

            foreach (HullData hull in hulls)
            {
                Mesh mesh = new Mesh();
                mesh.SetVertices(hull.vertices);
                mesh.SetTriangles(hull.triangles, 0);
                mesh.RecalculateBounds();

                MeshCollider meshCollider = host.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = mesh;
                colliders.Add(meshCollider);
                generatedMeshes.Add(mesh);
            }

            return true;
        }

        private static List<HullData> Decompose(
            Vector3[] vertices,
            int[] triangles,
            CoACD.Parameters parameters)
        {
            double[] nativeVertices = new double[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                nativeVertices[i * 3] = vertices[i].x;
                nativeVertices[i * 3 + 1] = vertices[i].y;
                nativeVertices[i * 3 + 2] = vertices[i].z;
            }

            NativeMesh input = new NativeMesh();
            NativeMeshArray result = default;
            bool hasResult = false;
            GCHandle vertexHandle = GCHandle.Alloc(nativeVertices, GCHandleType.Pinned);
            GCHandle triangleHandle = GCHandle.Alloc(triangles, GCHandleType.Pinned);

            try
            {
                input.vertices = vertexHandle.AddrOfPinnedObject();
                input.vertexCount = (ulong)vertices.Length;
                input.triangles = triangleHandle.AddrOfPinnedObject();
                input.triangleCount = (ulong)(triangles.Length / 3);
                result = Run(
                    ref input,
                    parameters.threshold,
                    parameters.maxConvexHull,
                    (int)parameters.preprocessMode,
                    parameters.preprocessResolution,
                    parameters.sampleResolution,
                    parameters.mctsNodes,
                    parameters.mctsIteration,
                    parameters.mctsMaxDepth,
                    parameters.pca,
                    parameters.merge,
                    parameters.seed);
                hasResult = true;
            }
            finally
            {
                vertexHandle.Free();
                triangleHandle.Free();
            }

            try
            {
                int meshSize = Marshal.SizeOf<NativeMesh>();
                List<HullData> hulls = new List<HullData>((int)result.meshCount);
                for (ulong i = 0; i < result.meshCount; i++)
                {
                    NativeMesh nativeMesh = Marshal.PtrToStructure<NativeMesh>(
                        IntPtr.Add(result.meshes, checked((int)(i * (ulong)meshSize))));
                    Vector3[] hullVertices = new Vector3[checked((int)nativeMesh.vertexCount)];
                    int[] hullTriangles = new int[checked((int)(nativeMesh.triangleCount * 3))];
                    double[] vertexValues = new double[hullVertices.Length * 3];

                    Marshal.Copy(nativeMesh.vertices, vertexValues, 0, vertexValues.Length);
                    for (int vertex = 0; vertex < hullVertices.Length; vertex++)
                    {
                        hullVertices[vertex] = new Vector3(
                            (float)vertexValues[vertex * 3],
                            (float)vertexValues[vertex * 3 + 1],
                            (float)vertexValues[vertex * 3 + 2]);
                    }

                    Marshal.Copy(nativeMesh.triangles, hullTriangles, 0, hullTriangles.Length);
                    hulls.Add(new HullData { vertices = hullVertices, triangles = hullTriangles });
                }

                return hulls;
            }
            finally
            {
                if (hasResult)
                {
                    Free(result);
                }
            }
        }

        public static void ClearColliders(
            GameObject host,
            List<MeshCollider> colliders,
            List<Mesh> generatedMeshes)
        {
            foreach (Mesh mesh in generatedMeshes)
            {
                DestroyObject(mesh);
            }

            colliders.Clear();
            generatedMeshes.Clear();

            if (host == null)
            {
                return;
            }

            foreach (Collider collider in host.GetComponents<Collider>())
            {
                DestroyObject(collider);
            }
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
