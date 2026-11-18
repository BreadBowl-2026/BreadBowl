using System.Collections.Generic;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class PinsReporter : MonoBehaviour, IResultReporter
    {
        private DoughMultiplierGoal goal;
        private readonly List<int> pinsPerRoll = new List<int>();
        private bool rollActive;

        private void Awake()
        {
            goal = GetComponent<DoughMultiplierGoal>();
        }

        public void BeginRoll()
        {
            if (rollActive)
                EndRoll(goal != null ? goal.CountPinsDown() : 0);

            rollActive = true;
        }

        public void EndRoll(int pinsDown)
        {
            if (!rollActive)
                return;

            pinsPerRoll.Add(pinsDown);
            rollActive = false;
        }

        public void AddResults(List<ResultStat> stats)
        {
            if (rollActive && goal != null)
                EndRoll(goal.CountPinsDown());

            for (int i = 0; i < pinsPerRoll.Count; i++)
            {
                stats.Add(new ResultStat($"Roll {i + 1} - Pins knocked", pinsPerRoll[i].ToString()));
            }
        }
    }
}
