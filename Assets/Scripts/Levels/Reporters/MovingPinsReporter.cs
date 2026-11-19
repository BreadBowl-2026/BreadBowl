using System.Collections.Generic;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class MovingPinsReporter : MonoBehaviour, IResultReporter
    {
        private LevelController levelController;
        private MovingTargetPin[] pins;
        private readonly List<int> downAfterEachRoll = new List<int>();

        private void Awake()
        {
            levelController = FindFirstObjectByType<LevelController>();
            pins = GetComponentsInChildren<MovingTargetPin>(true);
        }

        private void OnEnable()
        {
            if (levelController != null)
                levelController.RollEnded += RecordRoll;
        }

        private void OnDisable()
        {
            if (levelController != null)
                levelController.RollEnded -= RecordRoll;
        }

        private void RecordRoll(Loaf loaf)
        {
            int down = 0;
            foreach (MovingTargetPin pin in pins)
            {
                if (pin != null && pin.IsDown)
                    down++;
            }

            downAfterEachRoll.Add(down);
        }

        public void AddResults(List<ResultStat> stats)
        {
            for (int i = 0; i < downAfterEachRoll.Count; i++)
                stats.Add(new ResultStat($"Roll {i + 1}", $"{downAfterEachRoll[i]} / {pins.Length} down"));
        }
    }
}
