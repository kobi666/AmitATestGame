using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SphereController : ObjectManipulator
{
    

    public override ActionSet ActionSet
    {
        get =>  GameManager.instance.MenuSwitched ? ActionSet.B : ActionSet.A;
        
    }

    void Start()
    {
        base.Start();
        
    }
    
    
}

