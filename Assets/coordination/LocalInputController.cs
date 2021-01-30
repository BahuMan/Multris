using System.Collections;
using UnityEngine;

/* this class is only responsible for taking input and converting that to the right command */
[RequireComponent(typeof(PlayerController))]
public class LocalInputController: MonoBehaviour
{
    [Tooltip("The Unity Input Preferences can show you the abbreviations used for different inputs")]
    public string InputExtension = "KB";

    private PlayerController _player;

    public float moveDelay = .05f;
    public float rotateDelay = .3f;
    private float nextMove = 0;
    private float nextRotate = 0;

    void Start()
    {
        _player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (_player.PlayerStatus == PlayerController.PlayerStatusEnum.PLAYING)
        {
            ProcessInput();
        }
        else if (Input.GetButtonDown("Drop" + InputExtension))
        {
            _player.JoinGame();
        }

    }

    private void ProcessInput()
    {
        if (nextMove < Time.time)
        {
            if (Input.GetAxis("Horizontal" + InputExtension) > .3f)
            {
                _player.MoveOneRight();
                nextMove = Time.time + moveDelay / Input.GetAxis("Horizontal" + InputExtension);
            }

            if (Input.GetAxis("Horizontal" + InputExtension) < -.3f)
            {
                _player.MoveOneLeft();
                nextMove = Time.time + moveDelay / -Input.GetAxis("Horizontal" + InputExtension);
            }

            if (Input.GetAxis("Vertical" + InputExtension) < -.3f)
            {
                _player.MoveOneDown();
                nextMove = Time.time + moveDelay / -Input.GetAxis("Vertical" + InputExtension);
            }
        }
        if (nextRotate < Time.time)
        {
            if (Input.GetButton("RotateClockWise" + InputExtension))
            {
                _player.RotateClockWise();
                nextRotate = Time.time + rotateDelay;
            }
            if (Input.GetButton("RotateCounterClockWise" + InputExtension))
            {
                _player.RotateCounterClockWise();
                nextRotate = Time.time + rotateDelay;
            }
        }
        if (Input.GetButtonDown("Drop" + InputExtension))
        {
            _player.Drop();
            //lastKeyboard = Time.time;
        }
    }

    public void OnGameOver()
    {
        Debug.Log("Game Over for player " + InputExtension);
        Destroy(this); //will stop the script entirely; so no more processing input
    }
}
