using BreadBowl.Bowling;
using BreadBowl.Levels;
using UnityEngine;

public class HoopGoal : LevelGoal
{
    [SerializeField] private int hoops = 2;
    private int hoopsJumpedThrough = 0;
    public override bool IsComplete(Loaf loaf)
    {
        return hoopsJumpedThrough > hoops;
    }

    public void HoopJumpedThrough()
    {
        hoopsJumpedThrough++;
        Debug.Log("another hoop " + hoopsJumpedThrough);
    }

}
