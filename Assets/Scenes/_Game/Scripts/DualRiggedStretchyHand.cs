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
    public float doorSurfaceOffset = 0.08f;
    public float followSpeed = 25f;
    public float returnSpeed = 15f;
    public float maxStretchMultiplier = 8f;

    [Header("Audio Settings")]
    public AudioSource rubberAudioSource;
    public AudioClip rubberStretchSound;
    public float stretchDeltaThreshold = 0.05f;
    [Range(0f, 1f)] public float rubberVolume = 0.8f;

    private Transform currentGrabTarget;
    private bool isGrabbing = false;
    private bool isDualHandMode = true;

    private Vector3 localHitPoint;
    private Vector3 localHitNormal;
    private bool useSurfaceHit = false;

    private float initialArmLengthR, initialArmLengthL;
    private Vector3 initialUpperArmScaleR, initialForeArmScaleR;
    private Vector3 initialUpperArmScaleL, initialForeArmScaleL;

    private float lastAudioStretchRatio = 1.0f;
    private float rubberStopTimer = 0f;

    void Start()
    {
        // Pastikan tangan punya AudioSource sendiri agar tidak membajak sfxSource global
        if (rubberAudioSource == null)
        {
            rubberAudioSource = GetComponent<AudioSource>();
            if (rubberAudioSource == null)
            {
                rubberAudioSource = gameObject.AddComponent<AudioSource>();
                rubberAudioSource.playOnAwake = false;
                rubberAudioSource.spatialBlend = 0f;
            }
        }

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
                // pegang dua tangan
                Vector3 cameraRight = Camera.main != null ? Camera.main.transform.right : transform.right;

                Vector3 rightSide = centerPos + (cameraRight * handClampingOffset);
                Vector3 leftSide = centerPos - (cameraRight * handClampingOffset);

                ikTargetHandR.position = Vector3.Lerp(ikTargetHandR.position, rightSide, Time.deltaTime * followSpeed);
                ikTargetHandL.position = Vector3.Lerp(ikTargetHandL.position, leftSide, Time.deltaTime * followSpeed);

                Quaternion targetRotR = Quaternion.LookRotation(centerPos - ikTargetHandR.position, Vector3.up);
                Quaternion targetRotL = Quaternion.LookRotation(centerPos - ikTargetHandL.position, Vector3.up);

                ikTargetHandR.rotation = Quaternion.Slerp(ikTargetHandR.rotation, targetRotR, Time.deltaTime * followSpeed);
                ikTargetHandL.rotation = Quaternion.Slerp(ikTargetHandL.rotation, targetRotL, Time.deltaTime * followSpeed);

                ApplyArmStretch(upperArmR, foreArmR, ikTargetHandR, initialArmLengthR, initialUpperArmScaleR, initialForeArmScaleR);
                ApplyArmStretch(upperArmL, foreArmL, ikTargetHandL, initialArmLengthL, initialUpperArmScaleL, initialForeArmScaleL);
            }
            else
            {
                // pegang satu tangan
                Vector3 targetHandPos;
                Quaternion targetHandRot;

                if (useSurfaceHit)
                {
                    Vector3 worldHitPos = currentGrabTarget.TransformPoint(localHitPoint);
                    Vector3 worldNormal = currentGrabTarget.TransformDirection(localHitNormal);

                    float currentDist = Vector3.Distance(upperArmR.position, worldHitPos);
                    float stretchRatio = currentDist / (initialArmLengthR > 0.001f ? initialArmLengthR : 1f);

                    float dynamicOffset = doorSurfaceOffset * (stretchRatio > 1.0f ? 1.0f + (stretchRatio - 1.0f) * 0.4f : 1.0f);

                    Vector3 dirToShoulder = (upperArmR.position - worldHitPos).normalized;
                    targetHandPos = worldHitPos + (worldNormal * dynamicOffset) + (dirToShoulder * (dynamicOffset * 0.5f));
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

        // efek tangan melar
        float currentDistance = Vector3.Distance(upperArm.position, targetHand.position);
        float stretchRatio = currentDistance / initialLength;

        if (upperArm == upperArmR)
        {
            UpdateRubberAudio(stretchRatio);
        }

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

    private void UpdateRubberAudio(float stretchRatio)
    {
        if (rubberAudioSource == null || rubberStretchSound == null) return;

        float stretchDeltaSpeed = Mathf.Abs(stretchRatio - lastAudioStretchRatio) / Mathf.Max(Time.deltaTime, 0.0001f);
        bool isStretchingNow = (stretchRatio > 1.02f) && (stretchDeltaSpeed > stretchDeltaThreshold) && isGrabbing;

        if (isStretchingNow)
        {
            rubberStopTimer = 0.08f;
            if (!rubberAudioSource.isPlaying)
            {
                rubberAudioSource.clip = rubberStretchSound;
                rubberAudioSource.loop = true;
                rubberAudioSource.volume = rubberVolume;
                rubberAudioSource.pitch = Random.Range(0.96f, 1.04f);
                rubberAudioSource.Play();
            }
        }
        else
        {
            if (rubberStopTimer > 0f)
            {
                rubberStopTimer -= Time.deltaTime;
            }
            else if (rubberAudioSource.isPlaying)
            {
                rubberAudioSource.Stop();
            }
        }

        lastAudioStretchRatio = stretchRatio;
    }

    private void StopRubberAudio()
    {
        if (rubberAudioSource != null && rubberAudioSource.isPlaying)
        {
            rubberAudioSource.Stop();
        }
        lastAudioStretchRatio = 1.0f;
        rubberStopTimer = 0f;
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
        StopRubberAudio();
    }

    private void ResetHandsToDefault()
    {
        StopRubberAudio();
        // balik ke posisi awal
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
