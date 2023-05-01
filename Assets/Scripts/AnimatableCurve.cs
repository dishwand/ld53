using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TimedEvent {
    public float time;
    public UnityEvent evnt;
}

[System.Serializable]
public class AnimatableCurve {
    public AnimationCurve curve;
    public float duration;
    public Vector2 range;

    public float endDelay = 0f;
    public float crunch = 0f;
    public bool looping = false;

    // private bool reverse = false;

    private float t = 0;
    private bool playing = false;
    private IEnumerator coroutine = null;

    public System.Action<float> onUpdate = (f) => {};
    public System.Action onEnd = () => {};
    public System.Action onEndReverse = () => {};

    public System.Action onCantPlay = () => {};

    [SerializeField]
    public List<TimedEvent> timedEvents;

    public bool preventDoublePlay;

    private MonoBehaviour beh;
    private bool reverse = false;

    private IEnumerator PlayRoutine(float startTime = 0.0f) {
        playing = true;
        do {
            t = startTime;
            float lastFrac = reverse ? 1f - (t / duration) : t / duration;;
            while (t < duration) {
                t = Mathf.Min(t + Time.deltaTime, duration);
                float frac = reverse ? 1f - (t / duration) : t / duration;
                float val = curve.Evaluate(frac);
                val *= (range.y - range.x);
                if (crunch != 0) {
                    val = Mathf.Floor(val * crunch) / crunch;
                }
                val += range.x;
                onUpdate(val);
                for (int i = 0; i < timedEvents.Count; i++) {
                    if (lastFrac < timedEvents[i].time && frac >= timedEvents[i].time) {
                        timedEvents[i].evnt.Invoke();
                    }
                }
                yield return new WaitForEndOfFrame();
                lastFrac = frac;
            }
            if (endDelay != 0) yield return new WaitForSeconds(endDelay);
        } while (looping);

        playing = false;
        if (!reverse) onEnd();
        else onEndReverse();
    }

    public void Play(MonoBehaviour mb) {
        beh = mb;
        
        if (!playing) {
            if (preventDoublePlay && t > duration) return; 
            reverse = false;
            coroutine = PlayRoutine(); 
            mb.StartCoroutine(coroutine);
        }
        else if (reverse) {
            reverse = false;
            mb.StopCoroutine(coroutine);
            coroutine = PlayRoutine(duration - t);
            mb.StartCoroutine(coroutine);
        }
    }

    public void PlayReverse(MonoBehaviour mb) {
        beh = mb;
        
        if (!playing) {
            if (preventDoublePlay && t > duration) return;
            reverse = true;
            coroutine = PlayRoutine();
            mb.StartCoroutine(coroutine);
        }
        else if (!reverse) {
            reverse = true;
            mb.StopCoroutine(coroutine);
            coroutine = PlayRoutine(duration - t);
            mb.StartCoroutine(coroutine);
            //onCantPlay();
        }
            
    }

    public void Reverse() {
        range = new Vector2(range.y, range.x);
        t = duration - t;
    }

    public void SimulateEnd() {
        float val = curve.Evaluate(1);
        val *= (range.y  - range.x);
        val += range.x;
        onUpdate(val);
    }

    
    public void SimulateStart() {
        float val = curve.Evaluate(0);
        val *= (range.y  - range.x);
        val += range.x;
        onUpdate(val);
    }
}