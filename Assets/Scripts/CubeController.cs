using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeController : ObjectManipulator
{
    

    public override ActionSet ActionSet
    {
        get => GameManager.instance.MenuSwitched ? ActionSet.A : ActionSet.B;
        
    }

    
}
