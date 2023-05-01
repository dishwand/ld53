using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : GridObject {

    [HideInInspector]
    public bool ready = true;

    public AnimatableCurve rotCurve;

    public Transform hookParent;

    [HideInInspector]
    public Level showLevel = null;

    protected override void Awake() {
        base.Awake();
        rotCurve.onUpdate += (f) => {
            hookParent.transform.localEulerAngles = new Vector3(f, 0, 0);
        };

        rotCurve.onEnd += () => {
            if (showLevel != null) {
                GameManager.instance.SwitchLevel(showLevel.id);
            }
        };
    }

    public void Use() {
        if (ready) {
            ready = false;
            rotCurve.Play(this);
        }
    }

}


