using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayingFieldController : MonoBehaviour {

    public Transform GroupStartPosition;
    public Transform WaitingArea;

    public float fallDelay = 1;
    public float moveDelay = .05f;
    public float rotateDelay = .3f;

    [SerializeField] private int widthPerPlayer = 11;
    [SerializeField] private GameObject leftBorder;
    [SerializeField] private GameObject rightBorder;
    [SerializeField] private GameObject bottomBorder;
    [SerializeField] private GameObject grid;
    [SerializeField] private UnityEngine.UI.Text statusText;

    [SerializeField] private GameObject[] Groups;

    public enum Status_enum { IDLE, BLOCKS_FALLING, LINES_CHECK, LINES_DELETE, LINES_FILL, BLOCK_CREATE }
    public Status_enum FieldStatus = Status_enum.IDLE;

    //private ActiveGroupController theActiveGroup;
    //public ActiveGroupController CurrentlyActiveGroup { get { return theActiveGroup; } }
    //private ActiveGroupController theNextGroup;

    private int blocksRequiredPerLine;

    [Tooltip("Show this text when game is idle and no players have joined")]
    [SerializeField] private UnityEngine.UI.Text IdleText;

    private List<PlayerController> Players = new List<PlayerController>();
    public int PlayerCount { get => Players.Count; }

    private List<ActiveGroupController> FallingGroups = new List<ActiveGroupController>();
    private List<BlockController> BlocksToMoveDown = new List<BlockController>();

    void Start () {
        blocksRequiredPerLine = Mathf.RoundToInt(rightBorder.transform.position.x - leftBorder.transform.position.x)-1;
	}

    private void Update()
    {
        switch (FieldStatus)
        {
            case Status_enum.IDLE:
                this.statusText.text = "IDLE";
                IdleCheckForPlayers();
                break;
            case Status_enum.BLOCK_CREATE:
                this.statusText.text = "BLOCK_CREATE";
                BlockCreateForPlayer();
                break;
            case Status_enum.BLOCKS_FALLING:
                this.statusText.text = "BLOCKS_FALLING";
                ActiveGroupFall();
                break;
            case Status_enum.LINES_CHECK:
                this.statusText.text = "LINES_CHECK";
                LinesCheck();
                break;
            case Status_enum.LINES_DELETE:
                this.statusText.text = "LINES_DELETE";
                LinesDelete();
                break;
            case Status_enum.LINES_FILL:
                this.statusText.text = "LINES_FILL";
                LinesFillEmpty();
                break;
            //case Status_enum.:
            //    this.statusText.text = "FALLING";
            //    ActiveGroupCreate();
            //    break;
        }
    }

    private List<PlayerController> PlayersNeedBlocks = new List<PlayerController>();
    private void BlockCreateForPlayer()
    {
        while (PlayersNeedBlocks.Count > 0)
        {
            PlayerController p = PlayersNeedBlocks[0];
            p.ActivateNextGroup(this.CreateNewNextGroup(p));
            PlayersNeedBlocks.RemoveAt(0);
        }
        this.FieldStatus = Status_enum.BLOCKS_FALLING;
    }

    private List<BlockController> BlocksExploding = new List<BlockController>();
    private void LinesDelete()
    {
        foreach (var blok in BlocksExploding.ToArray())
        {
            if (blok.UpdateDestructionDone()) BlocksExploding.Remove(blok);
        }
        if (BlocksExploding.Count == 0) this.FieldStatus = Status_enum.LINES_FILL;
    }

    private List<float> LinesToFill = new List<float>();
    private List<BlockController> BlocksFalling = new List<BlockController>();
    private void LinesFillEmpty()
    {

        while (LinesToFill.Count > 0)
        {
            float LineY = LinesToFill[0];
            foreach (Transform b in transform)
            {
                if (b.position.y > LineY)
                {
                    BlockController block = b.gameObject.GetComponent<BlockController>();
                    if (block != null)
                    {
                        block.UpdateDownDone(-1f);
                        BlocksFalling.Add(block);
                    }
                }
            }
            LinesToFill.RemoveAt(0);
        }

        foreach (var blok in BlocksFalling.ToArray())
        {
            if (blok.UpdateDownDone(0f)) BlocksFalling.Remove(blok);
        }

        if (BlocksFalling.Count == 0) this.FieldStatus = Status_enum.BLOCK_CREATE;
    }

    private List<ActiveGroupController> FixedGroups = new List<ActiveGroupController>();
    private void LinesCheck()
    {
        //assume we immediately skip to creating new block for player. If any lines were filled,
        //this status will be overridden to Status_enum.LINES_DELETE

        while (FixedGroups.Count > 0)
        {
            ActiveGroupController fixedGroup = FixedGroups[0];

            PlayerController p = Players[fixedGroup.PlayerNr];
            this.FieldStatus = Status_enum.BLOCK_CREATE;
            this.PlayersNeedBlocks.Add(p);

            CheckForCompleteLines(fixedGroup);
            Reparent(fixedGroup);

            FixedGroups.RemoveAt(0);
            Destroy(fixedGroup.gameObject);
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

    private void ActiveGroupFall()
    {
        foreach (var g in FallingGroups.ToArray())
        {
            //during the call of this update, the element might get fixed and be removed
            //from the list. That's why the iterator runs over an array copy
            g.FallingUpdate();
        }
    }

    private List<PlayerController> PlayersToActivateNewGroup = new List<PlayerController>();
    private void ActiveGroupCreate()
    {
    }


    public int RegisterNewPlayer(PlayerController p)
    {
        int playernr = Players.Count;
        SetPlayerPositions(playernr, p);
        Players.Add(p);
        AdjustFieldWidth();
        return playernr;
    }

    public void RegisterFallingBlock(ActiveGroupController agc)
    {
        this.FallingGroups.Add(agc);
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
        m.mainTextureScale = new Vector2(widthPerPlayer* Players.Count, fieldHeight);

        //more players, equals bigger lines to fill
        blocksRequiredPerLine = widthPerPlayer * Players.Count;
    }

    //called by the group after it hit an obstruction and was fixed
    //(time to create a new group at the top)
    public void GroupWasFixed(ActiveGroupController fixedGroup)
    {

        Debug.Log("GroupWasFixed for player " + fixedGroup.PlayerNr);
        //this group should no longer receive fallingUpdates:
        this.FallingGroups.Remove(fixedGroup);

        this.FieldStatus = Status_enum.LINES_CHECK;
        this.FixedGroups.Add(fixedGroup);
    }

    private void Reparent(ActiveGroupController fixedGroup)
    {
        BlockController[] children = fixedGroup.GetComponentsInChildren<BlockController>();
        foreach (BlockController blk in children)
        {
            blk.transform.parent = transform;
        }
    }

    //I only need to check for complete lines on lines where blocks were added recently
    public void CheckForCompleteLines(ActiveGroupController fixedGroup)
    {

        BlockController[] blocks = fixedGroup.GetComponentsInChildren<BlockController>();
        float minY = float.NaN;
        float maxY = float.NaN;
        foreach (BlockController block in blocks)
        {
            if (float.IsNaN(minY) || block.transform.position.y < minY)
            {
                minY = block.transform.position.y;
            }
            if (float.IsNaN(maxY) || block.transform.position.y > maxY)
            {
                maxY = block.transform.position.y;
            }
        }
        Debug.Log("For " + blocks.Length + " blocks, CheckForCompleteLines from " + minY + " to " + maxY);
        float currentY = maxY;
        while (currentY > (minY-.1f))
        {
            if (CheckForSingleCompleteLine(currentY))
            {
                this.LinesToFill.Add(currentY);
                this.FieldStatus = Status_enum.LINES_DELETE;
            }
            currentY--;
        }
    }

    //returns true if a complete line was found. 
    public bool CheckForSingleCompleteLine(float LineY)
    {
        Vector3 origin = new Vector3(this.leftBorder.transform.position.x, LineY, 0);
        Vector3 direction = new Vector3(1f, 0, 0);
        float maxDistance = this.rightBorder.transform.position.x - this.leftBorder.transform.position.x;
        RaycastHit[] allHits = Physics.SphereCastAll(origin, .1f, direction, maxDistance);
        Debug.Log("for Y = " + LineY + ", found " + allHits.Length + " hits");
        if (allHits.Length == blocksRequiredPerLine)
        {
            foreach (var hit in allHits)
            {
                this.BlocksExploding.Add(hit.collider.GetComponent<BlockController>());
            }
            return true;
        }
        return false;
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
