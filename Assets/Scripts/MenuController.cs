using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MenuController : MonoBehaviour
{
   public ObjectManipulator CurrentObject;
   private int ObjectsMask;
   private int UIMask;

   public static MenuController instance;
   
   

   public GameObject ActionSet_A_parentObject;
   public GameObject ActionSet_B_parentObject;

   public CirculerMenuController CirculerMenuController;

   public List<ActionToSprite> ActionsToSprites = new();

   public List<MenuActionButton> ActionSet_A_buttons = new();
   public List<MenuActionButton> ActionSet_B_buttons = new();

   void SetActionMenu_A()
   {
      var actionSet = CurrentObject.ObjectActionsSet_A.ToArray();
      
      
      if (CurrentObject.ActionsRandomized)
      {
         for (int i = 0; i < CurrentObject.actionA_randomized.Length; i++)
         {
            var randomizedIndex = CurrentObject.actionA_randomized[i];
            var action = actionSet[randomizedIndex];
            ActionSet_A_buttons[i].Image.sprite =
               ActionsToSprites.First(x => x.ActionType == actionSet[randomizedIndex].Key).Sprite;
            ActionSet_A_buttons[i].ActionType = action.Key;
         }
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
         for (int i = 0; i < CurrentObject.actionB_randomized.Length; i++)
         {
            var randomizedIndex = CurrentObject.actionB_randomized[i];
            var action = actionSet[randomizedIndex];
            ActionSet_B_buttons[i].Image.sprite =
               ActionsToSprites.First(x => x.ActionType == actionSet[randomizedIndex].Key).Sprite;
            ActionSet_B_buttons[i].ActionType = action.Key;
         }
      }
      else
      {
         
         for (int i = 0; i < actionSet.Length; i++)
         {
            var action = actionSet[i];
            ActionSet_B_buttons[i].Image.sprite =
               ActionsToSprites.First(x => x.ActionType == actionSet[i].Key).Sprite;
            ActionSet_B_buttons[i].ActionType = action.Key;
         }
      }
   }
    
   
   
   public void SetMenuActions()
   {
      bool actionSet_A_or_B = CurrentObject.ActionSet == ActionSet.A;
      ActionSet_A_parentObject.SetActive(actionSet_A_or_B);
      ActionSet_B_parentObject.SetActive(!actionSet_A_or_B);
      var actionSet = actionSet_A_or_B
         ? CurrentObject.ObjectActionsSet_A
         : CurrentObject.ObjectActionsSet_B;
      if (actionSet_A_or_B)
      {
         SetActionMenu_A();
      }
      else
      {
         SetActionMenu_B();
      }
      
   }

   private void Start()
   {
      instance = this;
      camera = Camera.main;
      CirculerMenuController.onPointerExit += () => CloseCircularMenu();
      ObjectsMask = LayerMask.GetMask("Objects");
      UIMask = LayerMask.GetMask("UI");
   }

   public MenuState MenuState = MenuState.Closed;
   private Camera camera;

   public Vector3 TrackedMousePosition;
   
   

   public void UpdateMenuActions()
   {
      if (CurrentObject != null)
      {
         SetMenuActions();
      }
   }

   private CancellationTokenSource cts;

   public float MenuPopupTime = 0.8f;
   async Task OpenCircularMenuSequence(CancellationToken token)
   {
      CirculerMenuController.RectTransform.localScale = Vector3.zero;
      // var objposition = new Vector2(transform1.position.x, transform1.position.y);
               
      var screenPoint = camera.WorldToScreenPoint(CurrentObject.transform.position);
      CirculerMenuController.RectTransform.position = screenPoint;
      float counter = 0f;
      while (!token.IsCancellationRequested && counter <= MenuPopupTime)
      {
         counter += Time.deltaTime;
         float div = counter / MenuPopupTime;
         CirculerMenuController.RectTransform.localScale =
            new Vector3(Mathf.SmoothStep(0, 1, div), Mathf.SmoothStep(0, 1, div));
         await Task.Yield();
      }
   }

   public float MenuCloseTime = 0.2f;
   async Task CloseCirculerMenuSequence(CancellationToken token)
   {
      float currentScale = CirculerMenuController.RectTransform.localScale.x;
      float counter = 0f;
      
      while (!token.IsCancellationRequested && counter <= MenuCloseTime)
      {
         counter += Time.deltaTime;
         float div = counter / MenuCloseTime;
         CirculerMenuController.RectTransform.localScale =
            new Vector3(Mathf.SmoothStep(currentScale, 0, div), Mathf.SmoothStep(currentScale, 0, div));
         await Task.Yield();
      }
   }

   private RaycastHit RaycastHit;
   
   
   public ObjectManipulator GetRaycastedObject()
   {
      Ray ray = camera.ScreenPointToRay(TrackedMousePosition);
      Physics.Raycast(ray, out RaycastHit, 100f, ObjectsMask);
      
      if (RaycastHit.collider != null)
      {
         
         try
         {
            var obj = GameManager.instance.AllObjects[RaycastHit.collider.name] ?? null;
            return obj;
         }
         catch (Exception e)
         {
          Debug.LogWarning(e);  
         }
      }
      return null;
   }

   public async void TryToOpenMenuAtDelay()
   {
      await Task.Delay(TimeSpan.FromSeconds(MenuCloseTime + 0.1f));
      if (MenuState == MenuState.Closed)
      {
         TryOpeningObjectActionMenu();
      }
   }
   
   

   public async void TryOpeningObjectActionMenu()
   {
      if (CurrentObject != null)
      {
         cts?.Cancel();
         cts?.Dispose();
         
         await Task.Yield();
         cts = new CancellationTokenSource();
         if (MenuState == MenuState.Open)
         {
            await CloseCirculerMenuSequence(cts.Token);
            cts = new CancellationTokenSource();
         }
         SetMenuActions();
         await OpenCircularMenuSequence(cts.Token);
         MenuState = MenuState.Open;
      }
   }
   
   public async void CloseCircularMenu()
   {
      cts?.Cancel();
      cts?.Dispose();
      
      await Task.Yield();
      cts = new CancellationTokenSource();
      await CloseCirculerMenuSequence(cts.Token);
      MenuState = MenuState.Closed;
   }



   bool CheckIfMouseIsInsideMenuArea()
   {
      var mousePosition = (Vector2)camera.ScreenToWorldPoint(TrackedMousePosition);
      if (CirculerMenuController.RectTransform.rect.Contains(mousePosition))
      {
         return true;
      }

      return false;
   }

   private bool MouseInsideMenuArea = false;


   

   public void OnMouseClick()
   {
      
      
      if (MenuState == MenuState.Open)
      {
         if (!CirculerMenuController.PointerInsideMenu)
         {
            CurrentObject = GetRaycastedObject();
            TryOpeningObjectActionMenu();
         } 
      }
      else
      {
         CurrentObject = GetRaycastedObject();
         TryOpeningObjectActionMenu();
      }

   }
   
   

}

public enum MenuState {
   Closed,
   Open,
}


[Serializable]
public class ActionToSprite
{
   public ActionTypes ActionType;
   public Sprite Sprite;

}
