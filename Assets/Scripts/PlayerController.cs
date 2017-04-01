using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float baseMoveSpeed = 3.0f;
    public float maxVelocityChange = 0.5f;

    public float baseAimSpeed = 7.0f;
    public GameObject aimChildObject;
    public float aimAngleOffset = -90;

    private Rigidbody2D rb;

    private Vector3 targetVelocity;
    private Vector3 targetDirection;

    void Start ()
    {
        rb = this.GetComponent<Rigidbody2D>();
        targetVelocity = Vector3.zero;
        targetDirection = Vector3.zero;
    }
	
	void Update ()
    {
        /* Input */
        targetVelocity.x = Input.GetAxis("Horizontal");
        targetVelocity.y = Input.GetAxis("Vertical");
        targetDirection.x = Input.GetAxis("Right_Stick_Horizontal");
        targetDirection.y = Input.GetAxis("Right_Stick_Vertical");

        /* Player velocity */

        // Scale input to desired velocity
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= baseMoveSpeed;

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
            aimChildObject.transform.rotation = Quaternion.Slerp(
                aimChildObject.transform.rotation, targetRotation, Time.deltaTime * baseAimSpeed);
        }

        // TODO: Set aim to move direction if not aiming
        //else
        //{
        //    targetDirection.x = Input.GetAxis("Horizontal");
        //    targetDirection.x = Input.GetAxis("Vertical");
        //}

    }
}
