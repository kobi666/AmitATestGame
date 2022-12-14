using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputController : MonoBehaviour
{
    private Mouse Mouse;
    public static MouseInputController instance;
    private UserInput UserInput;
    public MenuController MenuController;
    
    void Start()
    {
        Mouse = Mouse.current;
        if (UserInput == null)
        {
            UserInput = new UserInput();
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
