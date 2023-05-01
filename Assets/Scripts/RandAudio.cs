using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandAudio : MonoBehaviour
{
    private AudioSource aud;

    public Vector2 pitchRange;
    public Vector2 volRange;

    private float pitch;
    private float vol;
    
    public bool useRandom = false;
    public AudioClip[] randomClips;

    public bool loopOnAwake = false;
    public Vector2 delayRange;

    void Awake() {
        aud = GetComponent<AudioSource>();
        pitch = aud.pitch;
        vol = aud.volume;

        if (loopOnAwake) {
            StartCoroutine(AudioLoop());
        }
    }

    IEnumerator AudioLoop() {
        while(true) {
            // wait for audio to not be playing
            while (aud.isPlaying) {
                yield return new WaitForSeconds(2f);
            }

            yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));

            Play();
        }
    }


    public void Play() {
        AudioClip clip = aud.clip;
        if (useRandom) {
            clip = randomClips[Random.Range(0, randomClips.Length)];
        }
        aud.pitch = pitch * Random.Range(pitchRange.x, pitchRange.y);
        aud.volume = vol * Random.Range(volRange.x, volRange.y);
        aud.PlayOneShot(aud.clip);
    }
}
