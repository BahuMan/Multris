using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController: MonoBehaviour
{

    public enum PlayerStatusEnum { IDLE, JOINING, PLAYING, GAMEOVER }
    public PlayerStatusEnum PlayerStatus = PlayerStatusEnum.IDLE;

    public Transform GroupStartPosition;
    public Transform NextGroupPosition;

    public int points;
    public int level = 0;
    public int playerNr = -1;
    public float fallDelay = 1;
    public Color MyColor;

    private PlayingFieldController playingField;
    private ActiveGroupController theActiveGroup;
    public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }


    private ActiveGroupController theNextGroup;

    public UnityEvent OnGameOver;


    public void JoinGame()
    {
        points = 0;
        level = 0;
        fallDelay = 1;
        MyColor = ChooseRandomColor();

        playingField = Object.FindObjectOfType<PlayingFieldController>();
        this.playerNr = playingField.RegisterNewPlayer(this);
        SetNextGroup(playingField.CreateNewNextGroup(this));
        ActivateNextGroup(playingField.CreateNewNextGroup(this));

        this.PlayerStatus = PlayerStatusEnum.PLAYING;
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
        this.PlayerStatus = PlayerStatusEnum.GAMEOVER;
        this.OnGameOver.Invoke();
    }

    private Color ChooseRandomColor()
    {
        Color clr = new Color(Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f)); //Random.ColorHSV(0, 1, .8f, 1, .5f, 1);

        //color must be bright enough:
        while (clr.grayscale < .5f)
        {
            clr = new Color(Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f), Random.Range(0, 5) * (1f / 4f)); //Random.ColorHSV(0, 1, .8f, 1, .5f, 1);
        }

        return clr;
    }
    public void MoveOneRight()
    {
        theActiveGroup.MoveOneRight();
    }

    public void MoveOneLeft()
    {
        theActiveGroup.MoveOneLeft();
    }

    public void MoveOneDown()
    {
        theActiveGroup.MoveOneDown();
    }

    public void Drop()
    {
        theActiveGroup.Drop();
    }

    public void RotateClockWise()
    {
        theActiveGroup.RotateClockWise();
    }

    public void RotateCounterClockWise()
    {
        theActiveGroup.RotateCounterClockWise();
    }

}
