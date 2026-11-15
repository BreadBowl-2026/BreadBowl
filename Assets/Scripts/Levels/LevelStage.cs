using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class LevelStage : MonoBehaviour
    {
        [SerializeField] private string displayName = "Stage 1";

        private int rolls;

        public string DisplayName => displayName;
        public LevelGoal[] Goals => GetComponentsInChildren<LevelGoal>();
        public LevelConstraint[] Constraints => GetComponentsInChildren<LevelConstraint>();
        public LaunchPoint LaunchPoint => GetComponentInChildren<LaunchPoint>();

        public ResultStat Result => new ResultStat(
            displayName, 
            rolls == 1 ? "1 roll" : $"{rolls} rolls"
        );

        public void RecordRoll()
        {
            rolls++;
        }
    }
}

