using UnityEngine;
using System;

public class TutorialPanel : UIPanel
{
    // Tutorial UI GameObjects - Assign these in the Inspector
    [SerializeField] private GameObject swipeToScan;
    [SerializeField] private GameObject switchLaserArrow;
    [SerializeField] private GameObject tapToZap;

    private TutorialState _currentTutorialState;
    private bool _hasScanned = false; // Flag to ensure scanner step is completed

    // Enum to manage the tutorial's progression
    private enum TutorialState
    {
        Inactive,               // Tutorial not active, no UI shown
        ScannerTutorialActive,  // Showing "Swipe to Scan"
        SwitchWeaponTutorialActive, // Showing "Switch to Laser" arrow
        LaserTutorialActive,    // Showing "Tap to Zap"
        Completed               // Tutorial finished, all UI hidden
    }

    public override void Hide()
    {
        swipeToScan.SetActive(false);
        switchLaserArrow.SetActive(false);
        tapToZap.SetActive(false);
        _currentTutorialState = TutorialState.ScannerTutorialActive;
        _hasScanned = false; 
        base.Hide();
    }

    private void OnEnable()
    {
        _currentTutorialState = TutorialState.ScannerTutorialActive;
        _hasScanned = false; 
        // Subscribe to events
        ScannerLogic.OnScannerUsed += OnScannerUsedHandler;
        PlayerController.OnWeaponSwitched += OnWeaponSwitchedHandler;
        LaserGunController.OnLaserFired += OnLaserFiredHandler;
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks
        ScannerLogic.OnScannerUsed -= OnScannerUsedHandler;
        PlayerController.OnWeaponSwitched -= OnWeaponSwitchedHandler;
        LaserGunController.OnLaserFired -= OnLaserFiredHandler;
    }

    public override void UpdatePanel()
    {
        swipeToScan.SetActive(false);
        switchLaserArrow.SetActive(false);
        tapToZap.SetActive(false);
        
        _currentTutorialState = TutorialState.ScannerTutorialActive;
        _hasScanned = false;
        
        //InitTutorial();
    }

    private void Start()
    {
        InitTutorial();
    }

    public void InitTutorial()
    {
        
        _currentTutorialState = TutorialState.ScannerTutorialActive;
        _hasScanned = false; 
        UpdateTutorialUI();
    }

 
    private void OnScannerUsedHandler()
    {
        if (_currentTutorialState == TutorialState.ScannerTutorialActive && !_hasScanned)
        {
            _hasScanned = true; // Mark scanner step as complete
            _currentTutorialState = TutorialState.SwitchWeaponTutorialActive; // Move to next state
            UpdateTutorialUI();
        }
    }

    private void OnWeaponSwitchedHandler()
    {
        
        //bool isLaserGunActiveNow = GameManager.Instance.playerController.CurrentLaserGun.IsActive;
        bool isLaserGunActiveNow = ((LaserGunController)GameManager.Instance.playerController.CurrentLaserGun).IsActive;
            switch (_currentTutorialState)
            {
                
                case TutorialState.ScannerTutorialActive:
                    if (_hasScanned)
                    {
                        
                    }
                    else
                    {
                        _currentTutorialState = TutorialState.SwitchWeaponTutorialActive;
                    }

                    break;
                 
                
                case TutorialState.SwitchWeaponTutorialActive:
                    // Player successfully switched to Laser Gun
                    if (!_hasScanned)
                    {
                        _currentTutorialState = TutorialState.ScannerTutorialActive;
                    }
                    else
                    {
                        if (isLaserGunActiveNow)
                        {
                            _currentTutorialState = TutorialState.LaserTutorialActive;
                            
                        }
                        else
                        {
                            _currentTutorialState = TutorialState.SwitchWeaponTutorialActive;
                            
                        }    
                    }
                    
                    
                    break;

                case TutorialState.LaserTutorialActive:
                    // Player is supposed to be Tapping to Zap, but switched weapon.
                    // If they switched to Scanner (meaning LaserGun is NOT active)
                    if (isLaserGunActiveNow)
                    {
                        _currentTutorialState = TutorialState.SwitchWeaponTutorialActive; // Guide them back to laser
                    }
                    // If they switched to Laser Gun (they were already on it), no state change needed.
                    break;
            }
            UpdateTutorialUI(); // Update UI after state changes
    }
 
    private void OnLaserFiredHandler()
    {
        if (_currentTutorialState == TutorialState.LaserTutorialActive)
        {
            _currentTutorialState = TutorialState.Completed;
            UpdateTutorialUI();
        }
    }

    // --- UI Update Logic ---

    private void UpdateTutorialUI()
    {
        swipeToScan?.SetActive(false);
        switchLaserArrow?.SetActive(false);
        tapToZap?.SetActive(false);

        switch (_currentTutorialState)
        {
            case TutorialState.ScannerTutorialActive:
                swipeToScan?.SetActive(true);
                break;
            case TutorialState.SwitchWeaponTutorialActive:
                switchLaserArrow?.SetActive(true);
                break; 
            case TutorialState.LaserTutorialActive:
                tapToZap?.SetActive(true);
                break;
            case TutorialState.Completed:
                // Tutorial is done, optionally hide the panel itself
                // gameObject.SetActive(false);
                break;
            case TutorialState.Inactive:
                // All hidden by default
                break;
        }
    }

    // Public method to reset the tutorial if needed (e.g., player dies, restarts level)
    public void ResetTutorial()
    {
        _currentTutorialState = TutorialState.Inactive;
        _hasScanned = false; // Reset flag as well
        UpdateTutorialUI();
    }
}