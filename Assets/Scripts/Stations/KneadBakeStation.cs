using System.Collections;
using BreadBowl.Dough;
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

        private void Start()
        {
            if (cameraRig == null)
            {
                cameraRig = Camera.main.transform;
            }

            kneadCanvas.SetActive(true);
            bakeCanvas.SetActive(false);
            showcaseCanvas.SetActive(false);
            showcase.enabled = false;
            breadBaker.gameObject.SetActive(false);
            MoveCamera(kneadView);
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
            yield return MoveOnto(tin, pullOutPoint, slideDuration);
            yield return MoveOnto(breadBaker.transform, showcase.transform, liftDuration);

            MoveCamera(showcaseView);
            showcase.enabled = true;
            showcaseCanvas.SetActive(true);
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
        }


        private void MoveCamera(Transform view)
        {
            cameraRig.SetPositionAndRotation(view.position, view.rotation);
        }
    }
}

