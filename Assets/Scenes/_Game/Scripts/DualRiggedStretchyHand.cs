using UnityEngine;

public class DualRiggedStretchyHand : MonoBehaviour
{
    [Header("Right Arm Bones & IK")]
    public Transform ikTargetHandR;
    public Transform defaultHandRestPosR;
    public Transform upperArmR;
    public Transform foreArmR;

    [Header("Left Arm Bones & IK")]
    public Transform ikTargetHandL;
    public Transform defaultHandRestPosL;
    public Transform upperArmL;
    public Transform foreArmL;

    [Header("Stretch & Clamping Settings")]
    public float handClampingOffset = 0.25f;
    public float doorSurfaceOffset = 0.08f; // Offset dasar permukaan pintu
    public float followSpeed = 25f;
    public float returnSpeed = 15f;
    public float maxStretchMultiplier = 8f;

    private Transform currentGrabTarget;
    private bool isGrabbing = false;
    private bool isDualHandMode = true;

    private Vector3 localHitPoint;
    private Vector3 localHitNormal;
    private bool useSurfaceHit = false;

    private float initialArmLengthR, initialArmLengthL;
    private Vector3 initialUpperArmScaleR, initialForeArmScaleR;
    private Vector3 initialUpperArmScaleL, initialForeArmScaleL;

    void Start()
    {
        if (upperArmR != null && foreArmR != null && defaultHandRestPosR != null)
        {
            initialArmLengthR = Vector3.Distance(upperArmR.position, foreArmR.position) + Vector3.Distance(foreArmR.position, defaultHandRestPosR.position);
            initialUpperArmScaleR = upperArmR.localScale;
            initialForeArmScaleR = foreArmR.localScale;
        }

        if (upperArmL != null && foreArmL != null && defaultHandRestPosL != null)
        {
            initialArmLengthL = Vector3.Distance(upperArmL.position, foreArmL.position) + Vector3.Distance(foreArmL.position, defaultHandRestPosL.position);
            initialUpperArmScaleL = upperArmL.localScale;
            initialForeArmScaleL = foreArmL.localScale;
        }
    }

    void LateUpdate()
    {
        if (isGrabbing && currentGrabTarget != null)
        {
            Vector3 centerPos = currentGrabTarget.position;

            if (isDualHandMode)
            {
                // --- MODE 2 TANGAN (Barang) ---
                // Gunakan sumbu kanan Kamera agar posisi kanan/kiri TIDAK PERNAH terbalik saat barang berputar
                Vector3 cameraRight = Camera.main != null ? Camera.main.transform.right : transform.right;

                Vector3 rightSide = centerPos + (cameraRight * handClampingOffset);
                Vector3 leftSide = centerPos - (cameraRight * handClampingOffset);

                ikTargetHandR.position = Vector3.Lerp(ikTargetHandR.position, rightSide, Time.deltaTime * followSpeed);
                ikTargetHandL.position = Vector3.Lerp(ikTargetHandL.position, leftSide, Time.deltaTime * followSpeed);

                // Rotasi pergelangan tetap menghadap ke pusat barang
                Quaternion targetRotR = Quaternion.LookRotation(centerPos - ikTargetHandR.position, Vector3.up);
                Quaternion targetRotL = Quaternion.LookRotation(centerPos - ikTargetHandL.position, Vector3.up);

                ikTargetHandR.rotation = Quaternion.Slerp(ikTargetHandR.rotation, targetRotR, Time.deltaTime * followSpeed);
                ikTargetHandL.rotation = Quaternion.Slerp(ikTargetHandL.rotation, targetRotL, Time.deltaTime * followSpeed);

                ApplyArmStretch(upperArmR, foreArmR, ikTargetHandR, initialArmLengthR, initialUpperArmScaleR, initialForeArmScaleR);
                ApplyArmStretch(upperArmL, foreArmL, ikTargetHandL, initialArmLengthL, initialUpperArmScaleL, initialForeArmScaleL);
            }
            else
            {
                // --- MODE 1 TANGAN (Pintu Lemari) ---
                Vector3 targetHandPos;
                Quaternion targetHandRot;

                if (useSurfaceHit)
                {
                    Vector3 worldHitPos = currentGrabTarget.TransformPoint(localHitPoint);
                    Vector3 worldNormal = currentGrabTarget.TransformDirection(localHitNormal);

                    // 1. Hitung rasio melar lengan secara real-time
                    float currentDist = Vector3.Distance(upperArmR.position, worldHitPos);
                    float stretchRatio = currentDist / (initialArmLengthR > 0.001f ? initialArmLengthR : 1f);

                    // 2. Dynamic Offset: Makin melar/mundur pemain, offset dinaikkan agar mesh lengan tidak mendesak ke dalam pintu
                    float dynamicOffset = doorSurfaceOffset * (stretchRatio > 1.0f ? 1.0f + (stretchRatio - 1.0f) * 0.4f : 1.0f);

                    // 3. Vektor tarik ke bahu pemain agar posisi tangan tertarik ke luar dari arah sudut pandang
                    Vector3 dirToShoulder = (upperArmR.position - worldHitPos).normalized;

                    // Gabungkan Offset Normal Pintu + Offset Tarikan Bahu
                    targetHandPos = worldHitPos + (worldNormal * dynamicOffset) + (dirToShoulder * (dynamicOffset * 0.5f));

                    // Rotasi pergelangan menyesuaikan permukaan pintu
                    targetHandRot = Quaternion.LookRotation(-worldNormal, Vector3.up);
                }
                else
                {
                    targetHandPos = centerPos;
                    targetHandRot = currentGrabTarget.rotation;
                }

                ikTargetHandR.position = Vector3.Lerp(ikTargetHandR.position, targetHandPos, Time.deltaTime * followSpeed);
                ikTargetHandR.rotation = Quaternion.Slerp(ikTargetHandR.rotation, targetHandRot, Time.deltaTime * followSpeed);

                ResetLeftHandToDefault();
                ApplyArmStretch(upperArmR, foreArmR, ikTargetHandR, initialArmLengthR, initialUpperArmScaleR, initialForeArmScaleR);
            }
        }
        else
        {
            ResetHandsToDefault();
        }
    }

