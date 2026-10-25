using System.Collections;
using System.Collections.Generic;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl.Levels
{
    public enum LevelOutcome
    {
        Completed,
        Quit
    }

    public struct LevelResult
    {
        public LevelOutcome Outcome;
        public int LoavesBowled;
        public float Time;
    }

    public class LevelController : MonoBehaviour
    {
        [SerializeField][Min(1)] private int maxLoaves = 13;
        [SerializeField] private string completeMessage = "Level Complete";

        [Header("Roll")]
        [SerializeField] private float settleSpeed = 0.1f;
        [SerializeField] private float settleTime = 1f;
        [SerializeField] private float maxRollTime = 15f;
        [SerializeField] private float fallDepth = 10f;

        private readonly List<Loaf> loaves = new List<Loaf>();
        private KneadBakeStation station;
        private BreadLauncher launcher;
        private CameraFollow cameraFollow;
        private LaneUI laneUI;
        private LaunchPoint launchPoint;
        private LevelGoal[] goals;
        private LevelConstraint[] constraints;
        private Loaf currentLoaf;
        private ResultUI resultUI;
        private LevelResult result;
        private int loavesBowled;
        private float startTime;


        public event System.Action<LevelResult> Finished;

        private void Awake()
        {
            station = GetComponentInChildren<KneadBakeStation>(true);
            launcher = GetComponentInChildren<BreadLauncher>(true);
            cameraFollow = GetComponentInChildren<CameraFollow>(true);
            laneUI = GetComponentInChildren<LaneUI>(true);
            launchPoint = GetComponentInChildren<LaunchPoint>(true);
            resultUI = GetComponentInChildren<ResultUI>(true);
            resultUI.Hide();

            launcher.gameObject.SetActive(false);
            laneUI.Hide();
        }

        private void OnEnable()
        {
            station.LoafReady += HandleLoafReady;
            launcher.Launched += HandleLaunched;
            laneUI.BowlPressed += BowlNextLoaf;
            laneUI.BakePressed += GoToKitchen;
            laneUI.MenuPressed += Quit;
            resultUI.ContinuePressed += HandleContinue;
        }

        private void OnDisable()
        {
            station.LoafReady -= HandleLoafReady;
            launcher.Launched -= HandleLaunched;
            laneUI.BowlPressed -= BowlNextLoaf;
            laneUI.BakePressed -= GoToKitchen;
            laneUI.MenuPressed -= Quit;
            resultUI.ContinuePressed -= HandleContinue;
        }

        private void Start()
        {
            startTime = Time.time;
            goals = FindObjectsByType<LevelGoal>(FindObjectsSortMode.None);
            constraints = FindObjectsByType<LevelConstraint>(FindObjectsSortMode.None);

            if (goals.Length == 0)
            {
                Debug.LogWarning("This level has no LevelGoal, so it can never be completed", this);
            }

            StartAttempt();
            GoToKitchen();
        }

        private void StartAttempt()
        {
            foreach (LevelConstraint constraint in constraints)
            {
                constraint.OnAttemptStarted();
            }
        }

        private void GoToKitchen()
        {
            laneUI.Hide();
            cameraFollow.enabled = false;
            station.Begin();
        }

        private void GoToLane(string message = "")
        {
            cameraFollow.enabled = false;
            MoveCameraTo(launchPoint.CameraView);
            laneUI.Show(loaves.Count, maxLoaves, message);
        }

        private void HandleLoafReady(Loaf loaf)
        {
            loaves.Add(loaf);
            GoToLane();
        }

        private void BowlNextLoaf()
        {
            if (loaves.Count == 0)
            {
                return;
            }

            Loaf loaf = loaves[0];
            loaves.RemoveAt(0);
            StartBowling(loaf);
        }

        private void StartBowling(Loaf loaf)
        {
            loavesBowled++;
            currentLoaf = loaf;
            laneUI.Hide();
            MoveCameraTo(launchPoint.CameraView);

            loaf.gameObject.SetActive(true);
            launcher.gameObject.SetActive(true);
            launcher.SetBall(loaf.Body, launchPoint);
            cameraFollow.SetTarget(loaf.transform);
            cameraFollow.enabled = true;
        }

        private void HandleLaunched()
        {
            foreach (LevelGoal goal in goals)
            {
                goal.OnRollStarted(currentLoaf);
            }

            foreach (LevelConstraint constraint in constraints)
            {
                constraint.OnRollStarted(currentLoaf);
            }

            StartCoroutine(WatchRoll(currentLoaf));
        }

        private IEnumerator WatchRoll(Loaf loaf)
        {
            float elapsed = 0f;
            float stillTime = 0f;

            while (elapsed < maxRollTime &&
                    stillTime < settleTime &&
                    !HasFallen(loaf))
            {
                elapsed += Time.deltaTime;
                stillTime = loaf.Body.linearVelocity.magnitude < settleSpeed
                    ? stillTime + Time.deltaTime
                    : 0f;

                yield return null;
            }

            yield return new WaitForSeconds(LongestSettleTime());

            LevelConstraint violated = FindViolation(loaf);
            bool completed = violated == null && AllGoalsComplete(loaf);

            cameraFollow.enabled = false;
            launcher.gameObject.SetActive(false);
            Destroy(loaf.gameObject);
            currentLoaf = null;

            if (completed)
            {
                Complete();
                yield break;
            }

            foreach (LevelGoal goal in goals)
            {
                goal.ResetForNextRoll();
            }

            string message = violated != null ? violated.ViolationMessage : "";

            if (!AllowsAnotherRoll())
            {
                StartAttempt();
                message = "Out of rolls, try again!";
            }

            GoToLane(message);
        }

        private LevelConstraint FindViolation(Loaf loaf)
        {
            foreach (LevelConstraint constraint in constraints)
            {
                if (constraint.IsViolated(loaf))
                {
                    return constraint;
                }
            }

            return null;
        }

        private bool AllGoalsComplete(Loaf loaf)
        {
            if (goals.Length == 0)
            {
                return false;
            }

            foreach (LevelGoal goal in goals)
            {
                if (!goal.IsComplete(loaf))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AllowsAnotherRoll()
        {
            foreach (LevelConstraint constraint in constraints)
            {
                if (!constraint.AllowsAnotherRoll)
                {
                    return false;
                }
            }

            return true;
        }

        private float LongestSettleTime()
        {
            float longest = 0f;

            foreach (LevelGoal goal in goals)
            {
                longest = Mathf.Max(longest, goal.SettleTime);
            }

            return longest;
        }

        private bool HasFallen(Loaf loaf)
        {
            return loaf.transform.position.y < launchPoint.transform.position.y - fallDepth;
        }

        private void MoveCameraTo(Transform view)
        {
            cameraFollow.transform.SetPositionAndRotation(view.position, view.rotation);
        }

        private void Complete()
        {
            result = BuildResult(LevelOutcome.Completed);
            resultUI.Show(completeMessage, result);
        }

        private void Quit()
        {
            result = BuildResult(LevelOutcome.Quit);
            HandleContinue();
        }

        private void HandleContinue()
        {
            if (Finished == null)
            {
                Debug.Log("Level finished, but no GameController listening.", this);
                return;
            }

            Finished.Invoke(result);
        }

        private LevelResult BuildResult(LevelOutcome outcome)
        {
            return new LevelResult
            {
                Outcome = outcome,
                LoavesBowled = loavesBowled,
                Time = Time.time - startTime
            };
        }
    }
}
