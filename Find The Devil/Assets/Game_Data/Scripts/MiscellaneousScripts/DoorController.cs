using UnityEngine;
using DG.Tweening; // Make sure you have this using directive for DOTween

public class DoorController : MonoBehaviour
{
    public Transform doorToAnimate; // Assign the actual door GameObject's Transform here in the Inspector
    public float openAngleZ = 90.0f; // How many degrees the door should rotate to open along its local Z-axis
    public float animationDuration = 1.0f; // How long the animation takes
    public Ease easeType = Ease.OutQuad; // The type of easing for the animation

    private Quaternion initialLocalRotation; // Store the door's initial local rotation
    private bool isOpen = false;

    void Awake()
    {
        if (doorToAnimate == null)
        {
            Debug.LogError("Door Transform not assigned! Please assign the door GameObject to 'doorToAnimate' in the Inspector.", this);
            enabled = false; // Disable the script if no door is assigned
            return;
        }
        initialLocalRotation = doorToAnimate.localRotation; // Store the door's starting local rotation
    }

    public void OpenThisDoor()
    {
        if (isOpen)
        {
            Debug.Log("Door is already open.");
            return;
        }

        // Calculate the target local rotation
        // We add the openAngleZ to the current local Euler Z-angle
        Vector3 targetEulerAngles = initialLocalRotation.eulerAngles;
        targetEulerAngles.z += openAngleZ; 

        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.WoodDoorOpen);
        // Animate the door's local rotation
        doorToAnimate.DOLocalRotate(targetEulerAngles, animationDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                Debug.Log("Door opened!");
                isOpen = true;
            });
    }

    // Optional: Add a method to close the door
    public void CloseThisDoor()
    {
        if (!isOpen)
        {
            Debug.Log("Door is already closed.");
            return;
        }

        // Animate the door back to its initial local rotation
        doorToAnimate.DOLocalRotate(initialLocalRotation.eulerAngles, animationDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                Debug.Log("Door closed!");
                isOpen = false;
            });
    }

    // Example of how you might trigger it (e.g., via a button click or collision)
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.O)) // Press 'O' to open
    //     {
    //         OpenThisDoor();
    //     }
    //     if (Input.GetKeyDown(KeyCode.C)) // Press 'C' to close
    //     {
    //         CloseThisDoor();
    //     }
    // }
}