using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [SerializeField] private BlockController thisBlock;

    private void Start()
    {
        thisBlock.IsBlockDestructionDone += ThisBlock_IsBlockDestructionDone;
    }

    //will get called every frame until you return <true>
    private bool ThisBlock_IsBlockDestructionDone()
    {
        Debug.Log("BOOM");
        return true;
    }

}
