using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using System;


[System.Serializable]
public class DamagedBuildings 
{
    [SerializeField] public GameObject originalFormInstance;
    [SerializeField] public GameObject damagedFormInstance;
    [SerializeField] public GameObject hidingLayerInstance;
    [SerializeField] public int coinsToRepair;
    [SerializeField] public bool isbuildingRepaired; 
} 

public class MetaController : MonoBehaviour
{
    [Header("Building Form Instances (Assign this in Inspector)")] 
    public DamagedBuildings[] damagedBuildings;

    public GameObject damagedProps;

    public GameObject repairedProps;
    public Transform _coinTargetTransform;
    public GameObject conffetiEffects;
    public GameObject buildingAppearEffects;
    
    // --- NEW: Flag to prevent animations when in showcase mode ---
    public bool IsShowcaseMeta = false;
    
    private int _totalCoinsNeededForFullRepair;
    private int _currentCoinsSpentGlobal;
    
    private int _currentBuildingPartIndex = 0;
    private List<int> _currentCoinsSpentPerPart;

    private List<float> _initialHidingLayerScalesZ = new List<float>();
    private bool _hasPlayedFullRepairConfetti = false;

    [Header("Repair Animation Settings")]
    [SerializeField] private float buildingRepairDelay = 0.2f;
    [SerializeField] private float hidingLayerScaleDuration = 0.3f;
    [SerializeField] private Ease hidingLayerScaleEase = Ease.OutSine;
    
    private bool _isProcessingRepair = false;

    public void ConfigureFromMetaData(MetaData data)
    {
        _totalCoinsNeededForFullRepair = 0;
        _initialHidingLayerScalesZ.Clear();
        _currentCoinsSpentPerPart = new List<int>(new int[damagedBuildings.Length]);
        Debug.Log("ConfigureFromMetaData(MetaData data");
        
        for (int i = 0; i < damagedBuildings.Length; i++)
        {
            DamagedBuildings part = damagedBuildings[i];
            _totalCoinsNeededForFullRepair += part.coinsToRepair;

            if (part.hidingLayerInstance != null)
            {
                _initialHidingLayerScalesZ.Add(part.hidingLayerInstance.transform.localScale.z);
            }
            else
            {
                _initialHidingLayerScalesZ.Add(0f); 
            }
            part.isbuildingRepaired = false;
        }

        _currentCoinsSpentGlobal = 0;
        _currentBuildingPartIndex = 0;
        _hasPlayedFullRepairConfetti = false;
        _isProcessingRepair = false;

        while (_currentBuildingPartIndex < damagedBuildings.Length && damagedBuildings[_currentBuildingPartIndex].coinsToRepair <= 0)
        {
            DamagedBuildings zeroCostPart = damagedBuildings[_currentBuildingPartIndex];
            zeroCostPart.isbuildingRepaired = true;
            _currentBuildingPartIndex++;
        }

        UpdateRepairVisuals(); 
    }

    public void SpendCoins(int amount)
    {
        if (amount <= 0 || _isProcessingRepair) return;

        StartCoroutine(ProcessCoinSpending(amount));
    }

