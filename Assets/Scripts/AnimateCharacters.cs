using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnimateCharacters : MonoBehaviour
{
    TMP_Text textMesh;

    Mesh mesh;

    Vector3[] vertices;
    Color[] colors;

    float t = 0;

    public float duration = 1;

    public AnimationCurve yCurve;
    public float yScale;
    public AnimationCurve xCurve;
    public float xScale;

    public Gradient gradient;

    public float charDelay = 1f;

    public AnimatableCurve fadeOutCurve;

    private RandAudio aud;

    public enum State {
        Stopped,
        Waiting,
        Playing,
    }

    public State state = State.Stopped;

    int curChar = 0;

    public Image dialogueImage;

    public AnimatableCurve dialogueShow;
    
    private Dialogue currentDialogue = null;
    private int currentProgress = 0;

    private InputStall dialogueStall = null;

    void Awake()
    {
        aud = GetComponent<RandAudio>();
        textMesh = GetComponent<TMP_Text>();

        fadeOutCurve.onUpdate += (f) => {
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, f);  
        };

        dialogueShow.onUpdate += (f) => {
            dialogueImage.fillAmount = f;
        };

        // StartCoroutine(Play("i literally love drinking brake fluid after a hard's night work"));
    }

    float GetFinishedTime() {
        return (charDelay * (2 + textMesh.textInfo.characterCount));
    }
    
    void MoveChar(ref Vector3[] vertices, int index, Vector3 offset) {
        vertices[index] += offset;
        vertices[index + 1] += offset;
        vertices[index + 2] += offset;
        vertices[index + 3] += offset;
    }

    void SetCharColor(ref Color[] colors, int index, Color c) {
        colors[index] = c;
        colors[index + 1] = c;
        colors[index + 2] = c;
        colors[index + 3] = c;
    }


    public void PlayDialogue(Dialogue d) {
        currentDialogue = d;
        currentProgress = 0;
        if (d.stallInput) {
            dialogueStall = new InputStall(gameObject, true);
        }
        StartCoroutine(Play());
    }

    private IEnumerator Advance() {
        state = State.Waiting;
        PlayerSprite.instance.StopTalking();
        yield return new WaitForSeconds(3f);
        PlayerSprite.instance.StartTalking();
        currentProgress += 1;
        curChar = 0;
        fadeOutCurve.SimulateStart();
        textMesh.SetText(currentDialogue.text[currentProgress]);
        textMesh.ForceMeshUpdate();
        t = 0;
        textMesh.enabled = true;
        state = State.Playing;

        // yield return new WaitForSeconds(0.2f);

        UpdateVisuals();
    }
    
    private IEnumerator Play() {
        curChar = 0;
        PlayerSprite.instance.StartTalking();
        fadeOutCurve.SimulateStart();
        textMesh.SetText(currentDialogue.text[currentProgress]);
        textMesh.ForceMeshUpdate();
        t = 0;
        textMesh.enabled = true;
        state = State.Playing;

        dialogueShow.Play(this);
        UpdateVisuals();
        yield return new WaitForSeconds(0.2f);

        UpdateVisuals();
    }

    public IEnumerator Stop() {
        if (dialogueStall != null) {
            dialogueStall.Resolve();
        }
        PlayerSprite.instance.StopTalking();
        state = State.Waiting;
        yield return new WaitForSeconds(3f);
        state = State.Stopped;
        dialogueShow.PlayReverse(this);
        textMesh.enabled = false;
    }


    Vector3 GetOffset(float t) {
        if (state == State.Playing) {
            return new Vector3(xCurve.Evaluate(t / duration) * xScale, yCurve.Evaluate(t / duration) * yScale, 0);
        } else {
            return new Vector3(xCurve.Evaluate(1 - (t / duration)) * xScale, yCurve.Evaluate(1 - (t / duration)) * -yScale, 0);
        }
    }

    Color GetColor(float t) {
        if (state == State.Playing) {
            return gradient.Evaluate(t / duration);
        } else {
            return gradient.Evaluate(1 - (t / duration));
        }
         
    }

    void UpdateVisuals() {
        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        vertices = mesh.vertices;
        colors = mesh.colors;

        int visCount = 0;


        for (int i = 0; i < textMesh.textInfo.characterCount; i++)
        {
            
            if (!textMesh.textInfo.characterInfo[i].isVisible) {
                visCount += 1;
                continue;
            }

            

            TMP_CharacterInfo c = textMesh.textInfo.characterInfo[i];

            int index = c.vertexIndex;

            int newChar = Mathf.Clamp(Mathf.FloorToInt(t / charDelay), 0, textMesh.textInfo.characterCount) / 2;
            if (newChar != curChar) {
                curChar = newChar;
                aud.Play();
            }

            float vt = Mathf.Clamp(t - (visCount * charDelay), 0, duration);

            Vector3 offset = GetOffset(vt);
            MoveChar(ref vertices, index, offset);
            SetCharColor(ref colors, index, GetColor(vt));
            visCount += 1;

        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        textMesh.canvasRenderer.SetMesh(mesh);
    }

    void Update()
    {
        if (state == State.Playing) {
            t += Time.deltaTime;
            UpdateVisuals();
            if (t >= GetFinishedTime()) {
                if (currentProgress >= currentDialogue.text.Length - 1) {
                    StartCoroutine(Stop());
                } else {
                    StartCoroutine(Advance());
                }
                
            }
        }

    }

    Vector2 Wobble(float time) {
        return new Vector2(Mathf.Sin(time*3.3f), Mathf.Cos(time*2.5f));
    }
}