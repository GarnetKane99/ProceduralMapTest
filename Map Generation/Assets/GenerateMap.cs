using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        Generation Generator = FindObjectOfType<Generation>();
        
        if(!Generator.GeneratingMap)
            Generator.GenerateMap();
    }
}
