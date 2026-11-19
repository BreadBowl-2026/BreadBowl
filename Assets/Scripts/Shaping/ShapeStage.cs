using TMPro;
using UnityEngine;

namespace BreadBowl
{
    public class ShapeStage : MonoBehaviour
    {
        [SerializeField] private BreadShapeChoice[] choices;
        [SerializeField] private TMP_Text shapeLabel;

        private BreadShapeChoice selected;

        public event System.Action<BreadShape> Finished;

        private void OnEnable()
        {
            foreach (BreadShapeChoice choice in choices)
            {
                choice.Clicked += Select;
            }
        }

        private void OnDisable()
        {
            foreach (BreadShapeChoice choice in choices)
            {
                choice.Clicked -= Select;
            }
        }

        public void Restart()
        {
            if (choices == null || choices.Length == 0)
            {
                Debug.LogWarning("No bread shape choices available", this);
                return;
            }

            Select(choices[0]);
        }

        public void Confirm()
        {
            Debug.Log("Bread shape choice: " + selected.Shape.DisplayName);

            if (selected == null)
            {
                Debug.LogWarning("No bread shape selected", this);
                return;
            }

            Finished?.Invoke(selected.Shape);
        }

        private void Select(BreadShapeChoice choice)
        {
            selected = choice;

            foreach (BreadShapeChoice other in choices)
            {
                other.SetSelected(other == choice);
            }

            shapeLabel.text = choice.Shape.DisplayName;
        }
    }
}

