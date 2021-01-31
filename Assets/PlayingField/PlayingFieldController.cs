using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayingFieldController : MonoBehaviour {

    public Transform GroupStartPosition;
    public Transform WaitingArea;

    [Serializable]
    public class PlayerPresets
    {
        public Vector3 GroupStartPosition;
        public Vector3 NextGroupPosition;
        public Vector3 CameraPosition;
    }

    public float fallDelay = 1;
    public float moveDelay = .05f;
    public float rotateDelay = .3f;

    [SerializeField] private int widthPerPlayer = 11;
    [SerializeField] private GameObject leftBorder;
    [SerializeField] private GameObject rightBorder;
    [SerializeField] private GameObject bottomBorder;
    [SerializeField] private GameObject grid;

    [SerializeField] private GameObject[] Groups;

    public enum Status_enum { IDLE, BLOCKS_FALLING, DELETING_LINES, FILLING_LINES }
    public Status_enum FieldStatus = Status_enum.IDLE;

    //private ActiveGroupController theActiveGroup;
    //public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }
    //private ActiveGroupController theNextGroup;

    private int blocksRequiredPerLine;

    [Tooltip("Show this text when game is idle and no players have joined")]
    [SerializeField] private UnityEngine.UI.Text IdleText;

    private List<PlayerController> Players = new List<PlayerController>();

    void Start () {
        blocksRequiredPerLine = Mathf.RoundToInt(rightBorder.transform.position.x - leftBorder.transform.position.x)-1;
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
        SetPlayerPositions(playernr, p);
        Players.Add(p);
        AdjustFieldWidth();
        return playernr;
    }

    /**
     * provides the new player with a unique starting position
     */
    private void SetPlayerPositions(int playernr, PlayerController p)
    {
        //this is how much each new player needs to shift to the right:
        Vector3 displacement = playernr * widthPerPlayer * Vector3.right;

        Transform newWaitingArea = null;
        if (playernr == 0) newWaitingArea = this.WaitingArea;
        else newWaitingArea = Instantiate(WaitingArea, WaitingArea.position + displacement, Quaternion.identity);

        p.GroupStartPosition = Instantiate(this.GroupStartPosition, this.GroupStartPosition.position + displacement, Quaternion.identity) ;
        p.NextGroupPosition = newWaitingArea.Find("NextGroupPosition");
    }

    private void AdjustFieldWidth()
    {
        //position right border
        this.rightBorder.transform.position = this.leftBorder.transform.position + Vector3.right * widthPerPlayer * Players.Count + Vector3.right;

        //position bottom border:
        this.bottomBorder.transform.localPosition = new Vector3(this.leftBorder.transform.position.x + .5f + widthPerPlayer * Players.Count / 2f, this.bottomBorder.transform.localPosition.y, this.bottomBorder.transform.localPosition.z);
        this.bottomBorder.transform.localScale = new Vector3(widthPerPlayer * Players.Count, 1, 1);


        //adjust background grid to show correct number of tiles
        float fieldHeight = this.grid.transform.localScale.y;
        this.grid.transform.localScale = new Vector3(widthPerPlayer * Players.Count, fieldHeight, 1);
        this.grid.transform.localPosition = new Vector3(this.leftBorder.transform.position.x + .5f + widthPerPlayer * Players.Count / 2f, this.grid.transform.localPosition.y, this.grid.transform.localPosition.z);
        Material m = this.grid.GetComponent<MeshRenderer>().material;
        m.SetVector("Tiling", new Vector4(widthPerPlayer * Players.Count, fieldHeight, 0, 0));

        //more players, equals bigger lines to fill
        blocksRequiredPerLine = widthPerPlayer * Players.Count;
    }

    //called by the group after it hit an obstruction and was fixed
    //(time to create a new group at the top)
    public void GroupWasFixed(int playerNr, List<GameObject> blocks)
    {
        PlayerController p = Players[playerNr];
        Reparent(blocks);
        CheckForCompleteLines(blocks);
        p.ActivateNextGroup(this.CreateNewNextGroup(p));
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

    public ActiveGroupController CreateNewNextGroup(PlayerController p)
    {
        int newIndex = UnityEngine.Random.Range(0, Groups.Length);
        GameObject newGroup = (GameObject)Instantiate(Groups[newIndex], p.NextGroupPosition.position, p.NextGroupPosition.rotation);
        ActiveGroupController agc = newGroup.GetComponent<ActiveGroupController>();
        agc.SetPlayer(p.playerNr, p.MyColor);
        return agc;
    }

    public void GameOver()
    {
        Debug.Log("GameOver!");
        Destroy(this); //will destroy the script, not the gameobject.
    }
}
