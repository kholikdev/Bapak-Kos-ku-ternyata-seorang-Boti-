using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    public Transform holdPoint;
    public float grabRange = 3.0f;
    public float grabForce = 180.0f;
    public float dragAmount = 10.0f;
    public float maxBreakDistance = 2.0f;
    public float throwForce = 8.0f;

    [Header("Visual References")]
    public DualRiggedStretchyHand stretchyHand;

    private Rigidbody targetRB;
    private float originalDrag;
    private CabinetDoor activeDoor;

    void Update()
    {
        bool isHoldingKey = (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                            (Keyboard.current != null && Keyboard.current.eKey.isPressed);

        if (isHoldingKey)
        {
            if (activeDoor != null)
            {
                Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
                activeDoor.DragDoor(mouseDelta);
            }
            else if (targetRB == null)
            {
                TryGrabObject();
            }
        }
        else
        {
            if (targetRB != null) ReleaseObject();
            if (activeDoor != null)
            {
                activeDoor.ReleaseDoor();
                activeDoor = null;

                if (stretchyHand != null) stretchyHand.DisableStretch();
            }
        }

        bool throwPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        if (throwPressed && targetRB != null) ThrowObject();

        if (targetRB != null)
        {
            float distance = Vector3.Distance(holdPoint.position, targetRB.position);
            if (distance > maxBreakDistance) ReleaseObject();
        }
    }

    void FixedUpdate()
    {
        if (targetRB != null)
        {
            Vector3 direction = holdPoint.position - targetRB.position;
            targetRB.linearVelocity = direction * (grabForce * Time.fixedDeltaTime);
        }
    }

    private void TryGrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grabRange))
        {
            if (hit.collider.CompareTag("MessyItem") && hit.rigidbody != null)
            {
                targetRB = hit.rigidbody;
                originalDrag = targetRB.linearDamping;
                targetRB.linearDamping = dragAmount;
                targetRB.useGravity = true;

                // Mode 2 tangan untuk barang
                if (stretchyHand != null)
                {
                    stretchyHand.EnableStretch(targetRB.transform, true);
                }
            }
            else if (hit.collider.CompareTag("Door"))
            {
                activeDoor = hit.collider.GetComponentInParent<CabinetDoor>();

                // Mode 1 tangan untuk pintu (Mengirimkan hit.point & hit.normal permukaan pintu)
                if (activeDoor != null && stretchyHand != null)
                {
                    stretchyHand.EnableStretch(activeDoor.transform, false, hit.point, hit.normal);
                }
            }
        }
    }

    public void ReleaseObject()
    {
        if (stretchyHand != null)
        {
            stretchyHand.DisableStretch();
        }

        if (targetRB != null)
        {
            targetRB.linearDamping = originalDrag;
            targetRB = null;
        }
    }

    private void ThrowObject()
    {
        Rigidbody rbToThrow = targetRB;
        ReleaseObject();
        rbToThrow.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }
}