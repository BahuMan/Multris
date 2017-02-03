using UnityEngine;
using System.Collections.Generic;

public class ActiveGroupController : MonoBehaviour {

    public float fallDelay = 128;

    private bool inWaitingRoom = true;
    private float lastFall = 0;
    private Quaternion Rotation90Degrees = Quaternion.Euler(0, 0, 90);
    private Quaternion RotationMinus90Degrees = Quaternion.Euler(0, 0, -90);
    public PlayingFieldController myPlayingField;

    // Use this for initialization
    void Start () {
        Debug.Log("ActiveGroupController.Start");
        ChooseRandomColor();
	}

    // Update is called once per frame
    void Update() {

        if (inWaitingRoom) return;

        if (lastFall + fallDelay < Time.time)
        {
            if (!MoveOneDown())
            {
                ConvertToFixed();
            }
            lastFall = Time.time;
        }
    }

    //when a group is created, it should be added to a playingfield before it can start falling
    public void SetPlayingField(PlayingFieldController thePlayingField, float fallingDelay)
    {
        Debug.Log("SetPlayingField");
        inWaitingRoom = false;
        this.fallDelay = fallingDelay;
        this.myPlayingField = thePlayingField;
    }

    public void ConvertToFixed()
    {
        Debug.Log("ConvertToFixed");
        //create a list of children, so we can reset parent and feed list to playingfield
        List<GameObject> blocks = new List<GameObject>(transform.childCount);
        foreach (Transform child in transform)
        {
            child.GetComponent<BlockController>().wasFixed();
            blocks.Add(child.gameObject);
        }
        transform.DetachChildren();
        
        myPlayingField.GroupWasFixed(blocks);
        Destroy(this.gameObject);
    }

    //returns false if the block could no longer move
    public bool MoveOneDown()
    {
        //Debug.Log("MoveOneDown");
        return AttemptMove(new Vector3(0, -1f, 0));
    }

    //returns false if the block could no longer move
    public bool MoveOneLeft()
    {
        //Debug.Log("MoveOneLeft");
        return AttemptMove(new Vector3(-1f, 0, 0));
    }

    //returns false if the block could no longer move
    public bool MoveOneRight()
    {
        //Debug.Log("MoveOneRight");
        return AttemptMove(new Vector3(1f, 0, 0));
    }

    //returns false if the block could no longer move
    private bool AttemptMove(Vector3 moveDirection)
    {
        transform.position += moveDirection;

        if (isColliding())
        {
            //undo move
            transform.position -= moveDirection;
            return false;
        }

        return true;
    }
    //returns true if any of the blocks is currently colliding with the level or existing blocks
    //this will be called at each move/rotation to make sure the move/rotation is possible
    public bool isColliding()
    {
        foreach (Transform child in transform)
        {
            //we're checking against a cube smaller than the existing blocks, to prevent adjacent blocks from triggering collisions
            Collider[] colliders = Physics.OverlapBox(child.position, new Vector3(.4f, .4f, .4f));
            if (colliders.Length > 0)
            {
                //Debug.Log("Hit something!");
                foreach (Collider col in colliders)
                {
                    Debug.Log("block " + child.gameObject.name + " hit " + col.gameObject.name);
                }
                return true;
            }
        }
        return false;
    }

    public void Drop()
    {
        Debug.Log("Drop");
        while (MoveOneDown())
        {
            //we're moving down one line at the time
        }
        ConvertToFixed();
    }

    //returns false if rotation was impossible (due to collision in new position)
    virtual public bool RotateClockWise()
    {
        //Debug.Log("RotateClockWise");
        Quaternion undo = this.transform.rotation;
        this.transform.rotation *= this.RotationMinus90Degrees;
        if (isColliding())
        {
            this.transform.rotation = undo;
            return false;
        }
        return true;
    }

    //returns false if rotation was impossible (due to collision in new position)
    virtual public bool RotateCounterClockWise()
    {
        //Debug.Log("RotateCounterClockWise");
        Quaternion undo = this.transform.rotation;
        this.transform.rotation *= this.Rotation90Degrees;
        if (isColliding())
        {
            this.transform.rotation = undo;
            return false;
        }
        return true;
    }

    public void ChooseRandomColor()
    {
        Color clr = new Color(Random.Range(0, 5) * (1f/4f), Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f)); //Random.ColorHSV(0, 1, .8f, 1, .5f, 1);

        //color must be bright enough:
        while (clr.grayscale < .5f)
        {
            clr = new Color(Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f)); //Random.ColorHSV(0, 1, .8f, 1, .5f, 1);
        }

        //set color for all blocks in group
        foreach (Transform block in transform)
        {
            block.GetComponent<BlockController>().SetColor(clr);
        }
    }
}
