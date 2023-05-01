using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonArm : MonoBehaviour
{

    public AnimatableCurve extendCurve;

    private StackToggle extendStack = new StackToggle();

    private Vector3 localStart;

    InputStall extendStall;

    public Piston piston;

    void Awake() {
        localStart = transform.localPosition;

        extendStack.onActivate += () => {
            extendStall = new InputStall(gameObject, true);
            extendCurve.Play(this);
        };
        extendStack.onDeactive += () => {
            extendStall = new InputStall(gameObject, true);
            extendCurve.PlayReverse(this);
        };

        extendCurve.onUpdate += (f) => {
            transform.localPosition = localStart + new Vector3(0, -f, 0);
            piston.ArmRetractUpdate(f);
        };
        extendCurve.onEnd += () => {
            piston.OnExtendDone();
            if (extendStall != null) extendStall.Resolve();
        };
        extendCurve.onEndReverse += () => {
            piston.OnRetractDone();
            if (extendStall != null) extendStall.Resolve();
        };
    }

    public void Extend() {
        extendStack.Activate();
    }

    public void Retract(bool sticky) {
        if (sticky) {
            extendStack.count = 0;
            extendCurve.PlayReverse(this);
        } else {
            extendStack.Deactivate();    
        }
        
    }
}
