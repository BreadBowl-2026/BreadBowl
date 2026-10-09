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
        [SerializeField, Min(0f), Tooltip("Minimum value at which a cell becomes well kneaded")] 
        private float wellKneadedThreshold = 1f;
        [SerializeField, Min(0f), Tooltip("Minimum value at which a cell becomes over kneaded. Must be higher than well kneaded.")]
        private float overKneadedThreshold = 2f;
        [SerializeField, Min(1), Tooltip("How many keys must be pressed at once to fold the dough")]
        private int foldGrabKeyCount = 3;
        [SerializeField, Range(0f, 1f), Tooltip("How far each cell value shifts during averaging between itself and its pair. 1 = full, 0 = none")]
        private float foldBlend = 1f;
        [SerializeField, Min(0f), Tooltip("Kneadedness added per second at the center of each valid pressed key")]
        private float kneadForcePerSecond = 1f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Radius of each key press as a fraction of dough width")]
        private float pressRadius = 0.15f;
        [SerializeField, Tooltip("How press force fades from the center of each key press to the edge of the radius")]
        private KneadBrushFalloff pressFalloff = KneadBrushFalloff.Smooth;

        public float WellKneadedThreshold => wellKneadedThreshold;
        public float OverKneadedThreshold => overKneadedThreshold;
        public int FoldGrabKeyCount => foldGrabKeyCount;
        public float FoldBlend => foldBlend;
        public float KneadForcePerSecond => kneadForcePerSecond;
        public float PressRadius => pressRadius;
        public KneadBrushFalloff PressFalloff => pressFalloff;

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

        /// <summary>
        /// Uses Mathf.InverseLerp to store the progress between two knead states as a 0-1 value
        /// instead of storing arbitrary color values to make progress more readable.
        /// </summary>
        public float KneadLevelProgress(float kneadedness)
        {
            float wellKneadedRange = overKneadedThreshold - wellKneadedThreshold;

            return Classify(kneadedness) switch
            {
                KneadLevel.Over => Mathf.InverseLerp(overKneadedThreshold, overKneadedThreshold + wellKneadedRange, kneadedness),
                KneadLevel.Well => Mathf.InverseLerp(wellKneadedThreshold, overKneadedThreshold, kneadedness),
                _ => Mathf.InverseLerp(0f, wellKneadedThreshold, kneadedness) // KneadLevel.Under
            };
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

