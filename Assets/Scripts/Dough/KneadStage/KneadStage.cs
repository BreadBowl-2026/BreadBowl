using System.Collections;
using UnityEngine;

namespace BreadBowl.Dough
{
    [RequireComponent(typeof(KneadableDough), typeof(KneadInput))]
    public class KneadStage : MonoBehaviour
    {
        private KneadableDough dough;
        private KneadInput input;
        private KneadResult result;
        private bool finishing;

        public KneadResult Result => result;

        public event System.Action<KneadResult> Finished;

        private void Awake()
        {
            dough = GetComponent<KneadableDough>();
            input = GetComponent<KneadInput>();
        }

        public void Finish()
        {
            if (finishing)
            {
                return;
            }

            finishing = true;
            StartCoroutine(FinishRoutine());
        }

        private IEnumerator FinishRoutine()
        {
            input.enabled = false;

            yield return new WaitUntil(() => !dough.IsFolding);

            result = dough.CreateResult();
            Debug.Log($"Knead finished. Under {result.UnderFraction:P0} | " +
            $"Well {result.WellFraction:P0} | Over {result.OverFraction:P0}", this);
            Finished?.Invoke(result);
        }
    }
}