    private void ApplyArmStretch(Transform upperArm, Transform foreArm, Transform targetHand, float initialLength, Vector3 baseScaleUpper, Vector3 baseScaleFore)
    {
        if (upperArm == null || foreArm == null || targetHand == null || initialLength <= 0.001f) return;

        float currentDistance = Vector3.Distance(upperArm.position, targetHand.position);
        float stretchRatio = currentDistance / initialLength;

        if (stretchRatio > 1.0f)
        {
            stretchRatio = Mathf.Clamp(stretchRatio, 1.0f, maxStretchMultiplier);
            upperArm.localScale = new Vector3(baseScaleUpper.x, baseScaleUpper.y * stretchRatio, baseScaleUpper.z);
            foreArm.localScale = new Vector3(baseScaleFore.x, baseScaleFore.y * stretchRatio, baseScaleFore.z);
        }
        else
        {
            upperArm.localScale = baseScaleUpper;
            foreArm.localScale = baseScaleFore;
        }
    }

    public void EnableStretch(Transform target, bool useBothHands = true, Vector3 hitPoint = default, Vector3 hitNormal = default)
    {
        currentGrabTarget = target;
        isGrabbing = true;
        isDualHandMode = useBothHands;

        if (!useBothHands && hitPoint != Vector3.zero)
        {
            useSurfaceHit = true;
            localHitPoint = target.InverseTransformPoint(hitPoint);
            localHitNormal = target.InverseTransformDirection(hitNormal);
        }
        else
        {
            useSurfaceHit = false;
        }
    }

    public void DisableStretch()
    {
        isGrabbing = false;
        currentGrabTarget = null;
        useSurfaceHit = false;
    }

    private void ResetHandsToDefault()
    {
        if (ikTargetHandR != null && defaultHandRestPosR != null)
        {
            ikTargetHandR.position = Vector3.Lerp(ikTargetHandR.position, defaultHandRestPosR.position, Time.deltaTime * returnSpeed);
            ikTargetHandR.rotation = Quaternion.Slerp(ikTargetHandR.rotation, defaultHandRestPosR.rotation, Time.deltaTime * returnSpeed);
        }
        ResetLeftHandToDefault();

        if (upperArmR != null) upperArmR.localScale = initialUpperArmScaleR;
        if (foreArmR != null) foreArmR.localScale = initialForeArmScaleR;
    }

    private void ResetLeftHandToDefault()
    {
        if (ikTargetHandL != null && defaultHandRestPosL != null)
        {
            ikTargetHandL.position = Vector3.Lerp(ikTargetHandL.position, defaultHandRestPosL.position, Time.deltaTime * returnSpeed);
            ikTargetHandL.rotation = Quaternion.Slerp(ikTargetHandL.rotation, defaultHandRestPosL.rotation, Time.deltaTime * returnSpeed);
        }
        if (upperArmL != null) upperArmL.localScale = initialUpperArmScaleL;
        if (foreArmL != null) foreArmL.localScale = initialForeArmScaleL;
    }
}