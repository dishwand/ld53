using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableGoal : MonoBehaviour {

    public bool reached;

    public void SetReached(bool _reached) {
        reached = _reached;
    }
}
