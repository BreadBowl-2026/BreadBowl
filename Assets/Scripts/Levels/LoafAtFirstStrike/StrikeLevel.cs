using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class StrikeLevel : LevelController
    {
        [SerializeField] private Transform pinRack;
        [SerializeField] private float knockedAngle = 30f;

        private Rigidbody[] pins;
        private Pose[] pinStarts;
        private Vector3[] pinUps;

        protected override void OnLevelStarted()
        {
            pins = pinRack.GetComponentsInChildren<Rigidbody>();
            pinStarts = new Pose[pins.Length];
            pinUps = new Vector3[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                Transform pin = pins[i].transform;
                pinStarts[i] = new Pose(pin.position, pin.rotation);
                pinUps[i] = Quaternion.Inverse(pin.rotation) * Vector3.up;
                pins[i].Sleep();
            }
        }

        protected override bool EvaluateRoll(Loaf loaf)
        {
            int standing = 0;

            for (int i = 0; i < pins.Length; i++)
            {
                if (IsStanding(i))
                {
                    standing++;
                }
            }

            Debug.Log($"{pins.Length - standing} of {pins.Length} pins down", this);

            if (standing == 0)
            {
                return true;
            }

            ResetPins();
            return false;
        }

        private bool IsStanding(int index)
        {
            Vector3 up = pins[index].rotation * pinUps[index];
            return Vector3.Angle(up, Vector3.up) < knockedAngle;
        }

        private void ResetPins()
        {
            for (int i = 0; i < pins.Length; i++)
            {
                pins[i].linearVelocity = Vector3.zero;
                pins[i].angularVelocity = Vector3.zero;
                pins[i].position = pinStarts[i].position;
                pins[i].rotation = pinStarts[i].rotation;
                pins[i].Sleep();
            }
        }
    }
}

