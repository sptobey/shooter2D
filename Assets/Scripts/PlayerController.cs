using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    /* TODO: pull values from weapon properties that 
     * affect aim and move speed. 
     * Abstract all input to a separate class to be 
     * used by all controller classes.
     * Disable sprint if firing. */

    [Tooltip("Child object of weapon/pointer")]
    public GameObject aimBaseChildObject;

    [Tooltip("Base movement speed")]
    public float baseMoveSpeed = 3.0f;
    [Tooltip("ADS movement speed")]
    public float adsMoveSpeed = 1.0f;
    [Tooltip("Sprint movement speed")]
    public float sprintMoveSpeed = 5.0f;
    [Tooltip("Sprint lock timer")]
    public float sprintTime = 4.0f;
    [Tooltip("Maximum acceleration")]
    public float maxVelocityChange = 0.5f;

    [Tooltip("Base look speed (degrees)")]
    public float baseAimSpeed = 7.0f;
    [Tooltip("Base look speed (degrees)")]
    public float adsAimSpeed = 2.0f;
    [Tooltip("Sprint look speed (degrees)")]
    public float sprintAimSpeed = 1.0f;

    [Tooltip("Degrees offset from positive z-direction")]
    public float aimAngleOffset = -90;

    private Rigidbody2D rb;

    private Vector3 targetVelocity;
    private Vector3 targetDirection;

    [Tooltip("How hard to press 'Aim' to aim"), Range(-1.0f, 1.0f)]
    public float aimThreshold = 0.01f;

    private bool isAiming;
    private bool wasAimingReleased;
    private bool isSprinting;
    private bool isSprintLocked;

    private float moveSpeed;
    private float aimSpeed;

    void Start ()
    {
        rb = this.GetComponent<Rigidbody2D>();
        targetVelocity = Vector3.zero;
        targetDirection = Vector3.zero;
        isAiming = false;
        wasAimingReleased = false;
        isSprinting = false;
        moveSpeed = baseMoveSpeed;
        aimSpeed = baseAimSpeed;
    }
	
	void Update ()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        /* Input */
        targetVelocity.x = Input.GetAxis("Horizontal");
        targetVelocity.y = Input.GetAxis("Vertical");
        targetDirection.x = Input.GetAxis("Right_Stick_Horizontal");
        targetDirection.y = Input.GetAxis("Right_Stick_Vertical");
        bool wasAiming = isAiming; /* Previous frame */
        isAiming = (Input.GetAxis("L2_Axis") >= aimThreshold);
        wasAimingReleased = wasAiming && !isAiming;
        isSprinting = !isSprintLocked && Input.GetButtonDown("L3");

        if(isAiming)
        {
            moveSpeed = adsMoveSpeed;
            aimSpeed = adsAimSpeed;
        }
        else if(!isSprintLocked && isSprinting)
        {
            moveSpeed = sprintMoveSpeed;
            aimSpeed = sprintAimSpeed;
            StartCoroutine(SprintLockTimer(sprintTime));
        }
        else if(wasAimingReleased)
        {
            moveSpeed = baseMoveSpeed;
            aimSpeed = baseAimSpeed;
        }

        /* Player velocity */
        // Scale input to desired velocity
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= moveSpeed;

        // Calculate change in velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 deltaVelocity = (targetVelocity - currentVelocity);
        deltaVelocity.x = Mathf.Clamp(deltaVelocity.x, -maxVelocityChange, maxVelocityChange);
        deltaVelocity.y = Mathf.Clamp(deltaVelocity.y, -maxVelocityChange, maxVelocityChange);

        // Apply move to rigidbody
        rb.AddForce(deltaVelocity * rb.mass / Time.deltaTime);


        /* Look Direction */
        if (targetDirection.magnitude > 0)
        {
            // Calculate rotation
            targetDirection.Normalize();
            float targetAngle = aimAngleOffset + (Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg);
            Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

            // Apply rotation with slerp to aim object
            aimBaseChildObject.transform.rotation = Quaternion.Slerp(
                aimBaseChildObject.transform.rotation, targetRotation, Time.deltaTime * aimSpeed);
        }

        /* Set aim to move direction if not aiming */
        else
        {
            targetDirection.x = Input.GetAxis("Horizontal");
            targetDirection.y = Input.GetAxis("Vertical");
            if(targetDirection.magnitude > 0)
            {
                // Calculate rotation
                targetDirection.Normalize();
                float targetAngle = aimAngleOffset + (Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg);
                Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

                // Apply rotation with slerp to aim object
                aimBaseChildObject.transform.rotation = Quaternion.Slerp(
                    aimBaseChildObject.transform.rotation, targetRotation, Time.deltaTime * aimSpeed);
            }
        }

    }

    private IEnumerator SprintLockTimer(float time)
    {
        isSprintLocked = true;
        yield return new WaitForSeconds(time);
        moveSpeed = baseMoveSpeed;
        aimSpeed = baseAimSpeed;
        isSprinting = false;
        isSprintLocked = false;
    }
}
