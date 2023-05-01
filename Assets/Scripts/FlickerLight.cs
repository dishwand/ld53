using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    private float intensity;
    
    public AnimatableCurve flicker;

    Light light;

    public Vector2 delayRange;

    void Awake() {
        light = GetComponent<Light>();
        intensity = light.intensity;

        flicker.onUpdate += (f) => {
            light.intensity = intensity + f;
        };

        StartCoroutine(Flicker());
    }

    IEnumerator Flicker() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));
            flicker.Play(this);
        }
    }
}
