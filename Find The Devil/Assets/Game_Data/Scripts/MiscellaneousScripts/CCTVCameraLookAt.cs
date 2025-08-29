using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTVCameraLookAt : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 2f;
    [SerializeField]
    private float rotationRange = 45f; // The total range of rotation (e.g., 45 degrees to -45 degrees)

    private Quaternion initialRotation;
    private float timeElapsed;

    void Start()
    {
        // Store the camera's initial rotation at the start of the game
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Increment time to drive the sine wave
        timeElapsed += Time.deltaTime * rotationSpeed;

        // Use a sine wave to get a smooth back-and-forth value between -1 and 1
        float newAngle = Mathf.Sin(timeElapsed) * rotationRange;

        // Apply the new angle to the initial rotation around the Y-axis
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, newAngle, 0);

        // Smoothly rotate the camera towards the new target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime);
    }
}