using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    public float gScore;
    public float hScore;
    public GameObject CameFrom;
    public bool Water = false, Grass = false, Dirt = false;

    public List<GameObject> ConnectedObjects;
    public bool Safe = false, Unsafe = false;

    void Start()
    {
        //ConnectedObjects = new List<GameObject>();
    }
    private void Update()
    {
        if (Safe)
            Unsafe = false;
    }

    public float FScore()
    {
        return gScore + hScore;
    }
}
