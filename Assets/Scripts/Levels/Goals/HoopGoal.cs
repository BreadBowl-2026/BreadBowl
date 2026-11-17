using BreadBowl.Bowling;
using BreadBowl.Levels;
using UnityEngine;

public class HoopGoal : LevelGoal
{
    [SerializeField] private int hoops = 2;
    public int hoopsJumpedThrough = 0;
    public override bool IsComplete(Loaf loaf)
    {
        if (hoopsJumpedThrough >= hoops)
        {
            return true;
        } else
        {
            hoopsJumpedThrough = 0;
            return false;
        }
    }

    public void HoopJumpedThrough()
    {
        hoopsJumpedThrough++;
        Debug.Log("another hoop " + hoopsJumpedThrough);
    }

}
