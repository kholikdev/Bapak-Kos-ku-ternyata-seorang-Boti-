using UnityEngine;

public enum RotationAxis { X, Y, Z }
public enum DragAxis { Horizontal, Vertical }

public class CabinetDoor : MonoBehaviour
{
    [Header("Pivot Reference")]
    public Transform doorPivot;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public RotationAxis rotationAxis = RotationAxis.Y;
    public bool invertRotation = false;
    public bool invertDrag = false;
    public DragAxis dragAxis = DragAxis.Horizontal;

    [Header("Smooth & Sensitivity Settings")]
    [Range(0.005f, 0.08f)]
    public float dragSensitivity = 0.025f;
    [Range(5f, 30f)]
    public float smoothSpeed = 18f;

    [Header("Magnetic Snap Settings")]
    [Range(0.02f, 0.2f)]
    public float snapThreshold = 0.1f;

    [Header("Current Status (Read Only)")]
    [Range(0f, 1f)]
    public float currentRatio = 0f;

    [Header("Audio Settings")]
    public AudioSource doorAudioSource;
    public AudioClip doorMoveSound;
    public float moveSpeedThreshold = 0.02f;
    [Range(0f, 1f)] public float doorVolume = 1f;

    private float targetRatio = 0f;
    private bool isBeingDragged = false;
    private Quaternion initialLocalRotation;
    private float lastRatio = 0f;
    private float stopTimer = 0f;
    private bool isThisDoorPlaying = false;

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;
        initialLocalRotation = doorPivot.localRotation;

        // Pastikan setiap pintu memiliki AudioSource sendiri (independen) agar tidak saling mematikan suara (Stop)
        if (doorAudioSource == null || doorAudioSource.gameObject != gameObject)
        {
            AudioSource localSource = GetComponent<AudioSource>();
            if (localSource == null)
            {
                localSource = gameObject.AddComponent<AudioSource>();
                localSource.playOnAwake = false;
                localSource.spatialBlend = 0.7f; // 3D spatial dengan blend nyaman
                localSource.minDistance = 1f;
                localSource.maxDistance = 25f;
            }
            doorAudioSource = localSource;
        }

        // Jika pintu ini belum dipasangi klip, pinjam dari pintu lain yang sudah ada
        if (doorMoveSound == null)
        {
            CabinetDoor[] allDoors = FindObjectsByType<CabinetDoor>(FindObjectsSortMode.None);
            foreach (var d in allDoors)
            {
                if (d != null && d.doorMoveSound != null)
                {
                    doorMoveSound = d.doorMoveSound;
                    break;
                }
            }
        }

        lastRatio = currentRatio;
    }

    void Update()
    {
        // magnet pintu
        if (!isBeingDragged)
        {
            if (targetRatio < snapThreshold)
                targetRatio = 0f;
            else if (targetRatio > (1f - snapThreshold))
                targetRatio = 1f;
            else
                targetRatio = (targetRatio > 0.5f) ? 1f : 0f;
        }

        currentRatio = Mathf.Lerp(currentRatio, targetRatio, Time.deltaTime * smoothSpeed);

        // putar suara saat ada pergerakan rotasi (loop selama ditarik, stop seketika saat berhenti)
        float ratioSpeed = Mathf.Abs(currentRatio - lastRatio) / Mathf.Max(Time.deltaTime, 0.0001f);
        bool isMoving = ratioSpeed > moveSpeedThreshold;

        if (doorAudioSource != null && doorMoveSound != null)
        {
            if (isMoving)
            {
                stopTimer = 0.12f; // buffer toleransi jeda frame mouse agar mulus

                if (!isThisDoorPlaying || !doorAudioSource.isPlaying)
                {
                    doorAudioSource.clip = doorMoveSound;
                    doorAudioSource.loop = true;
                    doorAudioSource.volume = doorVolume;
                    doorAudioSource.pitch = Random.Range(0.96f, 1.04f);
                    doorAudioSource.Play();
                    isThisDoorPlaying = true;
                }
            }
            else
            {
                if (stopTimer > 0f)
                {
                    stopTimer -= Time.deltaTime;
                }
                else if (isThisDoorPlaying)
                {
                    doorAudioSource.Stop(); // berhenti seketika saat tarikan berhenti
                    isThisDoorPlaying = false;
                }
            }
        }
        lastRatio = currentRatio;

        // rotasi pintu
        float dirMultiplier = invertRotation ? -1f : 1f;
        float currentAngle = currentRatio * openAngle * dirMultiplier;

        Vector3 eulerAngle;
        if (rotationAxis == RotationAxis.X)      eulerAngle = new Vector3(currentAngle, 0f, 0f);
        else if (rotationAxis == RotationAxis.Z) eulerAngle = new Vector3(0f, 0f, currentAngle);
        else                                     eulerAngle = new Vector3(0f, currentAngle, 0f);
        doorPivot.localRotation = initialLocalRotation * Quaternion.Euler(eulerAngle);
    }

    public void DragDoor(Vector2 mouseDelta)
    {
        isBeingDragged = true;

        // tarik pintu
        float pullMultiplier = invertDrag ? -1f : 1f;
        float rawInput = (dragAxis == DragAxis.Vertical) ? mouseDelta.y : mouseDelta.x;

        targetRatio += rawInput * pullMultiplier * dragSensitivity;
        targetRatio = Mathf.Clamp01(targetRatio);
    }

    public void ReleaseDoor()
    {
        isBeingDragged = false;
    }

    public bool IsClosed()
    {
        return currentRatio <= snapThreshold;
    }
}
