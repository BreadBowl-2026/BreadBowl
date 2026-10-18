using BreadBowl.Dough;
using UnityEngine;

namespace BreadBowl
{
    public class KneadBakeStation : MonoBehaviour
    {
        [SerializeField] private KneadStage kneadStage;
        [SerializeField] private BreadBaker breadBaker;
        [SerializeField] private GameObject kneadArea;
        [SerializeField] private GameObject bakeArea;

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

            kneadArea.SetActive(true);
            bakeArea.SetActive(false);
            MoveCamera(kneadView);
        }

        private void HandleKneadFinished(KneadResult result)
        {
            breadBaker.SetKneadResult(result);
            kneadArea.SetActive(false);
            bakeArea.SetActive(true);
            MoveCamera(bakeView);
        }

        private void MoveCamera(Transform view)
        {
            cameraRig.SetPositionAndRotation(view.position, view.rotation);
        }
    }
}

