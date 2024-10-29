using UnityEngine;
using System.Collections;


public class AdHocUnit : MonoBehaviour
{
    public float collisionDistance = 1f; // Distance to move the car back on collision
    float speed = 10f;
    public float turnAngle = 90f; // Angle to turn on trigger
    public float turnSpeed = 2f; // Speed of the turn

    private enum MovementState { Forward, Left, Right, PossibleLeft, PossibleRight, Adv ,Final  }
    private MovementState currentMovementState = MovementState.Forward;

    bool onTarget = false;

    void Update()
    {
        // Stop moving if we reached the target
        if (currentMovementState ==MovementState.Final)
            return;

        // Move forward continuously
        if (currentMovementState == MovementState.Forward)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime * -1);
        }

        // Trigger rotation based on state
        if (currentMovementState == MovementState.Left)
        {
            StartCoroutine(RotateCar(-turnAngle));
        }
        else if (currentMovementState == MovementState.Right)
        {
            StartCoroutine(RotateCar(turnAngle));
        }
        else if (currentMovementState == MovementState.PossibleLeft || currentMovementState == MovementState.PossibleRight)
        {
            // Randomly decide whether to turn or continue forward
            var d = Random.value;
            Debug.Log(d);
            bool shouldTurn = d > 0.5f;

            if (shouldTurn)
            {
                float turnDirection = currentMovementState == MovementState.PossibleLeft ? -turnAngle : turnAngle;
                StartCoroutine(RotateCar(turnDirection));
            }
            else
            {
                // If continuing forward, reset to Forward state
                currentMovementState = MovementState.Forward;
            }
        }
    }


    private IEnumerator MoveBackwardSmoothly(Vector3 direction, float duration)
    {

        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + (direction * collisionDistance);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Use Lerp to move smoothly from startPosition to endPosition
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // After moving backward, reset to the forward movement state
        currentMovementState = MovementState.Forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Seeker")
        {

            Vector3 collisionDirection = (other.transform.position - transform.position).normalized;
            Vector3 moveDirection = -collisionDirection; // Inverse of the collision direction

            // Move the other car in the opposite direction of the collision
            transform.position += moveDirection * 30f * Time.deltaTime;


        }
        if (currentMovementState == MovementState.Forward)
        {
            if (other.gameObject.name.Contains("possibleLeft"))
            {
                Destroy(other.gameObject);
                currentMovementState = MovementState.PossibleLeft;
            }
            else if (other.gameObject.name.Contains("possibleRight"))
            {
                Destroy(other.gameObject);
                currentMovementState = MovementState.PossibleRight;
            }
            else if (other.gameObject.name.Contains("Left"))
            {
                Destroy(other.gameObject);
                currentMovementState = MovementState.Left;
            }
            else if (other.gameObject.name.Contains("Right"))
            {
                Destroy(other.gameObject);
                currentMovementState = MovementState.Right;
            }
            else if (other.gameObject.name.Contains("Target"))
            {
                currentMovementState = MovementState.Final;
                onTarget = true;
            }
        }
    }

    

    private IEnumerator RotateCar(float angle)
    {
        // Reset the rotation state after turning
        currentMovementState = MovementState.Forward;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * angle);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed; // Controls the turning speed
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t); // Smooth rotation with Lerp
            yield return null;
        }
    }


}
