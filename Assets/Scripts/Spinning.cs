using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinning : MonoBehaviour
{
    public float speed;

    void Update() {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(Time.deltaTime * speed, Vector3.up);
    }
}
