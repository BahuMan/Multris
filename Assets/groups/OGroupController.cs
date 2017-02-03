using UnityEngine;
using System.Collections.Generic;

public class OGroupController: ActiveGroupController
{

    //the (square) O-Group doesn't rotate
    override public bool RotateClockWise()
    {
        return true;
    }

    //the (square) O-Group doesn't rotate
    override public bool RotateCounterClockWise()
    {
        return true;
    }


}
