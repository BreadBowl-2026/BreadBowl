using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class LevelStage : MonoBehaviour
    {
        [SerializeField] private string displayName = "Stage 1";

        public string DisplayName => displayName;
        public LevelGoal[] Goals => GetComponentsInChildren<LevelGoal>();
        public LevelConstraint[] Constraints => GetComponentsInChildren<LevelConstraint>();
        public LaunchPoint LaunchPoint => GetComponentInChildren<LaunchPoint>();
    }
}

