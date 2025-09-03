using UnityEngine;
using DG.Tweening;

public class DoorController : MonoBehaviour
{
    public Transform doorToAnimate;
    public float openAngleZ = 90.0f; 
    public float openAngleY = 90.0f;
    public float animationDuration = 1.0f; 
    public Ease easeType = Ease.OutQuad; 
    
    [Tooltip("If checked, the door will open on the Y-axis. Otherwise, it will use the Z-axis.")]
    public bool isYAxis = false;
    
    private Quaternion initialLocalRotation;
    private bool isOpen = false;

    void Awake()
    {
        if (doorToAnimate == null)
        {
            Debug.LogError("Door Transform not assigned! Please assign the door GameObject to 'doorToAnimate' in the Inspector.", this);
            enabled = false;
            return;
        }
        initialLocalRotation = doorToAnimate.localRotation; 
    }

    public void OpenThisDoor()
    {
        if (isOpen)
        {
            Debug.Log("Door is already open.");
            return;
        }

        Vector3 targetEulerAngles = initialLocalRotation.eulerAngles;

        // Check the boolean to determine the rotation axis
        if (isYAxis)
        {
            targetEulerAngles.y += openAngleY;
        }
        else
        {
            targetEulerAngles.z += openAngleZ; 
        }

        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.WoodDoorOpen);
        doorToAnimate.DOLocalRotate(targetEulerAngles, animationDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                Debug.Log("Door opened!");
                isOpen = true;
            });
    }
    
    public void CloseThisDoor()
    {
        if (!isOpen)
        {
            Debug.Log("Door is already closed.");
            return;
        } 

        doorToAnimate.DOLocalRotate(initialLocalRotation.eulerAngles, animationDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                Debug.Log("Door closed!");
                isOpen = false;
            });
    }
}