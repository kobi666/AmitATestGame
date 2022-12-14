using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyExtensions;

using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class ObjectManipulator : MonoBehaviour
{
    
    public MeshRenderer MeshRenderer;
    void RandomizeA()
    {
        actionA_randomized.Shuffle();
    }

    public int[] actionA_randomized = new[]
    {
        0,1,2,3
    };
    
    public int[] actionB_randomized = new[]
    {
        0, 1, 2
    };
    

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
    
    /// <summary>
    /// Assuming that constraints are predetermined in the game object inside unity, enables Gravity and removes
    /// rigidbody constraints
    /// </summary>
    /// <param name="token"></param>
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
    
    public Dictionary<ActionTypes, ActionContainer>  ObjectActionsSet_A = new();
    public Dictionary<ActionTypes, ActionContainer>  ObjectActionsSet_B = new();
    
    
    /// <summary>
    /// Used to call an Object action externally, according to action type
    /// </summary>
    /// <param name="actionType"></param>
    public async void CallAction(ActionTypes actionType)
    {
        var actions = ActionSet == ActionSet.A ? ObjectActionsSet_A : ObjectActionsSet_B;
        if (actions.ContainsKey(actionType))
        {
            var _action = actions[actionType];
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

    
    private static string objectName = "Object_";
    public async Task CloneObject(CancellationToken token)
    {
        var targetPosition = GetAvailablePositionAbove(transform.position, transform.localScale.x);
        ObjectManipulator newObject = GameObject.Instantiate(this, targetPosition, Quaternion.identity);
        newObject.name = $"{objectName}+{GameManager.instance.ObjectCounter++}";
        // waiting 2 frames for object to finish Start Method in order to maintain base object scale factor
        await Task.Delay(2);
        
        // changing the color once to force new instance of material for the instantiated object
        newObject.MeshRenderer.material.color = newObject.MeshRenderer.material.color;
    }

    public float GeneralLerpTime = 1.5f;
    
    
    public async Task ChangeColor(CancellationToken token)
    {
        var targetColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //var token = ObjectActions[ActionTypes.ChangeColor].Item2.Token;
        float counter = 0f;
        var currentColor = MyColor;
        while (!token.IsCancellationRequested && counter <= GeneralLerpTime)
        {
            counter += Time.deltaTime;
            MyColor = Color.Lerp(currentColor, targetColor, counter / GeneralLerpTime);
            await Task.Yield();
        }
    }


    public async Task EnlargeObject(CancellationToken token)
    {
        //var token = ObjectActions[ActionTypes.Enlarge].Item2.Token;
        float counter = 0f;
        var currentScale = transform.localScale.x;
        var targetScale = currentScale  * 1.1f;
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
        MenuController.instance.TryToOpenMenuAtDelay();
    }

    async Task RandomizeActions(CancellationToken token)
    {
        ActionsRandomized = true;
        actionA_randomized.Shuffle();
        actionB_randomized.Shuffle();
        MenuController.instance.TryToOpenMenuAtDelay();
    }


    // Start is called before the first frame update
    protected void Start()
    {
        GameManager.instance.AllObjects.Add(name, this);
        
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

/// <summary>
/// Contains each object's action sequences to later be stored in its action set dictionary
/// </summary>
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


