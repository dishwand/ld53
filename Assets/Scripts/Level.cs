using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int id = 0;

    [SerializeField]
    private Vector3Int playerStart;
    public GridObject playerStartObj;
    public Direction playerStartDir;

    public GridObject peek;

    public Vector3 cameraPos;

    public bool hasStartDialogue;
    public Dialogue startDialogue;

    public DialogueCollection idleDialogue;

    public Vector3Int GetPlayerStart() {
        if (playerStartObj != null) {
            return playerStartObj.pos + new Vector3Int(0, 1, 0);
        }
        return playerStart;
    }
}
