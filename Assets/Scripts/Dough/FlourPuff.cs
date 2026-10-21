using UnityEngine;

namespace BreadBowl.Dough
{
    public class FlourPuff : MonoBehaviour
    {
        [Header("Particles Per Puff")]
        [SerializeField] private int hazeCount = 7;
        [SerializeField] private int dustCount = 20;
        [SerializeField] private int grainsCount = 8;

        [SerializeField] private float minInterval = 0.25f;

        private ParticleSystem haze;
        private ParticleSystem dust;
        private ParticleSystem grains;

        private float lastPuffTime = float.NegativeInfinity;

        private void Awake()
        {
            haze = FindParticle("FlourHaze");
            dust = FindParticle("FlourDust");
            grains = FindParticle("FlourGrains");

            Configure(haze);
            Configure(dust);
            Configure(grains);
        }

        private ParticleSystem FindParticle(string childName)
        {
            Transform child = transform.Find(childName);

            if (child == null)
            {
                Debug.LogWarning(
                    $"FlourPuff: {childName} not found.",
                    this
                );
                return null;
            }

            return child.GetComponentInChildren<ParticleSystem>(true);
        }

        private void Configure(ParticleSystem ps)
        {
            if (ps == null) return;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;

            ps.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        public void Puff(float intensity = 1f)
        {
            if (intensity <= 0f) return;

            if (Time.time - lastPuffTime < minInterval)
                return;

            lastPuffTime = Time.time;

            Emit(haze, Mathf.RoundToInt(hazeCount * intensity));
            Emit(dust, Mathf.RoundToInt(dustCount * intensity));
            Emit(grains, Mathf.RoundToInt(grainsCount * intensity));
        }

        public void Puff(Vector3 position, float intensity = 1f)
        {
            transform.position = position;
            Puff(intensity);
        }

        private void Emit(ParticleSystem ps, int count)
        {
            if (ps == null || count <= 0) return;

            if (!ps.isPlaying)
                ps.Play();

            ps.Emit(count);
        }
    }
}
