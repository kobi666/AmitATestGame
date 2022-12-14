using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[DefaultExecutionOrder(-2000)]
public class GameManager : MonoBehaviour
{
    public bool MenuSwitched = false;
    
    public int ObjectCounter = 0;
    
    /// <summary>
    /// used to store each Object in order to avoid using "GetComponent"
    /// </summary>
    public Dictionary<String, ObjectManipulator> AllObjects = new();

    public static GameManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    
    
    
    
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}


public enum ActionSet
{
    A,
    B
}
