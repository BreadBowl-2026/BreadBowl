using UnityEngine;

namespace BreadBowl.Dough
{
    /// <summary>
    /// KneadSettings is how kneading is tuned. It works together with the raw values
    /// of KneadGrid by defining what those numbers mean. This way different dough types
    /// could hold references to their own KneadSettings and be tuned independently 
    /// without having to write separate scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "KneadSettings", menuName = "BreadBowl/Dough/Knead Settings")]
    public class KneadSettings : ScriptableObject
    {
        // a larger gap means more knead force needs to be applied to ascend knead states
        [SerializeField, Min(0f), Tooltip("Minimum value at which a cell becomes well kneaded")] 
        private float wellKneadedThreshold = 1f;
        [SerializeField, Min(0f), Tooltip("Minimum value at which a cell becomes over kneaded. Must be higher than well kneaded.")]
        private float overKneadedThreshold = 2f;

        public float WellKneadedThreshold => wellKneadedThreshold;
        public float OverKneadedThreshold => overKneadedThreshold;

        /// <summary>
        /// Takes in the kneadedness value and determines how it should be
        /// interpreted as a KneadLevel.
        /// </summary>
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

        // Only runs in the editor when values are changed in the inspector. Doesn't
        // affect anything at runtime. Just ensures that Classify can always return a 
        // valid wellKneadedState when messing around with values in the editor.
        private void OnValidate()
        {
            overKneadedThreshold = Mathf.Max(overKneadedThreshold, wellKneadedThreshold + 0.01f);
        }
    }
}

