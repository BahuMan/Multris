using UnityEngine;
using System.Collections.Generic;

public class PlayingFieldController : MonoBehaviour {

    public Transform GroupStartPosition;
    public Transform NextGroupPosition;

    public int points;
    public int level = 0;
    public float fallDelay = 1;
    public float keyboardDelay = .1f;

    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject[] Groups;

    private ActiveGroupController theActiveGroup;
    public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }
    private ActiveGroupController theNextGroup;

    private int blocksRequiredPerLine;
    private float lastKeyboard = 0;

    // Use this for initialization
    void Start () {
        blocksRequiredPerLine = Mathf.RoundToInt(rightBorder.transform.position.x - leftBorder.transform.position.x)-1;
        Debug.Log("blocksRequiredPerLine = " + blocksRequiredPerLine);
        points = 0;
        level = 0;
        fallDelay = 1;

        theNextGroup = CreateNewNextGroup();
        theActiveGroup = MakeNextGroupActive();

	}
	
	// Update is called once per frame
	void Update () {
        if (lastKeyboard + keyboardDelay < Time.time)
        {
            if (Input.GetAxis("Horizontal") > .5f)
            {
                theActiveGroup.MoveOneRight();
                lastKeyboard = Time.time;
            }

            if (Input.GetAxis("Horizontal") < -.5f)
            {
                theActiveGroup.MoveOneLeft();
                lastKeyboard = Time.time;
            }

            if (Input.GetAxis("Vertical") < -.5f)
            {
                if (!theActiveGroup.MoveOneDown())
                {
                    theActiveGroup.ConvertToFixed();
                    Debug.Log("Fixed. This script should be destroyed");
                }
                lastKeyboard = Time.time;
            }
            if (Input.GetButtonDown("Drop"))
            {
                theActiveGroup.Drop();
                //lastKeyboard = Time.time;
            }
            if (Input.GetButton("RotateClockWise"))
            {
                theActiveGroup.RotateClockWise();
                lastKeyboard = Time.time;
            }
            if (Input.GetButton("RotateCounterClockWise"))
            {
                theActiveGroup.RotateCounterClockWise();
                lastKeyboard = Time.time;
            }
        }
    }

    //called by the group after it hit an obstruction and was fixed
    //(time to create a new group at the top)
    public void GroupWasFixed(List<GameObject> blocks)
    {
        Reparent(blocks);
        CheckForCompleteLines(blocks);
        MakeNextGroupActive();
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
                block.MoveOneDown();
            }
        }
    }

    public ActiveGroupController MakeNextGroupActive()
    {
        Debug.Log("PutNextGroupInField");

        //the one to return was already created:
        theActiveGroup = theNextGroup;
        theActiveGroup.transform.position = GroupStartPosition.position;
        theActiveGroup.transform.rotation = GroupStartPosition.rotation;

        if (theActiveGroup.isColliding())
        {
            Destroy(theActiveGroup.gameObject);
            GameOver();
            return null;
        }

        //now create the one in the waiting area:
        theNextGroup = CreateNewNextGroup();

        //initialization:
        theActiveGroup.SetPlayingField(this, this.fallDelay);
        return theActiveGroup;
    }

    private ActiveGroupController CreateNewNextGroup()
    {
        int newIndex = Random.Range(0, Groups.Length);
        GameObject newGroup = (GameObject)Instantiate(Groups[newIndex], this.NextGroupPosition.position, this.NextGroupPosition.rotation);
        theNextGroup = newGroup.GetComponent<ActiveGroupController>();
        return theNextGroup;
    }

    public void GameOver()
    {
        Debug.Log("GameOver!");
        Destroy(this); //will destroy the script, not the gameobject.
    }
}
