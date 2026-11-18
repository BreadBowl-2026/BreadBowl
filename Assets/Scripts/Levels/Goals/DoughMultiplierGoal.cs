using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class DoughMultiplierGoal : LevelGoal
    {
        [SerializeField] private float knockedAngle = 30f;
        [SerializeField] private float settleTime = 1f;

        private Rigidbody[] pins;
        private Pose[] pinStarts;
        private Vector3[] pinUps;
        private bool allPinsDown;
        private int lastDownCount = -1;
        private PinsReporter pinsReporter;

        public override float SettleTime => settleTime;

        private void Awake()
        {
            pinsReporter = GetComponent<PinsReporter>();
        }

        private void Start()
        {
            pins = GetComponentsInChildren<Rigidbody>();
            pinStarts = new Pose[pins.Length];
            pinUps = new Vector3[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                Transform pin = pins[i].transform;
                pinStarts[i] = new Pose(pin.position, pin.rotation);
                pinUps[i] = Quaternion.Inverse(pin.rotation) * Vector3.up;
                pins[i].Sleep();
            }

            if (pins.Length == 0)
                Debug.LogWarning("[StrikeGoal] No pin Rigidbodies found under PinRack.", this);
        }

        public override void OnRollStarted(Loaf loaf)
        {
            allPinsDown = false;
            lastDownCount = -1;
            pinsReporter?.BeginRoll();
        }

        public override bool IsComplete(Loaf loaf)
        {
            if (allPinsDown)
                return true;
            if (pins == null || pins.Length == 0)
                return false;

            int downCount = CountPinsDown();

            if (downCount != lastDownCount)
            {
                Debug.Log($"[StrikeGoal] {downCount} / {pins.Length} pins down", this);
                lastDownCount = downCount;
            }

            if (downCount == pins.Length)
            {
                allPinsDown = true;
                Debug.Log("[StrikeGoal] All pins down! Level complete.", this);
            }

            return allPinsDown;
        }

        public override void ResetForNextRoll()
        {
            // Record the result before returning the pins to their starting positions.
            pinsReporter?.EndRoll(CountPinsDown());

            allPinsDown = false;
            lastDownCount = -1;
            if (pins == null)
                return;

            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i] == null)
                    continue;
                pins[i].linearVelocity = Vector3.zero;
                pins[i].angularVelocity = Vector3.zero;
                pins[i].transform.SetPositionAndRotation(pinStarts[i].position, pinStarts[i].rotation);
                pins[i].Sleep();
            }
        }

        public int CountPinsDown()
        {
            if (pins == null)
                return 0;

            int downCount = 0;
            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i] != null && !IsStanding(i))
                    downCount++;
            }

            return downCount;
        }

        private bool IsStanding(int index)
        {
            Vector3 up = pins[index].rotation * pinUps[index];
            return Vector3.Angle(up, Vector3.up) < knockedAngle;
        }
    }
}