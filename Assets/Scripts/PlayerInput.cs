using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InputType
{
    MOVE_NORTH,
    MOVE_EAST,
    MOVE_SOUTH,
    MOVE_WEST,
    EXTEND_ARM,
    RETRACT_ARM
}

public class InputStall
{
    public GameObject go;
    
    public InputStall (GameObject _go, bool autoAdd) {
        if (autoAdd) Add();
        go = _go;
    }

    public void Add() {
        if (PlayerInput.instance.stalls == null) {
            PlayerInput.instance.stalls = new List<InputStall>();
        }
        PlayerInput.instance.stalls.Add(this);
    }

    public void Resolve() {
        PlayerInput.instance.stalls.Remove(this);
    }
}

public class PlayerInput : MonoBehaviour {

    [HideInInspector]
    public static PlayerInput instance;
    [HideInInspector]
    public List<InputStall> stalls;

    public Piston player;

    private float heldRTime = 0;
    private float heldPTime = 0;

    private InputStall startStall;
    public static bool firstTime = true;
    
    public bool IsInputReady() {
        if (stalls == null) {
            stalls = new List<InputStall>();
        }
        return stalls.Count == 0;
    }

    // Use this for initialization
    private void Awake() {
        instance = this;
        if (firstTime) {
            startStall = new InputStall(gameObject, true);
        }
    }

    // Update is called once per frame
    void Update() {

        bool successfulInput = false;
        if (Input.GetKey(KeyCode.D) && IsInputReady()) {
            successfulInput = player.Tumble(Direction.WEST);
        } else if (Input.GetKey(KeyCode.A) && IsInputReady()) {
            successfulInput = player.Tumble(Direction.EAST);
        } else if (Input.GetKey(KeyCode.W) && IsInputReady()) {
            successfulInput = player.Tumble(Direction.SOUTH);
        } else if (Input.GetKey(KeyCode.S) && IsInputReady()) {
            successfulInput = player.Tumble(Direction.NORTH);
        } else if (Input.GetKey(KeyCode.Space) && IsInputReady()) {
            successfulInput = player.ExtendArm();
        } else if (!Input.GetKey(KeyCode.Space) && IsInputReady()) {
            successfulInput = player.RetractArm();
        } else if (Input.GetKey(KeyCode.Tab) && IsInputReady()) {
            successfulInput = true;     
        }

        if (Input.GetKey(KeyCode.R) && IsInputReady()) {
            heldRTime += Time.deltaTime;
            if (heldRTime > 1) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            }
        } else {
            heldRTime = 0;
        }

        if (Input.GetKey(KeyCode.P) && IsInputReady()) {
            heldPTime += Time.deltaTime;
            if (heldPTime > 1) {
                GameManager.instance.FinishLevel();
                heldPTime = 0;
            }
        } else {
            heldPTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.Q) && firstTime) {
            firstTime = false;
            startStall.Resolve();
            GameManager.instance.StartGame();
        }

    }
}
