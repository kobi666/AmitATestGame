using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class MenuController : MonoBehaviour
{
   public ObjectManipulator CurrentObject;
   private int ObjectsMask;

   public static MenuController instance;

   public CirculerMenuController CirculerMenuController;

   public List<ActionToSprite> ActionsToSprites = new();

   public List<MenuActionButton> ActionSet_A_buttons = new();
   public List<MenuActionButton> ActionSet_B_buttons = new();

   void SetActionMenu_A()
   {
      var actionSet = CurrentObject.ObjectActionsSet_A.ToArray();
      
      if (CurrentObject.ActionsRandomized)
      {
         // get indexes from Object
      }
      else
      {
         var actionArray = actionSet.ToArray();
         for (int i = 0; i < actionArray.Length; i++)
         {
            var action = actionArray[i];
            ActionSet_A_buttons[i].Image.sprite =
               ActionsToSprites.First(x => x.ActionType == actionArray[i].Key).Sprite;
            ActionSet_A_buttons[i].ActionType = action.Key;
         }
      }
   }

   void SetActionMenu_B()
   {
      var actionSet = CurrentObject.ObjectActionsSet_B.ToArray();
      if (CurrentObject.ActionsRandomized)
      {
         // get indexes from Object
      }
      else
      {
         var actionArray = actionSet.ToArray();
         for (int i = 0; i < actionArray.Length; i++)
         {
            var action = actionArray[i];
            ActionSet_B_buttons[i].Image.sprite =
               ActionsToSprites.First(x => x.ActionType == actionArray[i].Key).Sprite;
            ActionSet_B_buttons[i].ActionType = action.Key;
         }
      }
   }
    
   
   
   public void SetMenuActions()
   {
      bool actionSet_A_or_B = CurrentObject.ActionSet == ActionSet.A;
      var actionSet = actionSet_A_or_B
         ? CurrentObject.ObjectActionsSet_A
         : CurrentObject.ObjectActionsSet_B;
      if (actionSet_A_or_B)
      {
         
      }
      else
      {
         if (CurrentObject.ActionsRandomized)
         {

         }
         else
         {
            var _actionArray = actionSet.ToArray();
            for (int i = 0; i < _actionArray.Length; i++)
            {
               var _action = _actionArray[i];
               ActionSet_B_buttons[i].Image.sprite =
                  ActionsToSprites.First(x => x.ActionType == _actionArray[i].Key).Sprite;
               ActionSet_B_buttons[i].ActionType = _action.Key;
            }
         }
      }
   }

   private void Start()
   {
      instance = this;
      camera = Camera.main;
      
      ObjectsMask = LayerMask.GetMask("Objects");
   }

   public MenuState MenuState = MenuState.Disabled;
   private Camera camera;

   public Vector3 TrackedMousePosition;

   public void UpdateMenuActions()
   {
      if (CurrentObject != null)
      {
         
      }
   }

   private RaycastHit RaycastHit;
   
   [Button]
   public ObjectManipulator GetRaycastedObject()
   {
      Ray ray = camera.ScreenPointToRay(TrackedMousePosition);
      Physics.Raycast(ray, out RaycastHit, 100f, ObjectsMask);
      
      if (RaycastHit.collider != null)
      {
         Debug.DrawLine(camera.transform.position, RaycastHit.collider.transform.position, Color.blue, 5f);
         try
         {
            var obj = GameManager.instance.AllObjects[RaycastHit.collider.name] ?? null;
            if (obj != null)
            {
               var objposition = new Vector2(obj.transform.position.x, obj.transform.position.y);
               
               var screenPoint = camera.WorldToScreenPoint(obj.transform.position);
               CirculerMenuController.RectTransform.position = screenPoint;
               
               return obj;
            }
         }
         catch (Exception e)
         {
          Debug.LogWarning(e);  
         }
      }
      return null;
   }
   
   

   public void OnMouseClick()
   {
      switch (MenuState)
      {
         case MenuState.Disabled:
            var v = GetRaycastedObject();
            break;
      }
   }
   
   

}

public enum MenuState {
   Disabled,
   Open,
}


[Serializable]
public class ActionToSprite
{
   public ActionTypes ActionType;
   public Sprite Sprite;

}
