using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    //Creates an instance of the input map for each player
    public InputSystem_Actions InputActions { get; private set; }

    private void Awake()
    {
        InputActions = new InputSystem_Actions();
        InputActions.Enable();
    }

    private void OnDisable()
    {
        InputActions.Disable();
    }
}
