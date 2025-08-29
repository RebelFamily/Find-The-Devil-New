using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using ITHappy;
using UnityEngine.EventSystems;

public class ScannerLogic : MonoBehaviour
{
    private float zCoord; 
    private bool zCoordInitialized = false; 

    public bool IsActive { get; private set; } = false;
    [Header("Scan Settings (Raycast)")]
    [SerializeField] private float scanRange = 1000f; 
    [SerializeField] private LayerMask scanDetectionLayer;
    [SerializeField] private string targetHeadTag = "EnemyHead";
    [SerializeField] private Animator scannerAnimator;
    private bool isAnimationPlayed = false;
    [Header("LookAt Target")]
    [Tooltip("The Transform this scanner should orient itself towards.")]
    [SerializeField] private Transform lookAtTarget;
    public static event Action OnScannerUsed;
    private bool _effectsActivated = false;
    private bool isDragging = false;

    private bool antinaCheck = false;
    [SerializeField] private GameObject antina;
    [SerializeField] private MeshRenderer scannerMesh;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private GameObject scannerEffects;
    public Transform handlePoint;
    private Color _originalBlackMaterialColor;

    private void Awake()
    {
        blackMaterial = scannerMesh.materials[0];
        if (blackMaterial != null)
        {
            _originalBlackMaterialColor = blackMaterial.color;
        }
        SetBlackMaterialToPureBlack();
    }

    public void Init()
    {
        Activate();
    }

    public void Activate()
    {
       
        IsActive = true;
        if (!zCoordInitialized && Camera.main != null)
        {
            zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            zCoordInitialized = true;
            lookAtTarget = GameManager.Instance.playerController.toolHandler;
        }
        else if (Camera.main == null)
        {
            Debug.LogError("ScannerLogic: Main Camera not found! Cannot determine Z-coordinate for snapping/dragging.");
        }
        
        
    }

    public void Deactivate()
    {
        if (antina != null)
        {
            DOTweenAnimation rotationAnimator = antina.GetComponent<DOTweenAnimation>();
            if (rotationAnimator != null)
            {
                rotationAnimator.DOPause();
            }
        }
        scannerEffects.SetActive(false);
        antinaCheck = false;
        IsActive = false;
        _effectsActivated = false;
        if (isAnimationPlayed && scannerAnimator != null)
        {
            
            isAnimationPlayed = false;
            // Set speed to 1 to play forward
            scannerAnimator.speed = -1f; 
            // Play the animation from its beginning (normalizedTime = 0f) using its index (0)
            scannerAnimator.Play(0, 0, 0f);
        }
        SetBlackMaterialToPureBlack();
        GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Scanner_Activate,false);
    }
    
    public void ScanArea()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = -transform.forward; 
        
        if (Physics.Raycast(origin, direction, out hit, scanRange, scanDetectionLayer))
        {
           // Debug.Log("check for the raycasting");
            OnScannerUsed?.Invoke(); 
        }
        
    }

    public void SetLookAtTarget(Transform target)
    {
        lookAtTarget = target;
        if (lookAtTarget != null)
        {
            Debug.Log($"ScannerLogic: LookAt target set to '{lookAtTarget.name}'.");
            if (IsActive) 
            {
                 transform.LookAt(lookAtTarget.position);
            }
        }
        else
        {
            Debug.LogWarning("ScannerLogic: LookAt target set to null. Scanner will stop looking at a specific point.");
        }
    }

    void Update()
    {
       
        if (!IsActive) return;
        
        if (Camera.main == null || !zCoordInitialized)
        {
            Debug.LogWarning("ScannerLogic: Cannot move scanner. Main Camera not found or Z-coordinate not initialized.");
            return;
        } 

        bool isPointerOverUI = false;

        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        #else
        if (Input.touchCount > 0)
        {
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        #endif

        if (isPointerOverUI && !isDragging)
        {
            return; 
        }

        if (Input.GetMouseButton(0))
        {
            if (antina != null) 
            {
                DOTweenAnimation rotationAnimator = antina.GetComponent<DOTweenAnimation>();
                if (rotationAnimator != null)
                {
                    rotationAnimator.DOPlay(); 
                }
            }
            _effectsActivated = true;
            isDragging = true;
            Vector3 screenInputPos = Input.mousePosition;
            screenInputPos.z = zCoord; 
            
            Vector3 newWorldPos = Camera.main.ScreenToWorldPoint(screenInputPos);
            transform.position = newWorldPos;
            transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            
            GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Scanner_Activate,true);
            
            if (lookAtTarget != null)
            {
                transform.LookAt(lookAtTarget.position);
            }
            
            if (!isAnimationPlayed && scannerAnimator != null)
            {
                scannerAnimator.enabled = true;
                isAnimationPlayed = true;
                // Set speed to 1 to play forward
                scannerAnimator.speed = 1f; 
                // Play the animation from its beginning (normalizedTime = 0f) using its index (0)
                scannerAnimator.Play(0, 0, 0f);
            }
            scannerEffects.SetActive(true);
            RevertBlackMaterialToOriginalColor();
            ScanArea(); 
        }
        else 
        {
            isDragging = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (antina != null)
            {
                DOTweenAnimation rotationAnimator = antina.GetComponent<DOTweenAnimation>();
                if (rotationAnimator != null)
                {
                    rotationAnimator.DOPlay();
                }
            }

            if (!isAnimationPlayed && scannerAnimator != null)
            {
                scannerAnimator.enabled = true;
                isAnimationPlayed = true;
                // Set speed to 1 to play forward
                scannerAnimator.speed = 1f; 
                // Play the animation from its beginning (normalizedTime = 0f) using its index (0)
                scannerAnimator.Play(0, 0, 0f);
            }
            
            RevertBlackMaterialToOriginalColor();
            antinaCheck = true;
            scannerEffects.SetActive(true);
            ScanArea(); 
        }
    }
    
    public void SetBlackMaterialToPureBlack()
    {
        if (blackMaterial != null)
        {
            blackMaterial.color = Color.black;
        }
        else
        {
            Debug.LogWarning("ScannerLogic: blackMaterial is not assigned in the Inspector. Cannot change its color to pure black.");
        }
    }
    
    public void RevertBlackMaterialToOriginalColor()
    {
        if (blackMaterial != null)
        {
            blackMaterial.color = _originalBlackMaterialColor;
        }
        else
        {
            Debug.LogWarning("ScannerLogic: blackMaterial is not assigned in the Inspector. Cannot revert its color.");
        }
    }
}