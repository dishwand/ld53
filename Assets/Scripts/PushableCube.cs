using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Pushable {
    public bool TryPush(Direction inputDir);
}

public class PushableCube : GridObject, Pushable {

    public bool stuck;

    public float crunch = 1;

    public AnimationCurve pushCurve;

    //returns false if this box cannot be pushed
    public bool TryPush(Direction inputDir) {
        if (GridWorldLibrary.RaycastInDirection(inputDir, pos)) return false;
        if (!GridWorldLibrary.IsGroundBelowPosition(GridWorldLibrary.TranslatePosByDir(pos, inputDir))) return false;
        UpdateGoal(inputDir);
        pos = pos.Translate(inputDir);
        StartCoroutine(LerpToPosition(pos, 0.25f));
        return true;
    }

    private void UpdateGoal(Direction inputDir) {
        GameObject go = GridWorldLibrary.RaycastInDirection(Direction.DOWN, GridWorldLibrary.TranslatePosByDir(pos, inputDir));
        if (go) {
            PushableGoal pg = go.GetComponent<PushableGoal>();
            if (pg) {
                StartCoroutine(End());
            }
        }
    }

    IEnumerator End() {
        InputStall stall = new InputStall(gameObject, true);
        GameManager.instance.victory.Play();
        yield return StartCoroutine(DoSpawn(2, 0.3f, VisualsManager.instance.groundSpawnCurve, true));

        GameManager.instance.FinishLevel();
        yield return new WaitForSeconds(1);
        stall.Resolve();
        gameObject.SetActive(false);
    }

    IEnumerator LerpToPosition(Vector3 inputPos, float timeLength) {

        float t = 0;
        if (timeLength < 0) timeLength = 1;

        Vector3 startPosition = transform.position;
        while (t < timeLength) {
            t = Mathf.Clamp(t + Time.deltaTime, 0, timeLength);
            float f = t / timeLength;
            // f = Mathf.Floor(f * crunch) / crunch;
            // transform.position = Vector3.Slerp(startPosition, inputPos, f);
            transform.position = Vector3.Lerp(startPosition, inputPos, pushCurve.Evaluate(f).Crunch(7));
            yield return new WaitForEndOfFrame();
        }
        transform.position = inputPos;
    }

    public override void Spawn(float delay) {
        StartCoroutine(DoSpawn(delay));
    }

    public IEnumerator DoSpawn(float delay) {
        yield return StartCoroutine(DoSpawn(1, delay, VisualsManager.instance.entitySpawnCurve));
        VisualsManager.instance.BumpWave(pos, 3, 10, 0.3f);
    }

    public override void Despawn(float delay) {
        StartCoroutine(DoSpawn(1, delay, VisualsManager.instance.entitySpawnCurve, true));
    }

    public override float GetSpawnDelay(Vector3 startPos) {
        return 2f;
    }

    public override float GetDespawnDelay(Vector3 startPos) {
        return 0f;
    }

    
}


