using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[DefaultExecutionOrder(-2000)]
public class GameManager : MonoBehaviour
{

    public MenuController MenuController;

    private bool mMenuSwitched = false;

    public event Action onMenuUpdate;

    public bool MenuSwitched
    {
        get => mMenuSwitched;
        set
        {
            mMenuSwitched = value;
            onMenuUpdate?.Invoke();
        }
    }
    public int ObjectCounter = 0;
    
    [ShowInInspector]
    public Dictionary<String, ObjectManipulator> AllObjects = new();

    public static GameManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        // initilize menu swap state on start
        MenuSwitched = false;
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
