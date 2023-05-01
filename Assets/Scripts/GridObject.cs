using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public Vector3Int pos;
    public Direction dir;

    public GameObject visuals;

    [HideInInspector]
    public bool spawned = false;

    private bool bumping = false;

    protected virtual void Awake() {
        if (visuals == null) {
            visuals = transform.Find("Visuals").gameObject;
        }
        pos = Vector3Int.RoundToInt(transform.position);
        transform.position = pos;
    }

    public void SetVector3Int(Vector3Int p) {
        pos = p;
        transform.position = new Vector3(p.x, p.y, p.z);
    }

    public void SetDirection(Direction d) {
        dir = d;
        transform.rotation = Quaternion.LookRotation(-1 * DirectionVectors.worldSpace[(int)d]);
    }

    public void BumpVisuals(float strength, float dur, float delay) {
        if (!bumping) StartCoroutine(DoBump(strength, dur, delay));
    }

    private IEnumerator DoBump(float strength, float dur, float delay) {
        bumping = true;
        float t = 0;
        yield return new WaitForSeconds(delay);
        while (t < dur) {
            t += Time.deltaTime;
            SetBumpAmount(VisualsManager.instance.bumpCurve.Evaluate(t / dur) * strength);
            yield return new WaitForEndOfFrame();
        }
        SetBumpAmount(0);
        bumping = false;
    }

    public void SetBumpAmount(float amount) {
        visuals.transform.position = transform.position + Vector3.up * amount;
    }

    public virtual void Spawn(float delay) {
        if (spawned) return;
        spawned = true;
        StartCoroutine(DoSpawn(1, delay, VisualsManager.instance.groundSpawnCurve));
    }

    public virtual void Despawn(float delay) {
        StartCoroutine(DoSpawn(1, delay, VisualsManager.instance.groundSpawnCurve, true, true));
    }

    protected IEnumerator DoSpawn(float dur, float delay, AnimationCurve curve, bool reverse = false, bool despawn = false) {
        bumping = true;
        float t = 0;
        yield return new WaitForSeconds(delay);
        Show();
        while (t < dur) {
            t += Time.deltaTime;
            float f = t / dur;
            if (reverse) {
                f = 1 - f;
            }
            f = f.Crunch(VisualsManager.instance.spawnCrunch);
            SetBumpAmount(curve.Evaluate(f));
            yield return new WaitForEndOfFrame();
        }
        SetBumpAmount(curve.Evaluate(reverse ? 0 : 1));
        bumping = false;
        if (despawn) {
            gameObject.SetActive(false);
        }
    }

    public virtual float GetSpawnDelay(Vector3 startPos) {
        return GridWorldLibrary.FlatDist(startPos, transform.position) / 8f;
    }

    public virtual float GetDespawnDelay(Vector3 startPos) {
        return GridWorldLibrary.FlatDist(startPos, transform.position) / 8f;
    }

    public void Show() {
        visuals.gameObject.SetActive(true);
    }

    public void Hide() {
        visuals.gameObject.SetActive(false);
    }




}
