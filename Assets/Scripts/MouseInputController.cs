using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputController : MonoBehaviour
{
    private Mouse Mouse;
    public static MouseInputController instance;
    private UserInput UserInput;
    public MenuController MenuController;
    


    [Button]
    public void OnMouseClick()
    {
        
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        Mouse = Mouse.current;
        if (UserInput == null)
        {
            UserInput = new UserInput();
            // Tell the "gameplay" action map that we want to get told about
            // when actions get triggered.
            
        }
        UserInput.MouseActions.Enable();
        UserInput.MouseActions.LeftButtonClicked.performed += ctx => MenuController.OnMouseClick();
        
        
        
        
    }

    private Vector3 trackedMousePosition;

    // Update is called once per frame
    void Update()
    {
        MenuController.TrackedMousePosition = Mouse.position.ReadValue();
    }
}
