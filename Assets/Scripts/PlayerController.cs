using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions actions;
    private Rigidbody playerRB;

    private Vector3 movementVectorX;
    private Vector3 movementVectorZ;
    private Vector3 movementInput;

    private float xRotation = 0f;
    private float yRotation = 0f;

    [Header("Movement Settings")]
    [SerializeField]
    private float playerSpeed = 10;

    [Header("Look Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 20f;

    private void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Enable();
        playerRB = GetComponent<Rigidbody>();
        
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovementInput();
        
    }
    private void FixedUpdate()
    {
        Vector3 targetVelocity = movementInput.normalized * playerSpeed;
        playerRB.linearVelocity = new Vector3(targetVelocity.x, playerRB.linearVelocity.y, targetVelocity.z);
        

    }
    private void LateUpdate()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        Vector2 lookInput = actions.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Yukarý-aþaðý bakýþ: Sadece kamerayý X ekseninde döndürür ve baþ aþaðý dönmeyi engeller
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Saða-sola bakýþ: Ana Player gövdesini Y ekseninde döndürür

        yRotation += mouseX;
        playerRB.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));
    }
    private void ReadMovementInput()
    {
        Vector2 input = actions.Player.Move.ReadValue<Vector2>();
        movementVectorX = input.x * transform.right;
        movementVectorZ = input.y * transform.forward;

        movementInput = movementVectorX + movementVectorZ;
    }
}
