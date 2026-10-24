using System.Collections;
using BreadBowl.Dough;
using BreadBowl.Bowling;
using UnityEngine;

namespace BreadBowl
{
    public class KneadBakeStation : MonoBehaviour
    {
        [SerializeField] private KneadStage kneadStage;
        [SerializeField] private BreadBaker breadBaker;
        [SerializeField] private GameObject kneadCanvas;
        [SerializeField] private GameObject bakeCanvas;
        [SerializeField] private GameObject showcaseCanvas;

        [Header("Camera")]
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform kneadView;
        [SerializeField] private Transform bakeView;

        [Header("Showcase")]
        [SerializeField] private Showcase showcase;
        [SerializeField] private Transform showcaseView;
        [SerializeField] private Transform tin;
        [SerializeField] private Transform pullOutPoint;
        [SerializeField] private float slideDuration = 0.8f;
        [SerializeField] private float liftDelay = 0.7f;
        [SerializeField] private float liftDuration = 1f;

        [Header("Steam")]
        [SerializeField] private BreadSteam steam;

        [Header("Loaf")]
        [SerializeField] private Loaf loafPrefab;

        private struct Placement
        {
            private readonly Transform parent;
            private readonly Vector3 position;
            private readonly Quaternion rotation;

            public Placement(Transform target)
            {
                parent = target.parent;
                position = target.localPosition;
                rotation = target.localRotation;
            }

            public void Restore(Transform target)
            {
                target.SetParent(parent, false);
                target.SetLocalPositionAndRotation(position, rotation);
            }
        }

        private Placement tinHome;
        private Placement bakerHome;
        private Placement showcaseHome;
        private Placement steamHome;

        public event System.Action<Loaf> LoafReady;

        private void OnEnable()
        {
            kneadStage.Finished += HandleKneadFinished;
            breadBaker.Finished += HandleBakerFinished;
        }

        private void OnDisable()
        {
            kneadStage.Finished -= HandleKneadFinished;
            breadBaker.Finished -= HandleBakerFinished;
        }

        // needs to be awake so this is guaranteed to be ready when a levelcontroller
        // calls Begin() from it's own Start() method
        private void Awake()
        {
            if (cameraRig == null)
            {
                cameraRig = Camera.main.transform;
            }

            tinHome = new Placement(tin);
            bakerHome = new Placement(breadBaker.transform);
            showcaseHome = new Placement(showcase.transform);

            if (steam != null)
            {
                steamHome = new Placement(steam.transform);
            }

            kneadStage.gameObject.SetActive(false);
            kneadCanvas.SetActive(false);
            ResetStation();
        }

        [ContextMenu("Begin")]
        public void Begin()
        {
            kneadStage.gameObject.SetActive(true);
            kneadCanvas.SetActive(true);
            MoveCamera(kneadView);
        }

        private void ResetStation()
        {
            if (steam != null)
            {
                steam.StopAndClear();
                steamHome.Restore(steam.transform);
            }

            bakerHome.Restore(breadBaker.transform);
            tinHome.Restore(tin);
            showcaseHome.Restore(showcase.transform);
            breadBaker.ResetBake();

            showcase.enabled = false;
            showcaseCanvas.SetActive(false);
            bakeCanvas.SetActive(false);
            breadBaker.gameObject.SetActive(false);
        }

        public void TakeLoaf()
        {
            showcaseCanvas.SetActive(false);
            showcase.enabled = false;

            if (steam != null)
            {
                steam.FadeOut(1f);
            }

            Loaf loaf = Instantiate(loafPrefab);
            loaf.Build(Instantiate(breadBaker.BakedMesh), new Material(breadBaker.BakedMaterial), breadBaker.transform.lossyScale);
            loaf.gameObject.SetActive(false);

            LoafReady?.Invoke(loaf);
        }

        private void HandleKneadFinished(KneadResult result)
        {
            breadBaker.SetKneadResult(result);
            kneadStage.gameObject.SetActive(false);
            kneadCanvas.SetActive(false);
            breadBaker.gameObject.SetActive(true);
            bakeCanvas.SetActive(true);
            MoveCamera(bakeView);
        }

        private void HandleBakerFinished()
        {
            bakeCanvas.SetActive(false);
            StartCoroutine(ShowcaseRoutine());
        }

        private IEnumerator ShowcaseRoutine()
        {
            yield return new WaitForSeconds(liftDelay);

            breadBaker.transform.SetParent(tin, true);
            StartSteam();

            yield return MoveOnto(tin, pullOutPoint, slideDuration);
            yield return MoveOnto(breadBaker.transform, showcase.transform, liftDuration);

            MoveCamera(showcaseView);
            showcase.enabled = true;
            showcaseCanvas.SetActive(true);
        }

        private void StartSteam()
        {
            if (steam == null) return;

            // Follow the loaf; the particles simulate in world space so they trail behind.
            steam.transform.SetParent(breadBaker.transform, false);
            steam.transform.localPosition = Vector3.zero;
            steam.Play();
        }

        private IEnumerator MoveOnto(Transform mover, Transform target, float duration)
        {
            mover.SetParent(target, true);
            Vector3 startPosition = mover.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float time = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                mover.localPosition = Vector3.Lerp(startPosition, Vector3.zero, time);
                yield return null;
            }

            mover.localPosition = Vector3.zero;
        }

        private void MoveCamera(Transform view)
        {
            cameraRig.SetPositionAndRotation(view.position, view.rotation);
        }
    }
}