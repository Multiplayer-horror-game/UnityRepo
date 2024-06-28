using System.Collections;
using Network;
using Unity.Netcode;
using Unity.Netcode.Components;
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
    [SerializeField] private float lookXLimit = 40f; // Mouse sensitivity
    private float speedDampTime = 0.1f; // Time it takes for the character to reach the desired speed
    private float speedThreshold = 0.01f; // Threshold for stopping the character's movement
    private Vector2 speedValue; // Current speed value used for animation
    private CharacterController controller; // Reference to the CharacterController component
    private Vector3 playerVelocity; // Velocity of the player

    private bool isInCar;

    //Footsteps
    [SerializeField] public AudioSource footstepSound;

    // Camera
    [SerializeField] private float cameraYOffset = 1.0f; // Vertical offset for the camera position
    private Camera playerCamera; // Reference to the main camera

    //FlashLight
    private GameObject flashlight;
    private NetworkVariable<bool> flashlightState = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        flashlight = transform.Find("FlashLight").gameObject;

        //attach player to car
        StartCoroutine(WaitAndGetCar());

        if (!IsOwner) return; // Only execute on the client that owns this object

        // Set up the camera
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            // No main camera found, create a new one
            GameObject cameraGameObject = new GameObject("PlayerCamera");
            playerCamera = cameraGameObject.AddComponent<Camera>();
            cameraGameObject.AddComponent<AudioListener>(); // Optionally add an AudioListener if needed
        }

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

    private IEnumerator WaitAndGetCar()
    {
        yield return new WaitForSeconds(0.5f);
        //attach player to car
        NetworkCar networkCar = NetworkCar.Instance;

        if (networkCar != null)
        {
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            collider.enabled = false;

            NetworkTransform networkTransform = GetComponent<NetworkTransform>();
            networkTransform.enabled = false;

            networkCar.AttachPlayerToCarServerRpc(NetworkManager.LocalClientId);
            isInCar = true;


            animator.SetBool("SittingTrigger", true);

        }
        else
        {
            isInCar = false;
            animator.SetBool("SittingTrigger", false);
        }
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
        if (flashlightState.Value != flashlight.activeSelf)
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

        // Calculate and clamp the target vertical rotation
        float targetVerticalRotation = playerCamera.transform.localEulerAngles.x - lookInput.y * mouseSensitivity * Time.deltaTime;
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -lookXLimit, lookXLimit);

        if (targetVerticalRotation > 180f) targetVerticalRotation -= 360f;
        else if (targetVerticalRotation < -180f) targetVerticalRotation += 360f;

        // Horizontal rotation input
        float horizontalRotation = lookInput.x * mouseSensitivity * Time.deltaTime;

        if (isInCar)
        {
            // Rotate the character horizontally (left/right) around the car
            playerCamera.transform.Rotate(Vector3.up * horizontalRotation, Space.Self);

            // Directly apply the clamped vertical rotation to the camera
            playerCamera.transform.localEulerAngles = new Vector3(targetVerticalRotation, playerCamera.transform.localEulerAngles.y, 0);
            flashlight.transform.localEulerAngles = new Vector3(targetVerticalRotation, flashlight.transform.localEulerAngles.y, 0);
        }
        else
        {
            // Rotate the character horizontally (left/right)
            transform.Rotate(Vector3.up * horizontalRotation, Space.Self);

            // Directly apply the clamped vertical rotation to the camera
            playerCamera.transform.localEulerAngles = new Vector3(targetVerticalRotation, playerCamera.transform.localEulerAngles.y, 0);
            flashlight.transform.localEulerAngles = new Vector3(targetVerticalRotation, flashlight.transform.localEulerAngles.y, 0);
        }
    }


    void HandleMovement()
    {
        if (isInCar) return;

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

        if (isInCar) return;

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

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            footstepSound.enabled = true;
        }
        else
        {
            footstepSound.enabled = false;
        }

        // Apply rotation to the intended movement direction
        Vector3 move = transform.TransformDirection(moveInput) * targetSpeed;
        move += Physics.gravity;

        // Apply movement to the character
        controller.Move(move * Time.deltaTime);
    }
    
    public void LeaveCar()
    {
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        collider.enabled = true;
        
        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        networkTransform.enabled = true;
        
        transform.SetParent(null);
        isInCar = false;
        animator.SetBool("SittingTrigger", false);
        
        playerCamera.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
    }

    public void ResetCameraRotation()
    {
        playerCamera.transform.rotation = Quaternion.EulerAngles(0, 0, 0);
    }

}