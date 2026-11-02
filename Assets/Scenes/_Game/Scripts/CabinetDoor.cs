using UnityEngine;

public class CabinetDoor : MonoBehaviour
{
    [Header("Pivot Reference")]
    public Transform doorPivot;

    [Header("Door Settings")]
    [Tooltip("Sudut bukaan maksimal (SELALU ISI ANGKA POSITIF, misal: 90)")]
    public float openAngle = 90f;

    [Tooltip("Centang jika arah rotasi pintu kebalikan (misal berputar ke kiri/negatif)")]
    public bool invertRotation = false;

    [Tooltip("Centang jika geseran mouse kanan/kiri terasa kebalikan saat membuka pintu ini")]
    public bool invertDrag = false;

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

    private float targetRatio = 0f;
    private bool isBeingDragged = false;
    private Quaternion initialLocalRotation;

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;
        initialLocalRotation = doorPivot.localRotation;
    }

    void Update()
    {
        // 1. Logika Magnet saat mouse dilepas
        if (!isBeingDragged)
        {
            if (targetRatio < snapThreshold)
            {
                targetRatio = 0f; // Jepret Tutup
            }
            else if (targetRatio > (1f - snapThreshold))
            {
                targetRatio = 1f; // Jepret Buka
            }
            else
            {
                targetRatio = (targetRatio > 0.5f) ? 1f : 0f;
            }
        }

        // 2. Haluskan pergerakan ratio
        currentRatio = Mathf.Lerp(currentRatio, targetRatio, Time.deltaTime * smoothSpeed);

        // 3. Hitung rotasi lokal bersih
        float dirMultiplier = invertRotation ? -1f : 1f;
        float currentAngle = currentRatio * openAngle * dirMultiplier;

        doorPivot.localRotation = initialLocalRotation * Quaternion.Euler(0f, currentAngle, 0f);
    }

    public void DragDoor(Vector2 mouseDelta)
    {
        isBeingDragged = true;

        // Menggunakan mouseDelta.x untuk geseran Horizontal (Kanan/Kiri)
        float pullMultiplier = invertDrag ? -1f : 1f;
        float pullInput = mouseDelta.x * pullMultiplier;

        targetRatio += pullInput * dragSensitivity;
        targetRatio = Mathf.Clamp01(targetRatio);
    }

    public void ReleaseDoor()
    {
        isBeingDragged = false;
    }

    // Dipanggil oleh EndingEvaluator untuk mengecek apakah pintu tertutup rapat
    public bool IsClosed()
    {
        return currentRatio <= snapThreshold; // Mengembalikan true jika bukaan <= 10%
    }
}