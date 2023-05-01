using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Piston : GridObject {

    bool pistonExtended;
    

    public bool stickyRetract;

    public Transform arm;

    public LayerMask raycastHitLayers;

    public PistonArm armObj;

    public float crunch = 6;

    private Vector3 stickyStartPos;

    public RandAudio extendAud;
    public RandAudio retractAud;
    public RandAudio moveAud;

    InputStall retractStall;
    protected override void Awake()
    {
        base.Awake();
        
        pistonExtended = false;

        // dir = Direction.EAST;

    }

    private void Update()
    {
        
    }


    public void Stick( GameObject go) {
        go.transform.SetParent(arm, true);
    }

    public bool Tumble(Direction inputDir) {
        Debug.Log("trying tumble");
        if (!IsMoveValid(inputDir)) return false;
        Vector3Int oldPos = pos;
        CheckWalkableGoal(inputDir);
        pos = GridWorldLibrary.TranslatePosByDir(pos, inputDir);
        Vector3 edgePos = GridWorldLibrary.GetTopEdgeBetweenSpots(pos, oldPos);
        dir = GridWorldLibrary.RotateFaceByDirection(dir, inputDir);

        GameObject go = GridWorldLibrary.RaycastInDirection(Direction.DOWN, oldPos);
        if (go != null) {
            GridObject obj = go.GetComponent<GridObject>();
            // lol
            if (obj != null && go.name.StartsWith("Ice")) {
                obj.GetComponent<BoxCollider>().enabled = false;
                obj.Despawn(0);
            }
        }        

        StartCoroutine(RotateAroundEdge(0.25f, edgePos, 90, inputDir));
        return true;
    }

    public bool TryHook() {
        Vector3Int targetPos = GridWorldLibrary.TranslatePosByDirX(pos, dir, 2);
        List<GridObject> objs = GridWorldLibrary.RaycastAllInDirection(Direction.DOWN, targetPos);
        Hookable hookable = null;
        foreach(GridObject go in objs) {
            if (go is Hookable && ((Hookable)go).ready) {
                hookable = (Hookable)go;
            }
        }
        if (hookable == null) return false;

        if (!IsHookValid(hookable)) return false;
        Vector3Int oldPos = pos;
        pos = targetPos;
        Vector3 edgePos = GridWorldLibrary.GetTopEdgeBetweenSpots(pos, GridWorldLibrary.TranslatePosByDir(oldPos, dir));
        Direction oldDir = dir;
        dir = Direction.DOWN;
        hookable.Use();
        StartCoroutine(DoHookRotAndRet(edgePos, oldDir));
        return true;
    }

    private IEnumerator DoHookRotAndRet(Vector3 edgePos, Direction oldDir) {
        yield return StartCoroutine(RotateAroundEdge(0.25f, edgePos, 90, oldDir));
        RetractArm(true);
    }

    private bool IsMoveValid(Direction inputDir) {
        //also check the new facing direction + new piston location
        if (GridWorldLibrary.RotateFaceByDirection(dir, inputDir) == Direction.DOWN && pistonExtended) return false;
        Debug.Log("check: not facing down");
        if (RaycastInDirection(inputDir)) return false;
        Debug.Log("check: not blocked");
        if (pistonExtended && RaycastInDirection(GridWorldLibrary.RotateFaceByDirection(dir, inputDir), GridWorldLibrary.TranslatePosByDir(pos, inputDir))) return false;
        Debug.Log("check: piston won't collide");
        if (!GridWorldLibrary.IsGroundBelowPosition(GridWorldLibrary.TranslatePosByDir(pos, inputDir), inputDir, true)) return false;
        Debug.Log("check: there is ground");
        return true;
    }

    private bool IsHookValid(Hookable hookable) {
        if (hookable.dir != dir.Opposite()) return false;
        if (!pistonExtended) return false;
        if (!GridWorldLibrary.IsGroundBelowPosition(GridWorldLibrary.TranslatePosByDirX(pos, dir, 2))) return false;
        return true;
    }

    //default to facing direction
    private bool IsArmExtendValid() {
        //TODO: work for multiple height levels
        if (dir == Direction.DOWN) return false;
        GameObject go = RaycastInDirection(dir);
        if (go && (go.tag == "Immovable" || go.tag == "Grate")) return false;
        return true;
    }

    public bool ExtendArm() {
        if (pistonExtended) return false;
        if (!IsArmExtendValid()) return false;

        GameObject hit = RaycastInDirection(dir);
        if (hit) {
            Pushable p = hit.GetComponent<Pushable>();
            if (p != null) {
                if (!p.TryPush(dir)) return false;
                else {

                }
                
            }
        }

        extendAud.Play();
        pistonExtended = true;
        armObj.Extend();
        return true;
    }

    public bool RetractArm(bool _stickyRetract = false) {
        if (!pistonExtended) return false;
        pistonExtended = false;
        stickyRetract = _stickyRetract;
        if (stickyRetract) {
            stickyStartPos = pos;
        }
        retractAud.Play();
        armObj.Retract(stickyRetract);
        if (retractStall != null) {
            retractStall.Resolve();
        }
        retractStall = new InputStall(gameObject, true);
        return true;
    }

    public void ArmRetractUpdate(float f) {
        if (stickyRetract) {
            Vector3Int intDir = (DirectionVectors.worldSpace[(int)dir]);
            Vector3 retDir = new Vector3(intDir.x, intDir.y, intDir.z);
            transform.position = stickyStartPos + (retDir * -f);
        }

    }

    void CheckWalkableGoal(Direction inputDir) {
        GameObject go = GridWorldLibrary.RaycastInDirection(Direction.DOWN, GridWorldLibrary.TranslatePosByDir(pos, inputDir));
        if (go) {
            WalkableGoal wg = go.GetComponent<WalkableGoal>();
            if (wg && wg.active && !wg.reached) {
                wg.reached = true;
                wg.HidePressure();
                StartCoroutine(End());
            }
        }
    }

    IEnumerator End() {
        InputStall stall = new InputStall(gameObject, true);
        GameManager.instance.victory.Play();
        yield return new WaitForSeconds(2);
        GameManager.instance.FinishLevel();
        yield return new WaitForSeconds(1);
        stall.Resolve();
    }

    public void OnExtendDone() {
        TryHook();
    }
    public void OnRetractDone() {
        stickyRetract = false;
        if (retractStall != null) {
            retractStall.Resolve();
            retractStall = null;
        }
    }


    private GameObject RaycastInDirection(Direction dir) {
        RaycastHit hit;
        Vector3 worldDir = DirectionVectors.worldSpace[(int)dir];
        if (Physics.Raycast(transform.position, worldDir, out hit, 1f, raycastHitLayers.value)){
            return hit.collider.gameObject;
        }
        return null;
    }

    //from a non-piston position
    private GameObject RaycastInDirection(Direction dir, Vector3 pos) {
        RaycastHit hit;
        Vector3 worldDir = DirectionVectors.worldSpace[(int)dir];
        if (Physics.Raycast(pos, worldDir, out hit, 1f, raycastHitLayers.value)) {
            return hit.collider.gameObject;
        }
        return null;
    }

    IEnumerator RotateAroundEdge(float timeLength, Vector3 edgePos, float degrees, Direction dir)
    {
        InputStall stall = new InputStall(gameObject, true);

        float t = 0;
        if (timeLength < 0) timeLength = 1;

        Quaternion startRotation = transform.localRotation;
        Vector3 startPosition = transform.localPosition;
        Vector3 axisDir = DirectionVectors.rollVectors[(int)dir];
        while (t < timeLength) {
            t = Mathf.Clamp(t + Time.deltaTime, 0, timeLength);
            float f = t / timeLength;
            f = ((Mathf.Floor(f  * crunch) + 1) / (crunch + 1)) * degrees;
            
            RotateAroundHelper(startRotation, startPosition, f, edgePos, axisDir);

            yield return new WaitForEndOfFrame();
        }
        RotateAroundHelper(startRotation, startPosition, degrees, edgePos, axisDir);
        //transform.localPosition = GridWorldLibrary.GridToWorldPos(pos);

        moveAud.Play();
        stall.Resolve();
    }

    private void RotateAroundHelper(Quaternion startRotation, Vector3 startPosition, float degrees, Vector3 point, Vector3 axisDir)
    {
        transform.localRotation = startRotation;
        transform.localPosition = startPosition;
        transform.RotateAround(point, axisDir, degrees);
    }

    IEnumerator Animate(float length)
    {

        float t = 0;
        if (length < 0) length = 1;

        Vector3 translate = Vector3.zero;
        Vector3 rotate = Vector3.zero;
        Vector3 startPosition = transform.localPosition;
        Vector3 startRotation = transform.localEulerAngles;
        while (t < length) {
            t = Mathf.Clamp(t + Time.deltaTime, 0, length);
            float f = t / length;

            f = (Mathf.Floor(f * crunch) / crunch);

            rotate.z = f * 90f;
            translate.x = Mathf.Cos(Mathf.PI/2 + (f * Mathf.PI/2));
            translate.y = 0.5f * Mathf.Sin(f * Mathf.PI);

            transform.position = new Vector3(
                startPosition.x + translate.x,
                startPosition.y + translate.y,
                startPosition.z);

            transform.localEulerAngles = new Vector3(
                startRotation.x,
                startRotation.y,
                startRotation.z + rotate.z);

            yield return new WaitForEndOfFrame();
        }

        transform.position = new Vector3(
            startPosition.x + Mathf.Cos(Mathf.PI),
            startPosition.y + 0.5f * Mathf.Sin(Mathf.PI),
            startPosition.z);

        transform.localEulerAngles = new Vector3(
            startRotation.x,
            startRotation.y,
            startRotation.z + 90f);


    }

    public override void Spawn(float delay) {
        Debug.Log("pisting spawning!!!");
        StartCoroutine(DoSpawn(delay));
    }

    public IEnumerator DoSpawn(float delay) {
        yield return StartCoroutine(DoSpawn(1, delay, VisualsManager.instance.entitySpawnCurve));
        VisualsManager.instance.BumpWave(pos, 3, 10, 0.7f);
    }

    
    public override float GetSpawnDelay(Vector3 startPos) {
        return 2.5f;
    }
}
