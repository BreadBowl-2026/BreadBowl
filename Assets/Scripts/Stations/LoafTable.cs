using System.Collections;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl
{
    public class LoafTable : MonoBehaviour
    {
        [SerializeField] private Transform cameraView;
        [SerializeField] private Transform slotOrigin;
        [SerializeField][Min(1)] private int columns = 7;
        [SerializeField][Min(1)] private int rows = 2;
        [SerializeField] private Vector2 spacing = new Vector2(0.4f, 0.4f);
        [SerializeField] private float placeDuration = 0.8f;
        [SerializeField] private Transform selectionMarker;
        [SerializeField] private LoafStatsUI stats;

        private Loaf[] slots;

        public Transform CameraView => cameraView;
        public int Capacity => columns * rows;
        public int Count { get; private set; }
        public Loaf Selected { get; private set; }

        private void Awake()
        {
            slots = new Loaf[Capacity];
            Select(null);
        }

        public void Place(Loaf loaf)
        {
            int index = System.Array.IndexOf(slots, null); 

            if (index < 0)
            {
                Debug.LogWarning("LoafTable is full", this);
                return;
            }

            slots[index] = loaf;
            Count++;
            loaf.Clicked += Select;
            Select(loaf);

            loaf.Body.isKinematic = true;
            Vector3 restPosition = SlotPosition(index) + slotOrigin.up * loaf.RestHeight;
            StartCoroutine(Glide(loaf, index, restPosition, slotOrigin.rotation));
        }

        public void Take(Loaf loaf)
        {
            int index = System.Array.IndexOf(slots, loaf);

            if (index < 0)
            {
                return;
            }

            slots[index] = null;
            Count--;

            loaf.Clicked -= Select;
            
            if (Selected == loaf)
            {
                Select(FirstLoaf());
            }
        }

        private IEnumerator Glide(Loaf loaf, int index, Vector3 position, Quaternion rotation)
        {
            Vector3 startPosition = loaf.transform.position;
            Quaternion startRotation = loaf.transform.rotation;
            float elapsed = 0f;

            while (elapsed < placeDuration && slots[index] == loaf)
            {
                elapsed += Time.deltaTime;
                float time = Mathf.SmoothStep(0f, 1f, elapsed / placeDuration);
                loaf.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, position, time),
                    Quaternion.Slerp(startRotation, rotation, time)
                );

                yield return null;
            }

            if (slots[index] == loaf)
            {
                loaf.transform.SetPositionAndRotation(position, rotation);
            }
        }

        private Loaf FirstLoaf()
        {
            foreach (Loaf loaf in slots)
            {
                if (loaf != null)
                {
                    return loaf;
                }
            }

            return null;
        }

        private Vector3 SlotPosition(int index)
        {
            int column = index % columns;
            int row = index / columns;
            return slotOrigin.position + slotOrigin.rotation * new Vector3(column * spacing.x, 0f, row * spacing.y);
        }

        private void Select(Loaf loaf)
        {
            Selected = loaf;
            selectionMarker.gameObject.SetActive(loaf != null);

            if (loaf == null)
            {
                return;
            }

            selectionMarker.position = SlotPosition(System.Array.IndexOf(slots, loaf));
            stats.Show(loaf);
        }

        private void OnDrawGizmos()
        {
            if (slotOrigin == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;

            for (int i = 0; i < Capacity; i++)
            {
                Gizmos.DrawWireSphere(SlotPosition(i), Mathf.Min(spacing.x, spacing.y) * 0.5f);
            }
        }
    }
}


