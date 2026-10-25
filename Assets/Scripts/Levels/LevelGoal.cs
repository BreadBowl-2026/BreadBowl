using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    /// <summary>
    /// A level can have as many goals as you want. Each goal must 
    /// provide some logic to say that it has been completed.
    /// </summary>
    public abstract class LevelGoal : MonoBehaviour
    {
        public virtual float SettleTime => 0f;
        public virtual void OnRollStarted(Loaf loaf) {}
        public virtual void ResetForNextRoll() {}

        public abstract bool IsComplete(Loaf loaf);
    }
}

