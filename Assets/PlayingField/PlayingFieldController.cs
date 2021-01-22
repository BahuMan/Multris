using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayingFieldController : MonoBehaviour {

    public Transform GroupStartPosition;
    public Transform NextGroupPosition;

    public int points;
    public int level = 0;
    public float fallDelay = 1;
    public float moveDelay = .05f;
    public float rotateDelay = .3f;

    [SerializeField] private int widthPerPlayer = 11;
    [SerializeField] private GameObject leftBorder;
    [SerializeField] private GameObject rightBorder;
    [SerializeField] private GameObject[] Groups;

    public enum Status_enum { IDLE, BLOCKS_FALLING, DELETING_LINES, FILLING_LINES }
    public Status_enum FieldStatus = Status_enum.IDLE;

    //private ActiveGroupController theActiveGroup;
    //public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }
    //private ActiveGroupController theNextGroup;

    private int blocksRequiredPerLine;
    private float nextMove = 0;
    private float nextRotate = 0;

    [Tooltip("Show this text when game is idle and no players have joined")]
    [SerializeField] private UnityEngine.UI.Text IdleText;

    private List<PlayerController> Players = new List<PlayerController>();

    void Start () {
        blocksRequiredPerLine = Mathf.RoundToInt(rightBorder.transform.position.x - leftBorder.transform.position.x)-1;
        Debug.Log("blocksRequiredPerLine = " + blocksRequiredPerLine);
        points = 0;
        level = 0;
        fallDelay = 1;
	}

    private void Update()
    {
        switch (FieldStatus) {
            case Status_enum.IDLE: IdleCheckForPlayers(); break;
        }
    }

    private void IdleCheckForPlayers()
    {
        if (this.Players.Count == 0)
        {
            IdleText.enabled = true; //show text when zero players
        }
        else
        {
            this.FieldStatus = Status_enum.BLOCKS_FALLING;
            IdleText.enabled = false;
        }
    }

    public int RegisterNewPlayer(PlayerController p)
    {
        int playernr = Players.Count;
        Players.Add(p);
        return playernr;
    }
	
    //called by the group after it hit an obstruction and was fixed
    //(time to create a new group at the top)
    public void GroupWasFixed(int playerNr, List<GameObject> blocks)
    {
        PlayerController p = Players[playerNr];
        Reparent(blocks);
        CheckForCompleteLines(blocks);
        p.ActivateNextGroup(this.CreateNewNextGroup());
    }

    private void Reparent(List<GameObject> blocks)
    {
        foreach (GameObject blk in blocks)
        {
            blk.transform.parent = transform;
        }
    }

    //I only need to check for complete lines on lines where blocks were added recently
    public void CheckForCompleteLines(List<GameObject> blocks)
    {
        Debug.Log("CheckForCompleteLines");
        float minY = 0;
        float maxY = 0;
        foreach (GameObject block in blocks)
        {
            if (minY == 0 || block.transform.position.y < minY)
            {
                minY = block.transform.position.y;
            }
            if (maxY == 0 || block.transform.position.y > maxY)
            {
                maxY = block.transform.position.y;
            }
        }
        float currentY = minY;
        while (currentY < maxY+1f)
        {
            CheckForSingleCompleteLine(ref currentY);
        }
    }

    //returns true if a complete line was found. This means the same Y coordinate should be checked again
    public void CheckForSingleCompleteLine(ref float LineY)
    {
        Vector3 origin = new Vector3(this.leftBorder.transform.position.x, LineY, 0);
        Vector3 direction = new Vector3(1f, 0, 0);
        float maxDistance = this.rightBorder.transform.position.x - this.leftBorder.transform.position.x;
        RaycastHit[] allHits = Physics.SphereCastAll(origin, .1f, direction, maxDistance);
        Debug.Log("for Y = " + LineY + ", found " + allHits.Length + " hits");
        if (allHits.Length == blocksRequiredPerLine)
        {
            foreach (RaycastHit hit in allHits)
            {
                hit.collider.gameObject.GetComponent<BlockController>().removeLine();
            }
            BringOtherBlocksDown(LineY);
        }
        else
        {
            LineY++; //since this parameter is passed by reference, the top function will now check the lines higher-up
        }
    }

    private void BringOtherBlocksDown(float LineY)
    {
        foreach (Transform b in transform)
        {
            if (b.transform.position.y > LineY)
            {
                BlockController block = b.gameObject.GetComponent<BlockController>();
                block?.MoveOneDown();
            }
        }
    }

    public ActiveGroupController CreateNewNextGroup()
    {
        int newIndex = UnityEngine.Random.Range(0, Groups.Length);
        GameObject newGroup = (GameObject)Instantiate(Groups[newIndex], this.NextGroupPosition.position, this.NextGroupPosition.rotation);
        return newGroup.GetComponent<ActiveGroupController>();
    }

    public void GameOver()
    {
        Debug.Log("GameOver!");
        Destroy(this); //will destroy the script, not the gameobject.
    }
}
