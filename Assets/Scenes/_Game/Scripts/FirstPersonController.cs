using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.0f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.12f;
    public float maxLookAngle = 80.0f;
    [Range(5f, 30f)]
    public float lookSmoothness = 18.0f; // Makin besar nilainya, makin responsif & mulus

    private CharacterController controller;
    private Vector3 velocity;

    private float targetVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float currentVerticalRotation = 0f;
    private float currentHorizontalRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetHorizontalRotation = transform.eulerAngles.y;
        currentHorizontalRotation = targetHorizontalRotation;
    }

    void Update()
    {
        // 1. Mouse Look dengan Interpolasi Smooth (Lerp)
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            // Hitung target sudut rotasi
            targetHorizontalRotation += mouseDelta.x;
            targetVerticalRotation -= mouseDelta.y;
            targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -maxLookAngle, maxLookAngle);

            // Interpolasi rotasi horizontal (badan player)
            currentHorizontalRotation = Mathf.LerpAngle(currentHorizontalRotation, targetHorizontalRotation, Time.deltaTime * lookSmoothness);
            transform.rotation = Quaternion.Euler(0f, currentHorizontalRotation, 0f);

            // Interpolasi rotasi vertikal (kamera)
            currentVerticalRotation = Mathf.Lerp(currentVerticalRotation, targetVerticalRotation, Time.deltaTime * lookSmoothness);
            cameraTransform.localRotation = Quaternion.Euler(currentVerticalRotation, 0f, 0f);
        }

        // 2. Movement WASD
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
        }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 3. Simple Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}