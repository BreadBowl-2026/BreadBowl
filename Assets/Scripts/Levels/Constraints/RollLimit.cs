using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class RollLimit : LevelConstraint
    {
        [SerializeField][Min(1)] private int maxRolls = 3;

        private int rollsUsed;

        public override bool AllowsAnotherRoll => rollsUsed < maxRolls;

        public override void OnAttemptStarted()
        {
            rollsUsed = 0;
        }

        public override void OnRollStarted(Loaf loaf)
        {
            rollsUsed++;
        }
    }
}
