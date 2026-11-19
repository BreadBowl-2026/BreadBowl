using BreadBowl.Bowling;
using BreadBowl.Dough;
using TMPro;
using UnityEngine;

namespace BreadBowl
{
    public class LoafStatsUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text underText;
        [SerializeField] private TMP_Text wellText;
        [SerializeField] private TMP_Text overText;

        public void Show(Loaf loaf)
        {
            KneadResult knead = loaf.Knead;

            if (knead == null)
            {
                underText.text = "Under --";
                wellText.text = "Well --";
                overText.text = "Over --";
                return;
            }

            underText.text = $"Under {Percent(knead.UnderFraction)}";
            wellText.text = $"Well {Percent(knead.WellFraction)}";
            overText.text = $"Over {Percent(knead.OverFraction)}";
        }

        private static string Percent(float fraction)
        {
            return $"{Mathf.RoundToInt(fraction * 100f)}%";
        }
    }
}


