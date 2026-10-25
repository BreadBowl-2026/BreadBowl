using System.Collections;
using System.Collections.Generic;
using BreadBowl.Bowling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl.Levels
{
    public enum LevelOutcome
    {
        Completed,
        Quit
    }

    public abstract class LevelController : MonoBehaviour
    {
        [SerializeField] private LaunchPoint launchPoint;
        [SerializeField][Min(1)] private int maxLoaves = 13;

        [Header("Lane UI")]
        [SerializeField] private GameObject laneCanvas;
        [SerializeField] private Button bowlButton;
        [SerializeField] private Button bakeButton;
        [SerializeField] private TMP_Text loafCountText;

        [Header("Roll")]
        [SerializeField] private float settleSpeed = 0.1f;
        [SerializeField] private float settleTime = 1f;
        [SerializeField] private float maxRollTime = 15f;
        [SerializeField] private float fallDepth = 10f;
        [SerializeField] private float pinSettleTime = 1f;

        private readonly List<Loaf> loaves = new List<Loaf>();
        private KneadBakeStation station;
        private BreadLauncher launcher;
        private CameraFollow cameraFollow;
        private Loaf currentLoaf;

        public event System.Action<LevelOutcome> Finished;

        protected LaunchPoint LaunchPoint => launchPoint;

        private void Awake()
        {
            station = GetComponentInChildren<KneadBakeStation>(true);
            launcher = GetComponentInChildren<BreadLauncher>(true);
            cameraFollow = GetComponentInChildren<CameraFollow>(true);

            launcher.gameObject.SetActive(false);
            laneCanvas.SetActive(false);
        }

        private void OnEnable()
        {
            station.LoafReady += HandleLoafReady;
            launcher.Launched += HandleLaunched;
            bowlButton.onClick.AddListener(BowlNextLoaf);
            bakeButton.onClick.AddListener(GoToKitchen);
        }

        private void OnDisable()
        {
            station.LoafReady -= HandleLoafReady;
            launcher.Launched -= HandleLaunched;
            bowlButton.onClick.RemoveListener(BowlNextLoaf);
            bakeButton.onClick.RemoveListener(GoToKitchen);
        }

        private void Start()
        {
            OnLevelStarted();
            GoToKitchen();
        }

        private void GoToKitchen()
        {
            laneCanvas.SetActive(false);
            cameraFollow.enabled = false;
            station.Begin();
        }

        private void GoToLane()
        {
            cameraFollow.enabled = false;
            MoveCameraTo(launchPoint.CameraView);
            laneCanvas.SetActive(true);

            bowlButton.interactable = loaves.Count > 0;
            bakeButton.interactable = loaves.Count < maxLoaves;
            loafCountText.text = $"Loaves: {loaves.Count}/{maxLoaves}";
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
            currentLoaf = loaf;
            laneCanvas.SetActive(false);
            MoveCameraTo(launchPoint.CameraView);

            loaf.gameObject.SetActive(true);
            launcher.gameObject.SetActive(true);
            launcher.SetBall(loaf.Body, launchPoint);
            cameraFollow.SetTarget(loaf.transform);
            cameraFollow.enabled = true;
        }

        private void HandleLaunched()
        {
            OnLoafLaunched(currentLoaf);
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

             yield return new WaitForSeconds(pinSettleTime);

             bool completed = EvaluateRoll(loaf);

             cameraFollow.enabled = false;
             launcher.gameObject.SetActive(false);
             Destroy(loaf.gameObject);
             currentLoaf = null;

             if (completed)
             {
                Finished?.Invoke(LevelOutcome.Completed);
             }
             else
             {
                GoToLane();
             }
        }

        private bool HasFallen(Loaf loaf)
        {
            return loaf.transform.position.y < launchPoint.transform.position.y - fallDepth;
        }

        private void MoveCameraTo(Transform view)
        {
            cameraFollow.transform.SetPositionAndRotation(view.position, view.rotation);
        }

        protected virtual void OnLevelStarted() {}
        protected virtual void OnLoafLaunched(Loaf loaf) {}
        protected abstract bool EvaluateRoll(Loaf loaf);
    }
}

