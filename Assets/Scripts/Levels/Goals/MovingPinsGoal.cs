using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class MovingPinsGoal : LevelGoal
    {
        [SerializeField, Min(0f)] private float settleTime = 1.5f;
        private MovingTargetPin[] pins;

        public override float SettleTime => settleTime;

        private void Awake()
        {
            pins = GetComponentsInChildren<MovingTargetPin>(true);
            if (pins.Length < 2)
                Debug.LogWarning("Add at least two MovingTargetPin children to PinRack.", this);
        }

        public override bool IsComplete(Loaf loaf)
        {
            if (pins == null || pins.Length < 2) return false;
            foreach (MovingTargetPin pin in pins)
                if (!pin.IsDown) return false;
            return true;
        }
    }
}
