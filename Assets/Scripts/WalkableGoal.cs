using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkableGoal : MonoBehaviour {

    public bool reached = false;

    public bool active = true;
    public float delayActivate = 5f;

    public Dialogue onReachDialogue;
    public bool hasOnReachDialogue;

    public void HidePressure() {
        if (hasOnReachDialogue) DialogueManager.instance.ForcePlay(onReachDialogue);

        Transform pressure = transform.Find("Visuals/Pressure");
        if (pressure != null) {
            pressure.transform.localPosition = new Vector3(0, 0, -0.1f);
        }
    }

    void Start() {
        if (!active) StartCoroutine(Delay());
    }

    IEnumerator Delay() {
        yield return new WaitForSeconds(delayActivate);
        active = true;
    }
}
