using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{

    public float smoothing = 10;
    public Vector3 targetPos;

    public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt) {
        return Vector3.Lerp(a, b, 1- Mathf.Exp(-lambda * dt));
    }
    
    void Awake() {
        targetPos = transform.position;
    }

    void Update()
    {
        transform.position = Damp(transform.position, targetPos, smoothing, Time.deltaTime);
    }
}
