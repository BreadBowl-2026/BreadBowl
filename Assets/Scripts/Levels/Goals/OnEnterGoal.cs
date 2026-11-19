using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    [RequireComponent(typeof(Collider))]
    public class GateGoal : LevelGoal
    {
        [SerializeField] private float celebrateTime = 3f;

        private Loaf rollingLoaf;
        private bool passed;

        public override float SettleTime => passed ? celebrateTime : 0f;

        public override void OnRollStarted(Loaf loaf)
        {
            rollingLoaf = loaf;
            passed = false;
        }

        public override bool IsComplete(Loaf loaf)
        {
            return passed;
        }

        public override void ResetForNextRoll()
        {
            rollingLoaf = null;
            passed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (rollingLoaf != null && other.attachedRigidbody == rollingLoaf.Body)
            {
                passed = true;
            }
        }
    }
}
