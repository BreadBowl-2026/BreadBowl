using System.Collections.Generic;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class JumpStats : MonoBehaviour, IResultReporter
    {
        [SerializeField] private Transform ground;
        [SerializeField] private float landingTolerance = 0.5f;

        private LevelController level;
        private Loaf loaf;
        private Collider loafCollider;
        private bool landed;
        private float topSpeed;
        private float maxHeight;
        private float carryDistance;
        private float totalDistance;

        private void Awake()
        {
            level = FindFirstObjectByType<LevelController>();
        }

        private void OnEnable()
        {
            level.RollStarted += HandleRollStarted;
            level.RollEnded += HandleRollEnded;
        }

        private void OnDisable()
        {
            level.RollStarted -= HandleRollStarted;
            level.RollEnded -= HandleRollEnded;
        }

        private void HandleRollStarted(Loaf rolling)
        {
            loaf = rolling;
            loafCollider = rolling.GetComponent<Collider>();
            landed = false;
        }

        private void HandleRollEnded(Loaf rolling)
        {
            if (landed)
            {
                totalDistance = Mathf.Max(totalDistance, Downrange(rolling));
            }

            loaf = null;
            loafCollider = null;
        }

        private void FixedUpdate()
        {
            if (loaf == null)
            {
                return;
            }

            float bottom = loafCollider.bounds.min.y - ground.position.y;
            topSpeed = Mathf.Max(topSpeed, loaf.Body.linearVelocity.magnitude);
            maxHeight = Mathf.Max(maxHeight, bottom);

            if (landed)
            {
                return;
            }

            float downrange = Vector3.Dot(loaf.Body.position - transform.position, transform.forward);

            if (downrange > 0f && bottom <= landingTolerance)
            {
                carryDistance = Mathf.Max(carryDistance, downrange);
                landed = true;
            }
        }

        private float Downrange(Loaf target)
        {
            return Vector3.Dot(target.Body.position - transform.position, transform.forward);
        }

        public void AddResults(List<ResultStat> stats)
        {
            stats.Add(new ResultStat("Top speed", $"{topSpeed:0.0} m/s"));
            stats.Add(new ResultStat("Max height", $"{maxHeight:0.0} m"));
            stats.Add(new ResultStat("Carry distance", carryDistance > 0f ? $"{carryDistance:0.0} m" : "No landing"));
            stats.Add(new ResultStat("Total distance", totalDistance > 0f ? $"{totalDistance:0.0} m" : "No landing"));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(4f, 2f, 0f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 5f);
        }
    }
}

