using TMPro;
using UnityEngine;

namespace BreadBowl.Levels
{
    public class ResultRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        public void Show(ResultStat stat)
        {
            labelText.text = stat.Label;
            valueText.text = stat.Value;
        }
    }
}

