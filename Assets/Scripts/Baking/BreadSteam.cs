using System.Collections;
using UnityEngine;

namespace BreadBowl
{
    [RequireComponent(typeof(ParticleSystem))]
    public class BreadSteam : MonoBehaviour
    {
        [SerializeField] private int initialBurst = 3;

        private ParticleSystem steam;
        private float baseRate;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            // Use the Particle System already attached
            steam = GetComponent<ParticleSystem>();

            var main = steam.main;
            main.playOnAwake = false;
            main.loop = true;

            baseRate = steam.emission.rateOverTime.constant;

            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        public void Play()
        {
            if (steam == null) return;

            StopFade();
            SetRate(baseRate);

            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            steam.Play();

            if (initialBurst > 0)
                steam.Emit(initialBurst);
        }

        public void Stop()
        {
            if (steam == null) return;

            StopFade();

            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        public void StopAndClear()
        {
            if (steam == null) return;

            StopFade();

            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        public void FadeOut(float duration)
        {
            if (steam == null) return;

            StopFade();

            if (duration <= 0f)
            {
                Stop();
                return;
            }

            fadeRoutine = StartCoroutine(FadeRoutine(duration));
        }

        private IEnumerator FadeRoutine(float duration)
        {
            float elapsed = 0f;
            float startRate = steam.emission.rateOverTime.constant;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(elapsed / duration)
                );

                SetRate(Mathf.Lerp(startRate, 0f, t));

                yield return null;
            }

            SetRate(0f);

            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );

            fadeRoutine = null;
        }

        private void StopFade()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
        }

        private void SetRate(float rate)
        {
            var emission = steam.emission;
            emission.rateOverTime = rate;
        }
    }
}
