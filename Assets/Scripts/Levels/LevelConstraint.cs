using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    /// <summary>
    /// Constraints should never result in failure. Instead they should
    /// be an obstacle to completing a goal.
    /// </summary>
    public abstract class LevelConstraint : MonoBehaviour
    {
        public virtual string ViolationMessage => "That roll didn't count";
        public virtual bool AllowsAnotherRoll => true;
        public virtual void OnAttemptStarted() {}
        public virtual void OnRollStarted(Loaf loaf) {}
        public virtual bool IsViolated(Loaf loaf) => false;
    }
}

