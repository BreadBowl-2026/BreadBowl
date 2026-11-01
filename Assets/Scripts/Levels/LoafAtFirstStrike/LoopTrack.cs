using System.Collections.Generic;
using UnityEngine;

namespace BreadBowl.Levels
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class LoopTrack : MonoBehaviour
    {
        [SerializeField] private float width = 2f;
        [SerializeField] private float leadInLength = 10f;
        [SerializeField][Min(1)] private int leadInSegments = 10;

        private struct Frame
        {
            public Vector3 Position;
            public Vector3 Forward;
            public Vector3 Up;

            public Vector3 Right => Vector3.Cross(Up, Forward);
        }

        private Mesh mesh;

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            List<Frame> frames = new List<Frame>();
            AddStraight(frames, Vector3.zero, leadInLength, leadInSegments);

            Vector2[] profile =
            {
                new Vector2(-width / 2f, 0f),
                new Vector2(width / 2f, 0f)
            };

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            foreach (Frame frame in frames)
            {
                foreach (Vector2 point in profile)
                {
                    vertices.Add(frame.Position + frame.Right * point.x + frame.Up * point.y);
                }
            }

            int pointCount = profile.Length;

            for (int i = 0; i < frames.Count - 1; i++)
            {
                for (int j = 0; j < pointCount - 1; j++)
                {
                    int a = i * pointCount + j;
                    int b = a + 1;
                    int c = a + pointCount;
                    int d = c + 1;

                    triangles.AddRange(new[] { a, c, b, b, c, d});
                }
            }

            mesh = new Mesh { name = "LoopTrack" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void  AddStraight(List<Frame> frames, Vector3 start, float length, int segments)
        {
            for (int i = 0; i <= segments; i++)
            {
                frames.Add(new Frame
                {
                    Position = start + Vector3.forward * (length * i / segments),
                    Forward = Vector3.forward,
                    Up = Vector3.up
                });
            }
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
        }
    }
}

