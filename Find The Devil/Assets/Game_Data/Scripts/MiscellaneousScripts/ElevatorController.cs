using System; 
using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    [Header("Door Animation Settings")]
    [SerializeField] private float doorOpenDistance = 2.0f;
    [SerializeField] private float doorAnimationSpeed = 1.0f;

    [Header("Elevator Movement Settings")]
    [SerializeField] private Transform[] floorPositions;
    [SerializeField] private float elevatorMoveSpeed = 1.0f;

    private Vector3 leftDoorOriginalPosition;
    private Vector3 rightDoorOriginalPosition;
    private Vector3 leftDoorOpenTargetPosition;
    private Vector3 rightDoorOpenTargetPosition;

    private bool isAnimatingDoors = false;
    private bool isMovingElevator = false;
    private int currentFloorIndex = 0;

    private bool isLastFloor = false;
    private bool isFirstFloor = false;
    public SplineData splineData;

    // References to active coroutines
    private Coroutine animateDoorsCoroutine;
    private Coroutine moveElevatorSequenceCoroutine;

    [SerializeField] private bool isCrane;

    void Awake()
    {
        
        if (!isCrane)
        {
            if (leftDoor == null || rightDoor == null)
            {
                enabled = false;
                return;
            }

            leftDoorOriginalPosition = leftDoor.transform.localPosition;
            rightDoorOriginalPosition = rightDoor.transform.localPosition;

            leftDoorOpenTargetPosition = leftDoorOriginalPosition - new Vector3(doorOpenDistance, 0, 0);
            rightDoorOpenTargetPosition = rightDoorOriginalPosition + new Vector3(doorOpenDistance, 0, 0);

            leftDoor.transform.localPosition = leftDoorOpenTargetPosition;
            rightDoor.transform.localPosition = rightDoorOpenTargetPosition;
        }
        
        if (floorPositions.Length > 0)
        {
            transform.position = floorPositions[0].position;
        }
    }

    public void Restart()
    {
        if (!isCrane)
        {
            if (leftDoor == null || rightDoor == null)
            {
                Debug.LogError("Elevator doors are not assigned on Restart! Disabling ElevatorController.", this);
                enabled = false;
                return;
            }

            leftDoorOriginalPosition = leftDoor.transform.localPosition;
            rightDoorOriginalPosition = rightDoor.transform.localPosition;

            leftDoorOpenTargetPosition = leftDoorOriginalPosition - new Vector3(doorOpenDistance, 0, 0);
            rightDoorOpenTargetPosition = rightDoorOriginalPosition + new Vector3(doorOpenDistance, 0, 0);

            leftDoor.transform.localPosition = leftDoorOpenTargetPosition;
            rightDoor.transform.localPosition = rightDoorOpenTargetPosition;
        }

        if (floorPositions.Length > 0)
        {
            transform.position = floorPositions[0].position;
        }
        currentFloorIndex = 0; 
        isLastFloor = false;  
        isAnimatingDoors = false; 
        isMovingElevator = false; 
    }
    
    public void OpenDoors()
    {
        if (isAnimatingDoors) return;
        
       
        if (isCrane)
        {
            isAnimatingDoors = false; 
            ExecutePostDoorAnimationLogic();
            return;
        }
       // GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Elevator_OpenDoors);
        animateDoorsCoroutine = StartCoroutine(AnimateDoors(true));
        
        if(!isLastFloor)
            StartCoroutine(DelayedBlockerSwitch(1.5f));
    }

    public void CloseDoors()
    {
        if (isAnimatingDoors) return;
        
        if (isCrane)
        {
            isAnimatingDoors = false; 
            return; 
        }

       // GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Elevator_CloseDoors);
        animateDoorsCoroutine = StartCoroutine(AnimateDoors(false));
    }

    private IEnumerator AnimateDoors(bool open)
    {
        isAnimatingDoors = true;

        if (isCrane)
        {
            isAnimatingDoors = false;
            ExecutePostDoorAnimationLogic();
            yield break; 
        }

        float timer = 0f;

        Vector3 startLeftPos = leftDoor.transform.localPosition;
        Vector3 startRightPos = rightDoor.transform.localPosition;

        Vector3 targetLeftPos = open ? leftDoorOpenTargetPosition : leftDoorOriginalPosition;
        Vector3 targetRightPos = open ? rightDoorOpenTargetPosition : rightDoorOriginalPosition;

        while (Vector3.Distance(leftDoor.transform.localPosition, targetLeftPos) > 0.01f ||
               Vector3.Distance(rightDoor.transform.localPosition, targetRightPos) > 0.01f)
        {
            timer += Time.deltaTime * doorAnimationSpeed;
            leftDoor.transform.localPosition = Vector3.Lerp(startLeftPos, targetLeftPos, timer);
            rightDoor.transform.localPosition = Vector3.Lerp(startRightPos, targetRightPos, timer);
            yield return null;
        }

        leftDoor.transform.localPosition = targetLeftPos;
        rightDoor.transform.localPosition = targetRightPos;

        isAnimatingDoors = false;
        ExecutePostDoorAnimationLogic(); 
    }

    private void ExecutePostDoorAnimationLogic()
    {
        if (isLastFloor)
        {
            if (splineData != null)
            {
                splineData.SetNewMovementSpeed(4);
            }
        }
        
    }

    private IEnumerator DelayedBlockerSwitch(float delay)
    {
        Debug.Log(" DelayedBlockerSwitch(float delay)");
        yield return new WaitForSecondsRealtime(delay);
        if (GameManager.Instance._waitForTryGun)
        {
            GameManager.Instance._waitForTryGun = false;
        }
        GameManager.Instance.uiManager.SwitchBlocker(false);
    }

    public void MoveElevator(float time)
    {
        if (isMovingElevator || isAnimatingDoors || floorPositions.Length == 0) return;
        
        moveElevatorSequenceCoroutine = StartCoroutine(MoveElevatorSequence(time));
    }

    private IEnumerator MoveElevatorSequence(float time)
    {
        if(isLastFloor) yield break;
        
        yield return new WaitForSecondsRealtime(time);
        
        isMovingElevator = true;
       
        // Close doors only if it's an elevator
        if (!isCrane)
        {
            CloseDoors();
            while (isAnimatingDoors)
            {
                yield return null;
            }
        }

        currentFloorIndex = (currentFloorIndex + 1) % floorPositions.Length;
        Vector3 targetPosition = floorPositions[currentFloorIndex].position;

        float journeyLength = Vector3.Distance(transform.position, targetPosition);
        float startTime = Time.time;

        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Elevator_GoingUP);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            float distCovered = (Time.time - startTime) * elevatorMoveSpeed;
            float fractionOfJourney = journeyLength > 0 ? Mathf.Clamp01(distCovered / journeyLength) : 1f; 
            transform.position = Vector3.Lerp(transform.position, targetPosition, fractionOfJourney);
            yield return null;
        }

       // transform.position = targetPosition;

        if (isFirstFloor)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            GameManager.Instance.playerController.GunScannerSwitch();
        }

        //yield return new WaitForSecondsRealtime(0.5f);

        // Open doors only if it's an elevator
        if (!isCrane)
        {
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Elevatorbell);
          
           
            
            OpenDoors();
            while (isAnimatingDoors)
            {
                yield return null;
            }
        }
        else
        {
            
            // If it's a crane, ensure post-door-open logic still runs after arriving at floor
            ExecutePostDoorAnimationLogic();
        }
        
        
        isMovingElevator = false;
        //Elevator changes
        //yield return new WaitForSecondsRealtime(5f);
        yield return new WaitForSecondsRealtime(3.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FirstFloor"))
        {
            if (splineData != null)
            {
               // splineData.ActivateTools();
            }
        }
        else
        {
            isFirstFloor = true;
        }
       
        if(other.CompareTag("LastFloor"))
        {
            isLastFloor = true;
        }
    }
}