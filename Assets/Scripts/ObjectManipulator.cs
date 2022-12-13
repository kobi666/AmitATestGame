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

public class ObjectManipulator : MonoBehaviour
{
    public MeshRenderer MeshRenderer;

   
    public float ObjectScaleBase;

    public Dictionary<ActionTypes, (Func<Task>, CancellationTokenSource)>  ObjectActions = new();


    public async void CallAction(ActionTypes actionType)
    {
        var _action = ObjectActions[actionType];
        _action.Item2?.Cancel();
        _action.Item2?.Dispose();
        _action.Item2 = new CancellationTokenSource();
        await Task.Yield();
        await _action.Item1.Invoke();
    }


    public Color MyColor
    {
        get => MeshRenderer.material.color;
        set => MeshRenderer.material.color = value;
    }

    
    [Button]
    public async Task CloneObject()
    {
        var targetPosition = GetAvailablePositionAbove(transform.position, transform.localScale.x);
        ObjectManipulator newObject = GameObject.Instantiate(this, targetPosition, Quaternion.identity);
        newObject.ObjectScaleBase = ObjectScaleBase;
    }

    public float GeneralLerpTime = 2f;
    
    [Button]
    public async Task ChangeColor()
    {
        var targetColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        var token = ObjectActions[ActionTypes.ChangeColor].Item2.Token;
        float counter = 0f;
        var currentColor = MyColor;
        while (!token.IsCancellationRequested && counter <= GeneralLerpTime)
        {
            MyColor = Color.Lerp(currentColor, targetColor, counter / GeneralLerpTime);
            await Task.Yield();
        }
    }


    public async Task ScaleObject()
    {
        
    }


    // Start is called before the first frame update
    void Start()
    {
        ObjectScaleBase = transform.localScale.x;
        
        ObjectActions.Add(ActionTypes.Clone, (CloneObject, null) );
        ObjectActions.Add(ActionTypes.ChangeColor, (ChangeColor, null));
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

    // [Button]
    // public void CheckIfSpaceAboveMeIsAvailable()
    // {
    //     Debug.LogWarning($"Space above is available : {CheckIfSpaceIsAvailable(transform.position, transform.localScale.x)}");
    // }

    // Vector3 GetPositionForCloning()
    // {
    //     return GetAvailablePositionAbove(transform.position, transform.localScale.x);
    // }

    // Update is called once per frame
    void Update()
    {
        
    }
}


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
