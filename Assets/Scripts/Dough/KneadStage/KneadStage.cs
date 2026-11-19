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

        public void Restart()
        {
            finishing = false;
            result = null;
            input.enabled = true;
            dough.ResetKnead();
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

        [ContextMenu("Log Result Samples")]
        private void LogResultSamples()
        {
            if (result == null)
            {
                Debug.LogWarning("Finish kneading before sampling result", this);
                return;
            }

            int res = result.Resolution;
            int cellX = res / 2;
            int cellY = res / 2;

            Vector2 onCell = new Vector2((cellX + 0.5f) / res, (cellY + 0.5f) / res);
            Vector2 betweenCells = new Vector2((cellX + 1f) / res, (cellY + 0.5f) / res);
            float average = (result.Get(cellX, cellY) + result.Get(cellX + 1, cellY)) * 0.5f;

            // confirms that the sample from a uv directly in the center of a cell returns that value and the 
            // value between cells returns the average kneadedness of those cells
            Debug.Log($"On cell ({cellX},{cellY}) Get {result.Get(cellX, cellY):F3} | Sample {result.Sample(onCell):F3}", this);
            Debug.Log($"Between ({cellX},{cellY}) and ({cellX + 1},{cellY}) average {average:F3} | Sample {result.Sample(betweenCells):F3}", this);
        }
    }
}

