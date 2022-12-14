using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class ObjectManipulator : MonoBehaviour
{
    public MeshRenderer MeshRenderer;

    private static string objectName = "Object_";
    

    public Rigidbody Rigidbody;

    public abstract ActionSet ActionSet { get; }

    public bool ActionsRandomized = false;
    public Dictionary<ActionTypes, ActionContainer> GetObjectActions()
    {
        if (ActionSet == ActionSet.A)
        {
            return ObjectActionsSet_A;
        }

        return ObjectActionsSet_B;
    }
        

    private bool IsMenuOrderShuffled = false;
    //public bool ActionSet


    private bool mFallingEnabled = false;
    public bool FallingEnabled
    {
        get => mFallingEnabled;
        set
        {
            mFallingEnabled = value;
            if (value)
            {
                Rigidbody.useGravity = true;
                Rigidbody.constraints = RigidbodyConstraints.None;
            }
        }
    }
    
    
    // assuming that constraints are predetermined in the game object inside unity
    async Task EnableFalling(CancellationToken token)
    {
        if (!FallingEnabled)
        {
            FallingEnabled = true;
        }
    }

    public float DegreesToRotate = 90f;

    async Task RotateOnYAxis(CancellationToken token)
    {
        float counter = 0f;
        var currentYRotation = transform.rotation;
        var TargetRotation = currentYRotation * Quaternion.Euler(new Vector3(0, DegreesToRotate,0));
        
        while (!token.IsCancellationRequested && counter <= GeneralLerpTime)
        {
            counter += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(currentYRotation, TargetRotation, counter / GeneralLerpTime);
            await Task.Yield();
        }
    }
    
    
   
    public float ObjectScaleBase;

    public Dictionary<ActionTypes, ActionContainer>  ObjectActionsSet_A = new();
    public Dictionary<ActionTypes, ActionContainer>  ObjectActionsSet_B = new();

    
    [Button]
    public async void CallAction(ActionTypes actionType)
    {
        if (ObjectActionsSet_A.ContainsKey(actionType))
        {
            var _action = ObjectActionsSet_A[actionType];
            _action.cts?.Cancel();
            _action.cts?.Dispose();
            await Task.Yield();
            _action.cts = new CancellationTokenSource();
            var token = _action.cts.Token;
            await _action.ActionTask.Invoke(token);
        }
    }


    public Color MyColor
    {
        get => MeshRenderer.material.color;
        set => MeshRenderer.material.color = value;
    }

    
    [Button]
    public async Task CloneObject(CancellationToken token)
    {
        var targetPosition = GetAvailablePositionAbove(transform.position, transform.localScale.x);
        ObjectManipulator newObject = GameObject.Instantiate(this, targetPosition, Quaternion.identity);
        newObject.name = $"{objectName}+{GameManager.instance.ObjectCounter++}";
        // waiting 2 frames for object to finish Start Method in order to maintain base object scale factor
        await Task.Delay(2);
        newObject.ObjectScaleBase = ObjectScaleBase;
    }

    public float GeneralLerpTime = 1.5f;
    
    [Button]
    public async Task ChangeColor(CancellationToken token)
    {
        var targetColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //var token = ObjectActions[ActionTypes.ChangeColor].Item2.Token;
        float counter = 0f;
        var currentColor = MyColor;
        while (!token.IsCancellationRequested && counter <= GeneralLerpTime)
        {
            MyColor = Color.Lerp(currentColor, targetColor, counter / GeneralLerpTime);
            await Task.Yield();
        }
    }


    public async Task EnlargeObject(CancellationToken token)
    {
        //var token = ObjectActions[ActionTypes.Enlarge].Item2.Token;
        float counter = 0f;
        var currentScale = transform.localScale.x;
        var targetScale = currentScale + ObjectScaleBase * 0.1f;
        while (!token.IsCancellationRequested && counter <= GeneralLerpTime)
        {
            counter += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one * currentScale, Vector3.one * targetScale,
                counter / GeneralLerpTime);
            
            await Task.Yield();
        }
    }

    async Task SwitchMenuActions(CancellationToken token)
    {
        GameManager.instance.MenuSwitched = !GameManager.instance.MenuSwitched;
    }

    async Task RandomizeActions(CancellationToken token)
    {
        
    }


    // Start is called before the first frame update
    public void Start()
    {
        GameManager.instance.AllObjects.Add(name, this);
        ObjectScaleBase = transform.localScale.x;
        ObjectActionsSet_A.Add(ActionTypes.Clone, new ActionContainer(CloneObject, "Clone") );
        ObjectActionsSet_A.Add(ActionTypes.ChangeColor, new ActionContainer(ChangeColor, "Change Color"));
        ObjectActionsSet_A.Add(ActionTypes.Enlarge, new ActionContainer(EnlargeObject, "Enlarge"));
        ObjectActionsSet_A.Add(ActionTypes.Fall, new ActionContainer(EnableFalling, "Fall"));
        
        ObjectActionsSet_B.Add(ActionTypes.Rotate, new ActionContainer(RotateOnYAxis, "Rotate"));
        ObjectActionsSet_B.Add(ActionTypes.Switch, new ActionContainer(SwitchMenuActions, "Switch"));
        ObjectActionsSet_B.Add(ActionTypes.Randomize, new ActionContainer(RandomizeActions, "Randomize"));
    }
    /// <summary>
    /// Assuming that scale XYZ is synchronyzed, provides position above current object available for cloning
    /// </summary>
    /// <param name="originPosition"></param>
    /// <param name="ObjectScale"></param>
    /// <returns></returns>
    public Vector3 GetAvailablePositionAbove(Vector3 originPosition, float ObjectScale)
    {
        var targetPosition = originPosition + (Vector3.up * ObjectScale) + (MinimumVerticalSpaceBetweenObjects * Vector3.up);
        Collider[] results = new Collider[4];
        Physics.OverlapBoxNonAlloc(targetPosition, (Vector3.one * ObjectScale) / 2f, results, Quaternion.identity);
        return !results.Any(x => x != null) ? targetPosition : GetAvailablePositionAbove(targetPosition, ObjectScale);
    }

    public float MinimumVerticalSpaceBetweenObjects;

    
}

[Serializable]
public enum ActionTypes
{
    Enlarge,
    Clone,
    Fall,
    Randomize,
    ChangeColor,
    Rotate,
    Switch
}


public class ActionContainer
{
    public CancellationTokenSource cts;
    public Func<CancellationToken, Task> ActionTask;
    public String ActionDescription;

    public ActionContainer(Func<CancellationToken,Task> actionTask, String actionDescription)
    {
        ActionTask = actionTask;
        ActionDescription = actionDescription;
    }
}
