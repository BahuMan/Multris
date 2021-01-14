using UnityEngine;

public class PlayerController: MonoBehaviour
{
    public Transform GroupStartPosition;
    public Transform NextGroupPosition;

    public int points;
    public int level = 0;
    public int playerNr = -1;
    public float fallDelay = 1;
    public float moveDelay = .05f;
    public float rotateDelay = .3f;

    private PlayingFieldController playingField;
    private ActiveGroupController theActiveGroup;
    public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }
    private ActiveGroupController theNextGroup;

    private float nextMove = 0;
    private float nextRotate = 0;

    void Start()
    {
        points = 0;
        level = 0;
        fallDelay = 1;

        playingField = Object.FindObjectOfType<PlayingFieldController>();
        this.playerNr = playingField.RegisterNewPlayer(this);
        SetNextGroup(playingField.CreateNewNextGroup());
        ActivateNextGroup(playingField.CreateNewNextGroup());

    }

    public void SetNextGroup(ActiveGroupController ng)
    {
        theNextGroup = ng;
    }

    /**
     * -the group CURRENTLY in the waiting area will be activated.
     * - nextNextGroup is the group that will be put in the waiting area
     * 
     * @returns FALSE if game over (newly activated group collided immediately within the playing field)
     */
    public bool ActivateNextGroup(ActiveGroupController nextNextGroup)
    {
        Debug.Log("Player.ActivateNextGroup");

        //the one to activate was already created:
        theActiveGroup = theNextGroup;
        theActiveGroup.transform.position = GroupStartPosition.position;
        theActiveGroup.transform.rotation = GroupStartPosition.rotation;

        if (theActiveGroup.isColliding())
        {
            Destroy(theActiveGroup.gameObject);
            GameOver();
            return false;
        }

        //now create the one in the waiting area:
        theNextGroup = nextNextGroup;

        //initialization:
        theActiveGroup.StartFalling(this.playingField, this.playerNr, this.fallDelay);
        return theActiveGroup;
    }

    public void GameOver()
    {
        Debug.Log("GameOver!");
        Destroy(this); //will destroy the script, not the gameobject.
    }

    void Update()
    {
        if (nextMove < Time.time)
        {
            if (Input.GetAxis("Horizontal") > .3f)
            {
                theActiveGroup.MoveOneRight();
                nextMove = Time.time + moveDelay / Input.GetAxis("Horizontal");
            }

            if (Input.GetAxis("Horizontal") < -.3f)
            {
                theActiveGroup.MoveOneLeft();
                nextMove = Time.time + moveDelay / -Input.GetAxis("Horizontal");
            }

            if (Input.GetAxis("Vertical") < -.3f)
            {
                if (!theActiveGroup.MoveOneDown())
                {
                    theActiveGroup.ConvertToFixed();
                    Debug.Log("Fixed. This script should be destroyed");
                }
                nextMove = Time.time + moveDelay / -Input.GetAxis("Vertical");
            }
        }
        if (nextRotate < Time.time)
        {
            if (Input.GetButton("RotateClockWise"))
            {
                theActiveGroup.RotateClockWise();
                nextRotate = Time.time + rotateDelay;
            }
            if (Input.GetButton("RotateCounterClockWise"))
            {
                theActiveGroup.RotateCounterClockWise();
                nextRotate = Time.time + rotateDelay;
            }
        }
        if (Input.GetButtonDown("Drop"))
        {
            theActiveGroup.Drop();
            //lastKeyboard = Time.time;
        }

    }

}
