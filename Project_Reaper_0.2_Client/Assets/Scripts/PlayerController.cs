using Riptide;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform playerOrientation;
    [SerializeField] private Transform camTransform;
    [SerializeField] private float sensitivity;
    private bool[] inputs;
    private Vector2 move, look;
    private bool shouldDash, shouldJump, isSprinting, isCrouching, isInteracting, resetPlayer;
    public bool IsSprinting { get; private set; }
    public bool ShouldJump { get; private set; }
    private float yRotation;
    private float xRotation;

    [SerializeField] private Player player;

    public bool[] Inputs { get; private set; }

    //Unity's Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) { shouldJump = context.ReadValueAsButton(); }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            shouldDash = context.ReadValueAsButton();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
            isSprinting = true;
        if (context.canceled)
            isSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        isInteracting = context.ReadValueAsButton();
    }

    public void OnResetPlayer(InputAction.CallbackContext context)
    {
        resetPlayer = context.ReadValueAsButton();
    }

    private void Look()
    {
        //get mouse input
        yRotation += look.x * sensitivity;
        xRotation -= look.y * sensitivity;
        xRotation = Mathf.Clamp(xRotation, -85f, 50f);

        //rotate cam
        //camTransform.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        camTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void ConvertMovementVectorToBoolInputs(Vector2 vector, bool[] inputs)
    {
        // Convert y component to boolean
        // should be W and S
        inputs[0] = vector.y > 0f;
        inputs[1] = vector.y < 0f;

        // Convert x component to boolean
        // should be A and D
        inputs[2] = vector.x > 0f;
        inputs[3] = vector.x < 0f;
    }

    private void Start()
    {
        inputs = new bool[10];
        camTransform.transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleCursorMode();

        Debug.DrawRay(transform.position, transform.forward * 2f, Color.green);

        ConvertMovementVectorToBoolInputs(move, inputs);
        inputs[4] = shouldJump;
        inputs[5] = isSprinting;
        inputs[6] = shouldDash;
        inputs[7] = isCrouching;
        inputs[8] = isInteracting;
        inputs[9] = resetPlayer;
    }

    private void FixedUpdate()
    {
        if (player.dead == false)
        {
            SendInput();
            // as there is no way to set the value of the client to false after executing the movement script it has to be done like this
            shouldJump = false;
            shouldDash = false;
            isCrouching = false;
            isInteracting = false;
            resetPlayer = false;
        }
    }

    private void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
            Look();
    }

    public void SetCameraRotation(Vector3 newRotation)
    {
        yRotation = newRotation.y;
        // Apply the new rotation
        camTransform.rotation = Quaternion.Euler(newRotation);
    }

    #region Messages

    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector3(camTransform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion Messages
}