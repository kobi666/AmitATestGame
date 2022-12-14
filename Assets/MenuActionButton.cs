using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MenuActionButton : MonoBehaviour
{
    public ActionTypes ActionType;
    public Image Image;

    public void RunObjectAction()
    {
        if (ActionType != null)
        {
            MenuController.instance.CurrentObject?.CallAction(ActionType);
        }
    } 

}
