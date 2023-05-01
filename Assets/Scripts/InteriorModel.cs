using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorModel : MonoBehaviour
{
    public Piston piston;

    void LateUpdate() {
        transform.rotation = piston.transform.rotation;
    }
}
