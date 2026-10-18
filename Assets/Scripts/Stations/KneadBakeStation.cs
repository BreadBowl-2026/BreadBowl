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

        [Header("Camera")]
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform kneadView;
        [SerializeField] private Transform bakeView;  

        private void OnEnable()
        {
            kneadStage.Finished += HandleKneadFinished;
        }

        private void OnDisable()
        {
            kneadStage.Finished -= HandleKneadFinished;
        }

        private void Start()
        {
            if (cameraRig == null)
            {
                cameraRig = Camera.main.transform;
            }

            kneadCanvas.SetActive(true);
            bakeCanvas.SetActive(false);
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

        private void MoveCamera(Transform view)
        {
            cameraRig.SetPositionAndRotation(view.position, view.rotation);
        }
    }
}

