using UnityEngine;

public class BreadShapeChoice : MonoBehaviour
{
    public enum BreadShape
    {
        Sourdough,
        Baguette
    }

    [SerializeField] private BreadShape shape;
    public BreadShape Shape => shape;

    [SerializeField] private Renderer shapeRenderer;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material selectedMaterial;


    private void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        if (shapeRenderer != null && selectedMaterial != null)
        {
            shapeRenderer.material = selectedMaterial;
        }
    }

    public void Deselect()
    {
        if (shapeRenderer != null && ghostMaterial != null)
        {
            shapeRenderer.material = ghostMaterial;
        }
    }
}
