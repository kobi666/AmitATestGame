using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CirculerMenuController : MonoBehaviour
{

    public RectTransform RectTransform;
    public EventTrigger EventTrigger;

    public event Action onPointerExit;

    public void SetPointerState(bool state)
    {
        PointerInsideMenu = state;
        if (!state)
        {
            onPointerExit?.Invoke();
        }
    }

    public bool PointerInsideMenu = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