    private IEnumerator ProcessCoinSpending(int initialAmount)
    {
        _isProcessingRepair = true;
        int remainingAmount = initialAmount;
        int previousBuildingPartIndex = _currentBuildingPartIndex;
        
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Meta_CoinsSpend);
        while (remainingAmount > 0 && _currentBuildingPartIndex < damagedBuildings.Length)
        {
            DamagedBuildings currentPart = damagedBuildings[_currentBuildingPartIndex];

            if (currentPart.isbuildingRepaired || currentPart.coinsToRepair <= 0)
            {
                _currentBuildingPartIndex++;
                UpdateRepairVisuals();
                continue;
            }

            int remainingCoinsForThisPart = currentPart.coinsToRepair - _currentCoinsSpentPerPart[_currentBuildingPartIndex];
            int coinsToApply = Mathf.Min(remainingAmount, remainingCoinsForThisPart);

            if (coinsToApply <= 0)
            {
                _currentBuildingPartIndex++;
                UpdateRepairVisuals();
                continue;
            }

            _currentCoinsSpentPerPart[_currentBuildingPartIndex] += coinsToApply;
            _currentCoinsSpentGlobal += coinsToApply;
            remainingAmount -= coinsToApply;

            if (currentPart.damagedFormInstance != null) 
            {
                DOTweenAnimation dotweenAnim = currentPart.damagedFormInstance.GetComponent<DOTweenAnimation>();
                if (dotweenAnim != null) dotweenAnim.DORestart(); 
            }
            if (currentPart.originalFormInstance != null)
            {
                DOTweenAnimation dotweenAnim2 = currentPart.originalFormInstance.GetComponent<DOTweenAnimation>();
                if (dotweenAnim2 != null) dotweenAnim2.DORestart();
            }
            
            UpdateHidingLayerScale(currentPart, _currentCoinsSpentPerPart[_currentBuildingPartIndex]);
            
            if (_currentCoinsSpentPerPart[_currentBuildingPartIndex] >= currentPart.coinsToRepair)
            {
                GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Meta_Completed);
                currentPart.isbuildingRepaired = true;

                if (currentPart.originalFormInstance != null)
                {
                    // Play confetti once when a single building is repaired
                    PlayConfettiForBuilding(currentPart.originalFormInstance.transform.position, 1f);
                   //wait for next building
                    yield return new WaitForSeconds(0.7f);
                }
                _currentBuildingPartIndex++;
                UpdateRepairVisuals();
                
                if (_currentBuildingPartIndex < damagedBuildings.Length)
                {
                   
                    if (_currentBuildingPartIndex > previousBuildingPartIndex && _currentBuildingPartIndex < damagedBuildings.Length)
                    {
                        DamagedBuildings newPart = damagedBuildings[_currentBuildingPartIndex];
                        if (buildingAppearEffects != null && newPart.originalFormInstance != null)
                        {
                            GameObject effectInstance = Instantiate(buildingAppearEffects, newPart.originalFormInstance.transform.position, Quaternion.identity);
                   
                            Destroy(effectInstance, 2.0f); 
                        }
                        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Meta_Building_Appear);
                    }
 
                    yield return new WaitForSeconds(buildingRepairDelay);
                }
            }
            else
            {
                UpdateRepairVisuals();
            }
        }
        _isProcessingRepair = false;
    }

    private void UpdateHidingLayerScale(DamagedBuildings part, int currentCoinsSpent)
    {
        if (part.hidingLayerInstance != null)
        {
            float partRepairProgressPercentage = (part.coinsToRepair > 0) ? 
                                                 (float)currentCoinsSpent / part.coinsToRepair : 1f;

            float targetScaleZ = _initialHidingLayerScalesZ[System.Array.IndexOf(damagedBuildings, part)] * (1f - partRepairProgressPercentage);
            targetScaleZ = Mathf.Max(0f, targetScaleZ); 

            part.hidingLayerInstance.transform.DOKill();
            part.hidingLayerInstance.transform.DOScaleZ(targetScaleZ, hidingLayerScaleDuration)
                .SetEase(hidingLayerScaleEase);
        }
    }


    private void UpdateRepairVisuals()
    {
        int defaultLayer = LayerMask.NameToLayer("Default");

        for (int i = 0; i < damagedBuildings.Length; i++)
        {
           
            DamagedBuildings part = damagedBuildings[i]; 
            
            bool isThisPartFullyRepaired = part.isbuildingRepaired; 

            if (isThisPartFullyRepaired)
            {
                if (part.originalFormInstance != null) 
                {
                    part.originalFormInstance.SetActive(true);
                    // Use the new helper function to set the layer for the parent and all children
                    SetChildrenLayer(part.originalFormInstance, defaultLayer);
                }
                if (part.damagedFormInstance != null) part.damagedFormInstance.SetActive(false);
                if (part.hidingLayerInstance != null) part.hidingLayerInstance.SetActive(false);
            }
            else 
            {
                bool isActiveRepairTarget = (i == _currentBuildingPartIndex);
                
                if (part.damagedFormInstance != null) part.damagedFormInstance.SetActive(isActiveRepairTarget);
                if (part.hidingLayerInstance != null) 
                {
                    part.hidingLayerInstance.SetActive(isActiveRepairTarget);
                   
                    if (isActiveRepairTarget)
                    {
                        if (part.damagedFormInstance != null)
                        {
                            _coinTargetTransform = part.damagedFormInstance.transform;
                        }

                        if (part.originalFormInstance != null) part.originalFormInstance.SetActive(true); 
                    }
                }
            }
        }

        if (IsFullyRepaired())
        {
           
            damagedProps.SetActive(false);
            repairedProps.SetActive(true);

            // --- NEW: Add a check to prevent the animation from playing in showcase mode ---
            if (!_hasPlayedFullRepairConfetti && !IsShowcaseMeta)
            {
                _hasPlayedFullRepairConfetti = true;
                
                StartCoroutine(PlayFullRepairConfetti());
            }
        }
        else
        {
            _hasPlayedFullRepairConfetti = false;
            damagedProps.SetActive(true);
            repairedProps.SetActive(false);
        }
    }

    // Helper function to set the layer for a GameObject and all its children
    private void SetChildrenLayer(GameObject parent, int newLayer)
    {
        if (parent == null) return;

        // Set the layer for the parent object itself
        parent.layer = newLayer;

        // Recursively set the layer for all children
        foreach (Transform child in parent.transform)
        {
            SetChildrenLayer(child.gameObject, newLayer);
        }
    }

    public void LoadRepairProgress(int spentCoinsGlobal)
    {
        _currentCoinsSpentGlobal = 0;
        _currentCoinsSpentPerPart = new List<int>(new int[damagedBuildings.Length]);
        _currentBuildingPartIndex = 0;
        _hasPlayedFullRepairConfetti = false; 
        _isProcessingRepair = false;

        int remainingCoinsToDistribute = spentCoinsGlobal;

        for (int i = 0; i < damagedBuildings.Length; i++)
        {
            DamagedBuildings part = damagedBuildings[i];
            int coinsNeededForThisPart = part.coinsToRepair;

            if (remainingCoinsToDistribute >= coinsNeededForThisPart)
            {
                _currentCoinsSpentPerPart[i] = coinsNeededForThisPart;
                remainingCoinsToDistribute -= coinsNeededForThisPart;
                part.isbuildingRepaired = true;
                _currentBuildingPartIndex = i + 1;
            }
            else
            {
                _currentCoinsSpentPerPart[i] = remainingCoinsToDistribute;
                remainingCoinsToDistribute = 0;
                part.isbuildingRepaired = false;
                _currentBuildingPartIndex = i;
            
                // --- FIX: Update the hiding layer z-scale for the current repairing part ---
                UpdateHidingLayerScale(part, _currentCoinsSpentPerPart[i]);
                // --- END OF FIX ---
            
                break;
            }
        
            // --- FIX: Also ensure previously repaired buildings have their layers removed on load ---
            UpdateHidingLayerScale(part, _currentCoinsSpentPerPart[i]);
            // --- END OF FIX ---
        }

        _currentCoinsSpentGlobal = spentCoinsGlobal;
    
        UpdateRepairVisuals();
    }

    public int GetCurrentSpentCoins()
    {
        return _currentCoinsSpentGlobal;
    }

    public int GetTotalCoinsToRepair()
    {
        return _totalCoinsNeededForFullRepair;
    }

    public bool IsFullyRepaired()
    {
        return _currentCoinsSpentGlobal >= _totalCoinsNeededForFullRepair;
    }

    public bool IsProcessingRepair()
    {
        return _isProcessingRepair;
    }

    /// <summary>
    /// Plays a single confetti burst at a specified position for a given duration.
    /// </summary>
    /// <param name="position">The world position to spawn the confetti.</param>
    /// <param name="duration">The total duration for the confetti to be visible.</param>
    private void PlayConfettiForBuilding(Vector3 position, float duration)
    {
        if (conffetiEffects == null) return;
        GameObject confettiInstance = Instantiate(conffetiEffects, position, Quaternion.identity);
        
        ParticleSystem ps = confettiInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Confettipop);
            ps.Play();
        }

        Destroy(confettiInstance, duration);
    }

    private IEnumerator PlayFullRepairConfetti()
    {
        float totalDuration = 2f;
        float delayBetweenBursts = totalDuration / (damagedBuildings.Length * 2);
        
        for (int i = 0; i < damagedBuildings.Length; i++)
        {
            DamagedBuildings part = damagedBuildings[i];
            if (part.originalFormInstance != null)
            {
                GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Confettipop);
                // Play the first burst for this building
                PlayConfettiForBuilding(part.originalFormInstance.transform.position, totalDuration);
                yield return new WaitForSeconds(delayBetweenBursts);
                
                // Play the second burst for this building
                PlayConfettiForBuilding(part.originalFormInstance.transform.position, totalDuration);
            }
            yield return new WaitForSeconds(delayBetweenBursts);
        }
        
        // Final call after the animation is complete
        yield return new WaitForSeconds(0.5f);
        
        GameManager.Instance.metaManager.FillMeta();
    }
    
    public int GetRemainingCoinsToRepair()
    {
        return _totalCoinsNeededForFullRepair - _currentCoinsSpentGlobal;
    }
}