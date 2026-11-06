using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class DartsGoal : LevelGoal
    {
        [SerializeField] private float MaxScore = 100f;
        [SerializeField] private Transform targetCenter;
        [SerializeField][Min(0.01f)] private float targetRadius = 1f;

        private Loaf hitLoaf;

        public float Score { get; private set; }

        private void OnCollisionEnter(Collision collision)
        {
            if (hitLoaf != null)
            {
                return;
            }

            Loaf loaf = collision.collider.GetComponentInParent<Loaf>();
            if (loaf == null)
            {
                return;
            }

            Rigidbody body = loaf.Body;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;

            Vector3 hitPoint = collision.GetContact(0).point;
            Transform scoringTransform = targetCenter != null ? targetCenter : transform;
            Vector3 localHitPoint = scoringTransform.InverseTransformPoint(hitPoint);
            float distance = new Vector2(localHitPoint.x, localHitPoint.y).magnitude;
            Score = Mathf.Clamp01(1f - distance / targetRadius) * MaxScore;
            hitLoaf = loaf;

            Debug.Log($"Darts score: {Score:0}", this);
        }

        public override bool IsComplete(Loaf loaf)
        {
            return hitLoaf == loaf;
        }

        public override void OnRollStarted(Loaf loaf)
        {
            hitLoaf = null;
            Score = 0f;
        }

        public override void ResetForNextRoll()
        {
            hitLoaf = null;
            Score = 0f;
        }
    }
}