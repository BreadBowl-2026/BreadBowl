using UnityEngine;
using BreadBowl.Bowling;
using System.Collections.Generic;
using System;

namespace BreadBowl.Levels
{

    public class HoopReporter : MonoBehaviour, IResultReporter
    {
        [SerializeField] private HoopGoal hoopGoal;
        private int hoopsJumpedThrough = 0;
        public void AddResults(List<ResultStat> stats)
        {
            stats.Add(new ResultStat("Hoops jumped", $"{hoopsJumpedThrough}"));
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            hoopsJumpedThrough = hoopGoal.hoopsJumpedThrough;
        }
    }

}