using UnityEngine;

namespace BreadBowl.Dough
{
    [CreateAssetMenu(fileName = "KneadSettings", menuName = " BreadBowl/Dough/Knead Settings")]
    public class KneadSettings : ScriptableObject
    {
        [SerializeField, Min(0f)] private float wellKneadedThreshold = 1f;
        [SerializeField, Min(0f)] private float overKneadedThreshold = 2f;

        public float WellKneadedThreshold => wellKneadedThreshold;
        public float OverKneadedThreshold => overKneadedThreshold;

        public KneadLevel Classify(float kneadedness)
        {
            if (kneadedness >= overKneadedThreshold)
            {
                return KneadLevel.Over;
            }

            if (kneadedness >= wellKneadedThreshold)
            {
                return KneadLevel.Well;
            }

            return KneadLevel.Under;
        }

        // called whenever values are changes in the inspector, ensures Classify can
        // return a well kneaded result
        private void OnValidate()
        {
            overKneadedThreshold = Mathf.Max(overKneadedThreshold, wellKneadedThreshold);
        }
    }
}

