using UnityEngine;

public class Hoop : MonoBehaviour
{
    [SerializeField] private HoopGoal parent;
    [SerializeField] private ParticleSystem vfx;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " passed through");
        parent.HoopJumpedThrough();
        vfx.Play();
    }
}
