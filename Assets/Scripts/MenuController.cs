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
   private int ObjectsLayerMask;
   

   public static MenuController instance;
   
   
   public MenuState MenuState = MenuState.Closed;
   private Camera camera;
   public Vector3 TrackedMousePosition;
   private CancellationTokenSource cts;
   public float MenuPopupTime = 0.8f;
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
    
   private void Start()
   {
      instance = this;
      camera = Camera.main;
      CirculerMenuController.onPointerExit += () => CloseCircularMenu();
      ObjectsLayerMask = LayerMask.GetMask("Objects");
   }
   // sets Menu actions according to what is defined per Object
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

   

   
   
   /// <summary>
   /// async sequence for opening the Object menu
   /// </summary>
   /// <param name="token"></param>
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
   
   /// <summary>
   /// async sequence for closing the object menu
   /// </summary>
   /// <param name="token"></param>
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
   /// <summary>
   /// Tries to find an object from the GameManager using a raycast
   /// </summary>
   /// <returns></returns>
   public ObjectManipulator TryGetClickedObject()
   {
      Ray ray = camera.ScreenPointToRay(TrackedMousePosition);
      Physics.Raycast(ray, out RaycastHit, 100f, ObjectsLayerMask);
      
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
   
   
   /// <summary>
   /// trys to open the object menu at delay, for added appearance of menu related effects
   /// </summary>
   public async void TryToOpenMenuAtDelay()
   {
      await Task.Delay(TimeSpan.FromSeconds(MenuCloseTime + 0.1f));
      if (MenuState == MenuState.Closed)
      {
         TryOpeningObjectActionMenu();
      }
   }
   
   
   /// <summary>
   /// The function trys to open the object menu provided that in the meantime another object was not set or nullified
   /// </summary>
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
   
   
   /// <summary>
   /// used to start the close menu sequence without re-opening it
   /// </summary>
   public async void CloseCircularMenu()
   {
      cts?.Cancel();
      cts?.Dispose();
      
      await Task.Yield();
      cts = new CancellationTokenSource();
      await CloseCirculerMenuSequence(cts.Token);
      MenuState = MenuState.Closed;
   }



   

   


   
   /// <summary>
   /// subscribed to user actions, and only triggered if the mouse pointer is outside of an open menu area
   /// </summary>
   public void OnMouseClick()
   {
      if (MenuState == MenuState.Open)
      {
         if (!CirculerMenuController.PointerInsideMenu)
         {
            CurrentObject = TryGetClickedObject();
            TryOpeningObjectActionMenu();
         } 
      }
      else
      {
         CurrentObject = TryGetClickedObject();
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
