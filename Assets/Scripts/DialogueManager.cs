using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue {
    public string[] text;
    public bool stallInput;
}

[System.Serializable]
public class DialogueCollection {
    public List<Dialogue> dialogues;

    [HideInInspector]
    public List<Dialogue> seenDialogues;

    public Dialogue GetUnseen() {
        if (dialogues.Count == 0) {
            return null;
        }

        if (dialogues.Count == 1) {
            Dialogue d = dialogues[0];
            seenDialogues.Add(d);
            dialogues = new List<Dialogue>(seenDialogues);
            seenDialogues.Clear();
            return d;
        }

        int idx = Random.Range(0, dialogues.Count);
        Dialogue ret = dialogues[idx];
        dialogues.RemoveAt(idx);
        seenDialogues.Add(ret);
        return ret;
    }
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public Vector2 delayRange;

    public AnimateCharacters animChars;

    public DialogueCollection idleDialogue;

    private List<Dialogue> queue = new List<Dialogue>();

    void Awake() {
        instance = this;
    }

    public void StartGame() {
        StartCoroutine(DialogueLoop());
    }

    public void ForcePlay(Dialogue d) {
        queue.Add(d);
    }

    IEnumerator DialogueLoop() {
        yield return new WaitForSeconds(3.5f);
        while (true) {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("getting next dialogue to play");
            if (queue.Count > 0) {
                Debug.Log("choosing forced dialouge");
                Dialogue d = queue[0];
                queue.RemoveAt(0);
                animChars.PlayDialogue(d);
            } else if (GameManager.currentLevel != 0 && GameManager.currentLevel != 7 && GameManager.currentLevel != 10) {
                Debug.Log("waiting some time and playinh random");
                yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));

                // get one from current level collection or idle
                DialogueCollection curLevelIdle = GameManager.instance.levels[GameManager.currentLevel].idleDialogue;
                int numDialogues = curLevelIdle.dialogues.Count + idleDialogue.dialogues.Count;
                // bool chooseLevelCol = Random.Range(0, numDialogues) < curLevelIdle.dialogues.Count;
                bool chooseLevelCol = Random.Range(0f, 1f) < 0.6f && curLevelIdle.dialogues.Count >= 0;

                Dialogue chosen = (chooseLevelCol ? curLevelIdle : idleDialogue).GetUnseen();
                if (chosen != null) animChars.PlayDialogue(chosen);
            }

            while (animChars.state != AnimateCharacters.State.Stopped) {
                Debug.Log("waiting for dialogue to finish");
                yield return new WaitForSeconds(0.5f);
            }
            
            
        }
    }
}
