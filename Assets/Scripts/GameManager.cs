using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int levelOverride = -1;
    public static int currentLevel = 0;

    public Dictionary<int, Level> levels = new Dictionary<int, Level>();

    public Piston piston;

    public CamControl cam;

    public AudioSource mystery;
    public AudioSource victory;

    public AudioSource tick;
    public AudioSource explode;

    private bool levelFinished = false;

    public AnimatableCurve explosionCurve;
    public SpriteRenderer explosionFade;

    public GameObject instrText;
    public GameObject logo;
    public GameObject menu;

    void Awake() {
        instance = this;
        if (levelOverride != -1) currentLevel = levelOverride;
        Debug.Log("awaking game manager");
        
        Level[] foundLevels = FindObjectsOfType<Level>(true);
        for (int i = 0; i < foundLevels.Length; i++) {
            levels.Add(foundLevels[i].id, foundLevels[i]);
        }

        explosionCurve.onUpdate += (f) => {
            explosionFade.color = new Color(1, 1, 1, f);
        };
    }

    public void StartGame() {
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine() {
        tick.PlayOneShot(tick.clip);
        instrText.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        tick.PlayOneShot(tick.clip);
        logo.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        tick.PlayOneShot(tick.clip);
        menu.SetActive(false);
        StartLoad();
    }

    // fourth start function :)
    void StartLoad() {
        cam.transform.position = levels[currentLevel].cameraPos;
        Level curLevel = levels[currentLevel];
        piston.transform.position = curLevel.GetPlayerStart();
        piston.pos = curLevel.GetPlayerStart();
        piston.dir = curLevel.playerStartDir;
        piston.transform.localEulerAngles = DirectionVectors.spawnRots[(int)piston.dir];
        piston.Hide();
        piston.SetBumpAmount(10);
        piston.Spawn(piston.GetSpawnDelay(curLevel.GetPlayerStart()));
        LoadLevel(currentLevel);
        DialogueManager.instance.StartGame();
    }

    void Start() {
        foreach (var pair in levels) {
            int id = pair.Key;
            if (id != currentLevel) {
                HideLevel(id);
            }
        }

        if (!PlayerInput.firstTime) {
            instrText.SetActive(false);
            menu.SetActive(false);
            logo.SetActive(false);
            StartLoad();
        }

    } 

    void HideLevel(int id) {
        levels[id].gameObject.SetActive(false);
    }

    void LoadLevel(int level) {
        levelFinished = false;
        Level curLevel = levels[level];
        cam.targetPos = curLevel.cameraPos;
        curLevel.gameObject.SetActive(true);
        GridObject[] objs = curLevel.transform.GetComponentsInChildren<GridObject>(true);
        for (int i = 0; i < objs.Length; i++) {
            if (!objs[i].spawned) {
                objs[i].gameObject.SetActive(true);
                objs[i].Hide();
                objs[i].Spawn(objs[i].GetSpawnDelay(curLevel.GetPlayerStart()));
            }

        }
        
        if (curLevel.hasStartDialogue) DialogueManager.instance.ForcePlay(curLevel.startDialogue);

        StartCoroutine(LoadStall());
    }

    IEnumerator LoadStall() {
        InputStall stall = new InputStall(gameObject, true);
        yield return new WaitForSeconds(3.5f);
        stall.Resolve();
    }

    IEnumerator UnloadLevel(int level) {
        GridObject[] objs = levels[level].transform.GetComponentsInChildren<GridObject>();
        for (int i = 0; i < objs.Length; i++) {
            objs[i].Despawn(objs[i].GetDespawnDelay(levels[level].GetPlayerStart()));
        }
        yield return new WaitForSeconds(2f);
        levels[level].gameObject.SetActive(false);
    }

    public void FinishLevel() {
        if (levelFinished) return;
        PlayerSprite.instance.Smile();
        levelFinished = true;
        if (currentLevel >= 10) {
            StartCoroutine(EndSequence());
        } else {
            PeekLevel(currentLevel + 1);
        }
        
    }

    IEnumerator EndSequence() {
        InputStall stall =new InputStall(gameObject, true);
        tick.PlayOneShot(tick.clip);
        VisualsManager.instance.BumpWave(levels[currentLevel].transform.Find("Goal").transform.position, 50, 15, 2f, .25f);
        yield return new WaitForSeconds(1.5f);
        tick.PlayOneShot(tick.clip);
        VisualsManager.instance.BumpWave(levels[currentLevel].transform.Find("Goal").transform.position, 50, 15, 2f, .25f);
        
        yield return new WaitForSeconds(1.5f);
        tick.PlayOneShot(tick.clip);
        VisualsManager.instance.BumpWave(levels[currentLevel].transform.Find("Goal").transform.position, 50, 15, 2f, .25f);
        yield return new WaitForSeconds(1.5f);
        explode.Play();

        explosionCurve.Play(this);
    }

    void PeekLevel(int level) {
        Debug.Log("peeking level!" + level);
        mystery.Play();
        levels[level].gameObject.SetActive(true);
        Debug.Log(levels[level].gameObject.name);
        GridObject[] objs = levels[level].transform.GetComponentsInChildren<GridObject>();
        for (int i = 0; i < objs.Length; i++) {
            objs[i].Hide();
            objs[i].gameObject.SetActive(false);
        }
        GridObject peekObj = levels[level].peek;
        peekObj.gameObject.SetActive(true);
        peekObj.Spawn(0);
        (peekObj as Hookable).showLevel = levels[level];
    }

    public void SwitchLevel(int id) {
        StartCoroutine(UnloadLevel(currentLevel));
        currentLevel = id;
        LoadLevel(currentLevel);
    }
}
