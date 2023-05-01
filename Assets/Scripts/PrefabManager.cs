using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Prefab
{
    public string name;
    public GameObject prefab;
}

public class PrefabManager : MonoBehaviour {

    public static PrefabManager instance;

    [SerializeField]
    public Prefab[] prefabs;

    protected virtual void Awake() {
        instance = this;
    }

    public GameObject GetPrefab(string s) {
        foreach (Prefab p in prefabs) {
            if (p.name == s) {
                return p.prefab;
            }
        }
        return null;
    }
}
