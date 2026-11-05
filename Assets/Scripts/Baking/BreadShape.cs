using UnityEngine;

namespace BreadBowl
{
    [CreateAssetMenu(fileName = "BreadShape", menuName = "BreadBowl/Bread Shape")]
    public class BreadShape : ScriptableObject
    {
        [SerializeField] private string displayName = "Sourdough";
        [SerializeField] private Mesh mesh;
        [SerializeField] private Vector3 rawSize = new Vector3(3f, 0.8f, 3f);
        [SerializeField] private Vector3 bakedSize = new Vector3(3.3f, 2.5f, 3.3f);

        public string DisplayName => displayName;
        public Mesh Mesh => mesh;
        public Vector3 RawSize => rawSize;
        public Vector3 BakedSize => bakedSize;
    }
}

