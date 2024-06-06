using Unity.Netcode;
using UnityEngine;

public class CharacterMovement : NetworkBehaviour
{
    // Animation
    Animator animator; // Reference to the Animator component
    int speedXHash; // Hash code for the "speedX" parameter in the Animator
    int speedYHash; // Hash code for the "speedY" parameter in the Animator

    // Input
    PlayerInput input; // Reference to the PlayerInput component for input handling
    Vector2 currentMovement; // Current movement direction based on input
    bool movementPressed; // Flag indicating if the movement input is pressed
    bool runPressed; // Flag indicating if the run input is pressed

    // Movement
    [SerializeField] private float moveSpeed = 1.75f; // Speed for forward/backward movement
    [SerializeField] private float strafeSpeed = 1.5f; // Speed for sideways movement
    [SerializeField] private float backwardsSpeed = 1.0f; // Speed modifier for backward movement
    [SerializeField] private float runSpeed = 8f; // Speed modifier for running
    [SerializeField] private float mouseSensitivity = 10f; // Mouse sensitivity
    [SerializeField] private float lookXLimit = 40.0f; // Mouse sensitivity
    private float speedDampTime = 0.1f; // Time it takes for the character to reach the desired speed
    private float speedThreshold = 0.01f; // Threshold for stopping the character's movement
    private Vector2 speedValue; // Current speed value used for animation
    private CharacterController controller; // Reference to the CharacterController component
    private Vector3 playerVelocity; // Velocity of the player

    // Camera
    [SerializeField] private float cameraYOffset = 1.0f; // Vertical offset for the camera position
    private Camera playerCamera; // Reference to the main camera
    
    //FlashLight
    private GameObject flashlight;
    private NetworkVariable<bool> flashlightState = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        
        flashlight = transform.Find("FlashLight").gameObject;
        
        if (!IsOwner) return; // Only execute on the client that owns this object

        // Set up the camera
        playerCamera = Camera.main;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        playerCamera.transform.SetParent(transform);

        // Set up input handling
        input = new PlayerInput();
        input.Player.Move.performed += ctx => currentMovement = ctx.ReadValue<Vector2>();
        input.Player.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();

        // Get components
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Initialize animation parameters
        speedXHash = Animator.StringToHash("speedX");
        speedYHash = Animator.StringToHash("speedY");

        // Enable input
        input.Player.Enable();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (!IsOwner) return; // Only execute on the client that owns this object
        input.Player.Disable(); // Disable input when the component is disabled
    }

    void Update()
    {
        HandleFlashlight(); // Handle flashlight state
        
        if (!IsOwner) return; // Only execute on the client that owns this object

        HandleMovement(); // Calculate movement values
        HandleRotation(); // Calculate and apply rotation values
        ApplyMovement(); // Apply movement to the character
        HandleFlashlightInput(); //handle flashlight input
    }
    
    private void HandleFlashlight()
    {
        if(flashlightState.Value != flashlight.activeSelf)
        {
            flashlight.SetActive(flashlightState.Value);
        }
    }

    private void HandleFlashlightInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            flashlightState.Value = !flashlightState.Value;
        }
    }

    void HandleRotation()
    {
        // Get the look input from the player input
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();

        // Rotate the character horizontally (left/right)
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.deltaTime, Space.Self);

        // Rotate the camera vertically
        float targetRotation = playerCamera.transform.localEulerAngles.x - lookInput.y * mouseSensitivity * Time.deltaTime;

        // Wrap the target rotation around 360 degrees
        if (targetRotation > 180f) targetRotation -= 360f;
        else if (targetRotation < -180f) targetRotation += 360f;

        // Clamp the target rotation within the desired range
        targetRotation = Mathf.Clamp(targetRotation, -lookXLimit, lookXLimit);

        // Directly apply the clamped rotation to the camera
        playerCamera.transform.localEulerAngles = new Vector3(targetRotation, playerCamera.transform.localEulerAngles.y, 0);
        flashlight.transform.localEulerAngles = new Vector3(targetRotation, flashlight.transform.localEulerAngles.y, 0);
    }


    void HandleMovement()
    {
        // Get the current speed values from the Animator
        float speedX = animator.GetFloat(speedXHash);
        float speedY = animator.GetFloat(speedYHash);

        // Lerp the speed values towards the current movement direction
        float newSpeedX = Mathf.Lerp(speedX, currentMovement.x, Time.deltaTime / speedDampTime);
        float newSpeedY = Mathf.Lerp(speedY, currentMovement.y, Time.deltaTime / speedDampTime);

        // Set speed values to zero if they are below the threshold
        newSpeedX = Mathf.Abs(newSpeedX) < speedThreshold ? 0f : newSpeedX;
        newSpeedY = Mathf.Abs(newSpeedY) < speedThreshold ? 0f : newSpeedY;

        // Set the new speed values in the Animator
        animator.SetFloat(speedXHash, newSpeedX);
        animator.SetFloat(speedYHash, newSpeedY);
        speedValue = new Vector2(newSpeedX, newSpeedY);
    }

    void ApplyMovement()
    {
        // Calculate the intended movement direction based on input
        Vector3 moveInput = new Vector3(currentMovement.x, 0f, currentMovement.y);
        moveInput = moveInput.normalized;

        // Determine the target speed based on intended movement direction and run input
        float targetSpeed;
        if (moveInput.z > 0)
        {
            // Forward movement
            targetSpeed = runPressed ? runSpeed : moveSpeed;
        }
        else if (moveInput.z < 0)
        {
            // Backward movement
            targetSpeed = runPressed ? runSpeed * backwardsSpeed : backwardsSpeed;
        }
        else
        {
            // Sideways movement
            targetSpeed = runPressed ? runSpeed * strafeSpeed : strafeSpeed;
        }

        // Adjust speed for diagonal movement
        if (moveInput.x != 0 && moveInput.z != 0)
        {
            targetSpeed *= 0.7071f; // Approximation of 1/sqrt(2) for diagonal movement
        }

        // Apply rotation to the intended movement direction
        Vector3 move = transform.TransformDirection(moveInput) * targetSpeed;
        move += Physics.gravity;

        // Apply movement to the character
        controller.Move(move * Time.deltaTime);
    }

}