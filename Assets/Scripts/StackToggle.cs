using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StackToggle {
    public int count = 0;

    public System.Action onActivate = () => {};
    public System.Action onDeactive = () => {};


    public void Activate () {
        if (count == 0) {
            onActivate.Invoke();
        }
        count += 1;
    }

    public void Deactivate() {
        if (count <= 0) return;
        count -= 1;
        if (count == 0) {
            onDeactive.Invoke();
        }

    }

    public bool IsActive() {
        return count > 0;
    }
}