using UnityEngine;

public class BreadShapeChoice : MonoBehaviour
{
    public enum BreadShape
    {
        Sourdough,
        Baguette
    }

    [SerializeField] private BreadShape shape;

    [Header("Visual")]
    [SerializeField] private GameObject hoverIndicator;

    public BreadShape Shape => shape;

    public void SetHovered(bool hovered)
    {
        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(hovered);
        }
    }

    public void Select()
    {
        Debug.Log("Selected bread shape: " + shape);
    }
}
