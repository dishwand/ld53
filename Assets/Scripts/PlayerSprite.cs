using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public Sprite idle;
    public Sprite action;
    public Sprite happy;

    private SpriteRenderer sr;

    public static PlayerSprite instance;

    private bool talking = false;

    void Awake() {
        instance = this;
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Loop());
    }

    IEnumerator Loop() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(0.6f, 0.7f));
            if (talking && sr.sprite != happy) sr.sprite = action;
            yield return new WaitForSeconds(Random.Range(0.25f, 0.35f));
            if (talking && sr.sprite != happy) sr.sprite = idle;
        }
    }

    public void StartTalking() {
        talking = true;
    }

    public void StopTalking() {
        talking = false;
        sr.sprite = idle;
    }

    public void Smile() {
        StartCoroutine(DoSmile());
    }

    IEnumerator DoSmile() {
        sr.sprite = happy;
        yield return new WaitForSeconds(2);
        sr.sprite = idle;
    }
}
