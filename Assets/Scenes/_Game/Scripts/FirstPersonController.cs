using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.0f;
    public float acceleration = 10f;
    public float deceleration = 14f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.12f;
    public float maxLookAngle = 80.0f;
    [Range(5f, 30f)]
    public float lookSmoothness = 18.0f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.8f;
    public float bobAmplitudeY = 0.05f;
    public float bobAmplitudeX = 0.025f;

    [Header("Camera Tilt (Strafe)")]
    public bool enableCameraTilt = true;
    public float tiltAngle = 3f;
    public float tiltSpeed = 8f;

    [Header("Landing Impact")]
    public bool enableLandingImpact = true;
    public float impactDipAmount = 0.12f;
    public float impactRecoverSpeed = 7f;

    [Header("Breathing Idle")]
    public bool enableBreathing = true;
    public float breathFrequency = 0.4f;
    public float breathAmplitude = 0.008f;

    [Header("Audio Settings - Footsteps")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 smoothedVelocity;

    private float targetVerticalRotation = 0f;
    private float targetHorizontalRotation = 0f;
    private float currentVerticalRotation = 0f;
    private float currentHorizontalRotation = 0f;
    private float currentTilt = 0f;

    private float bobTimer = 0f;
    private float breathTimer = 0f;

    private bool wasGrounded = true;
    private float landingDip = 0f;
    private float stepTimer = 0f;

    private Vector3 cameraBaseLocalPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (footstepAudioSource == null) footstepAudioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetHorizontalRotation = transform.eulerAngles.y;
        currentHorizontalRotation = targetHorizontalRotation;

        if (cameraTransform != null)
            cameraBaseLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        // rotasi kamera
        Vector2 moveInput = Vector2.zero;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            targetHorizontalRotation += mouseDelta.x;
            targetVerticalRotation   -= mouseDelta.y;
            targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -maxLookAngle, maxLookAngle);

            currentHorizontalRotation = Mathf.LerpAngle(currentHorizontalRotation, targetHorizontalRotation, Time.deltaTime * lookSmoothness);
            transform.rotation = Quaternion.Euler(0f, currentHorizontalRotation, 0f);

            currentVerticalRotation = Mathf.Lerp(currentVerticalRotation, targetVerticalRotation, Time.deltaTime * lookSmoothness);
        }

        // input gerak
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
        }

        Vector3 targetVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed;
        float rate = (moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        smoothedVelocity = Vector3.MoveTowards(smoothedVelocity, targetVelocity, rate * moveSpeed * Time.deltaTime);

        controller.Move(smoothedVelocity * Time.deltaTime);

        // gravitasi
        bool isGroundedNow = controller.isGrounded;
        if (isGroundedNow && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // efek mendarat
        if (enableLandingImpact)
        {
            if (isGroundedNow && !wasGrounded)
            {
                float impactVelocity = Mathf.Abs(velocity.y);
                landingDip = Mathf.Clamp(impactVelocity * 0.015f, 0f, impactDipAmount);
            }
            landingDip = Mathf.MoveTowards(landingDip, 0f, Time.deltaTime * impactRecoverSpeed);
        }
        else
        {
            landingDip = 0f;
        }
        wasGrounded = isGroundedNow;

        // efek kamera jalan dan napas
        if (cameraTransform == null) return;

        float speedMagnitude = new Vector2(smoothedVelocity.x, smoothedVelocity.z).magnitude;
        Vector3 cameraOffset = Vector3.zero;

        // audio footstep
        if (isGroundedNow && speedMagnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime * (speedMagnitude / moveSpeed);
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0.05f;
        }

        if (enableHeadBob)
        {
            if (isGroundedNow && speedMagnitude > 0.1f)
                bobTimer += Time.deltaTime * bobFrequency * Mathf.PI * 2f;
            else
                bobTimer = Mathf.MoveTowards(bobTimer, 0f, Time.deltaTime * 5f);

            cameraOffset.y += Mathf.Sin(bobTimer) * bobAmplitudeY;
            cameraOffset.x += Mathf.Sin(bobTimer * 0.5f) * bobAmplitudeX;
        }

        if (enableBreathing)
        {
            breathTimer += Time.deltaTime * breathFrequency * Mathf.PI * 2f;
            float breathBlend = 1f - Mathf.Clamp01(speedMagnitude / 0.5f);
            cameraOffset.y += Mathf.Sin(breathTimer) * breathAmplitude * breathBlend;
        }

        cameraOffset.y -= landingDip;

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraBaseLocalPos + cameraOffset,
            Time.deltaTime * 20f
        );

        if (enableCameraTilt)
        {
            float targetTilt = -moveInput.x * tiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        }
        else
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltSpeed);
        }

        cameraTransform.localRotation = Quaternion.Euler(currentVerticalRotation, 0f, currentTilt);
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        AudioSource source = footstepAudioSource != null ? footstepAudioSource : (AudioManager.Instance != null ? AudioManager.Instance.sfxSource : null);
        if (source == null) return;

        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        if (clip != null)
        {
            source.pitch = Random.Range(0.92f, 1.08f);
            source.PlayOneShot(clip, footstepVolume);
        }
    }
}
