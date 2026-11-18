using System.Collections.Generic;
using System.Collections;
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
        public List<ResultStat> Stats;
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

        [Header("Stages")]
        [SerializeField] private LevelStage[] stages;

        [Header("Testing")]
        [SerializeField] private Loaf testLoaf;

        private LoafTable table;
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
        private int stageIndex;


        public event System.Action<LevelResult> Finished;
        public event System.Action<Loaf> RollStarted;
        public event System.Action<Loaf> RollEnded;

        private void Awake()
        {
            station = GetComponentInChildren<KneadBakeStation>(true);
            launcher = GetComponentInChildren<BreadLauncher>(true);
            cameraFollow = GetComponentInChildren<CameraFollow>(true);
            laneUI = GetComponentInChildren<LaneUI>(true);
            launchPoint = GetComponentInChildren<LaunchPoint>(true);
            resultUI = GetComponentInChildren<ResultUI>(true);
            table = GetComponentInChildren<LoafTable>(true);
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
            // laneUI.MenuPressed += Quit;
            resultUI.ContinuePressed += HandleContinue;
        }

        private void OnDisable()
        {
            station.LoafReady -= HandleLoafReady;
            launcher.Launched -= HandleLaunched;
            laneUI.BowlPressed -= BowlNextLoaf;
            laneUI.BakePressed -= GoToKitchen;
            // laneUI.MenuPressed -= Quit;
            resultUI.ContinuePressed -= HandleContinue;
        }

        private void Start()
        {
            startTime = Time.time;

            if (stages.Length > 0)
            {
                EnterStage(0);
            }
            else // level starts normally if no stages
            {
                goals = FindObjectsByType<LevelGoal>(FindObjectsSortMode.None);
                constraints = FindObjectsByType<LevelConstraint>(FindObjectsSortMode.None);
            }

            if (goals.Length == 0)
            {
                Debug.LogWarning("This level has no LevelGoal, so it can never be completed", this);
            }

            StartAttempt();
            if (testLoaf != null)
            {
                GoToTable();
            }
            else
            {
                GoToKitchen();
            }
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

        private void GoToTable(string message = "")
        {
            if (testLoaf != null && table.Count == 0)
            {
                table.Place(Instantiate(testLoaf, table.transform.position + Vector3.up, Quaternion.identity));
            }

            cameraFollow.enabled = false;
            MoveCameraTo(table.CameraView);
            laneUI.Show(table.Count, maxLoaves, message);
        }

        private void HandleLoafReady(Loaf loaf)
        {
            GoToTable();
        }

        private void BowlNextLoaf()
        {
            Loaf loaf = table.Selected;

            if (loaf == null)
            {
                return;
            }

            table.Take(loaf);
            StartBowling(loaf);
        }

        private void StartBowling(Loaf loaf)
        {
            loavesBowled++;
            currentLoaf = loaf;
            laneUI.Hide();
            MoveCameraTo(launchPoint.CameraView);

            launcher.gameObject.SetActive(true);
            launcher.SetBall(loaf.Body, launchPoint);
            cameraFollow.SetTarget(loaf.transform);
            cameraFollow.enabled = true;
        }

        private void HandleLaunched()
        {
            if (stages.Length > 0)
            {
                stages[stageIndex].RecordRoll();
            }

            foreach (LevelGoal goal in goals)
            {
                goal.OnRollStarted(currentLoaf);
            }

            foreach (LevelConstraint constraint in constraints)
            {
                constraint.OnRollStarted(currentLoaf);
            }

            RollStarted?.Invoke(currentLoaf);

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

            RollEnded?.Invoke(loaf);

            LevelConstraint violated = FindViolation(loaf);
            bool completed = violated == null && AllGoalsComplete(loaf);

            cameraFollow.enabled = false;
            launcher.gameObject.SetActive(false);
            Destroy(loaf.gameObject);
            currentLoaf = null;

            if (completed)
            {
                CompleteStage();
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

            GoToTable(message);
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

        private void EnterStage(int index)
        {
            stageIndex = index;
            LevelStage stage = stages[index];
            goals = stage.Goals;
            constraints = stage.Constraints;
            launchPoint = stage.LaunchPoint;
        }

        private void CompleteStage()
        {
            if (stageIndex >= stages.Length - 1)
            {
                Complete();
                return;
            }

            string message = $"{stages[stageIndex].DisplayName} cleared!";
            EnterStage(stageIndex + 1);
            StartAttempt();
            GoToTable(message);
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

        /// <summary>
        /// Result lines are added in this order: built-in stats, then each
        /// stage's rolls in stage order, then every IResultReporter in the
        /// scene in no particular order.
        /// </summary>
        private LevelResult BuildResult(LevelOutcome outcome)
        {
            float time = Time.time - startTime;

            List<ResultStat> stats = new List<ResultStat>
            {
                new ResultStat("Loaves bowled", loavesBowled.ToString()),
                new ResultStat("Time", FormatTime(time))
            };

            foreach (LevelStage stage in stages)
            {
                stats.Add(stage.Result);
            }

            AddReporterResults(stats);

            return new LevelResult
            {
                Outcome = outcome,
                LoavesBowled = loavesBowled,
                Time = time,
                Stats = stats
            };
        }

        private void AddReporterResults(List<ResultStat> stats)
        {
            foreach (MonoBehaviour behavior in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behavior is IResultReporter reporter)
                {
                    reporter.AddResults(stats);
                }
            }
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int remainder = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{remainder:00}";
        }
    }
}
