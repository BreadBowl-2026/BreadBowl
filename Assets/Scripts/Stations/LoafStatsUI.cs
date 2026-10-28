using BreadBowl.Bowling;
using BreadBowl.Dough;
using TMPro;
using UnityEngine;

namespace BreadBowl
{
    public class LoafStatsUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statsText;

        public void Show(Loaf loaf)
        {
            KneadResult knead = loaf.Knead;

            if (knead == null)
            {
                statsText.text = "Kneadedness:\n\tUnknown";
            }

            statsText.text = "Kneadedness:\n" +
                $"\tUnder {Percent(knead.UnderFraction)}\n" +
                $"\tWell {Percent(knead.WellFraction)}\n" + 
                $"\tOver {Percent(knead.OverFraction)}\n";
        }

        private static string Percent(float fraction)
        {
            return $"{Mathf.RoundToInt(fraction * 100f)}%";
        }
    }
}

