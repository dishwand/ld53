using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualsManager : MonoBehaviour {

    public static VisualsManager instance;

    public AnimationCurve bumpCurve;
    public AnimationCurve groundSpawnCurve;
    public AnimationCurve entitySpawnCurve;

    public float spawnCrunch;

    public RandAudio plonk;

    protected virtual void Awake() {
        instance = this;
    }


    public void BumpWave(Vector3 pos, float size, float speed = 5f, float strength = 0.5f, float duration = 0.1f) {
        GridObject[] objs = FindObjectsOfType<GridObject>();

        if (duration == 0.1f) plonk.Play();

        for (int i = 0; i < objs.Length; i++) {
            float dist = GridWorldLibrary.FlatDist(objs[i].gameObject.transform.position, pos);
            float str = strength * Mathf.Sqrt(Mathf.Max(0, size - dist) / size);
            float delay = dist / speed;
            if (str > 0) {
                objs[i].BumpVisuals(str * 0.2f, duration, delay);
            }
        }
    }
}
