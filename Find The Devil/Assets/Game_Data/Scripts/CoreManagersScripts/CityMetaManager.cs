using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;

[System.Serializable]
public class CityMeta
{
    public int cityID;
    public GameObject activeCityPart;
    public GameObject disabledCityPart;
    public GameObject hidingLayer;
    public List<GameObject> devilDrones;
    public bool isLocked;
}

public class CityMetaManager : MonoBehaviour
{
    [Header("City Meta Settings")]
    [SerializeField] private List<CityMeta> _cityReferences;
    [SerializeField] private Transform cityMetaSpawnParent;
    [SerializeField] private GameObject completionEffects;
    [SerializeField] private GameObject droneDestroyEffects;
    [SerializeField] private CityMetaAnimationHandler _CityMetaAnimation;

    private GameObject _instantiatedCompletionEffects;

    // A private field to track the ID of the next city to be unlocked
    private int _nextCityToUnlockID = 1;

    // Public property to get the currently unlocked city's ID
    public int CurrentCityMetaIndex { get; private set; } = 0;

    public void LoadCityMetaStates()
    {
        List<CityMeta> savedStates = GameManager.Instance.progressionManager.LoadCityMetaStates(_cityReferences.Count);
        int savedNextCityID = GameManager.Instance.progressionManager.LoadNextCityToUnlockID();
        
        if (savedStates != null && savedStates.Count == _cityReferences.Count)
        {
            for (int i = 0; i < _cityReferences.Count; i++)
            {
                _cityReferences[i].isLocked = savedStates[i].isLocked;
            }
        }

        _nextCityToUnlockID = savedNextCityID;
        
        // Find the highest unlocked city to set the current index
        int highestUnlockedID = _cityReferences.Where(c => !c.isLocked).Select(c => c.cityID).DefaultIfEmpty(-1).Max();
        CurrentCityMetaIndex = highestUnlockedID;
    }

    public void ActivateCityMetaView()
    {
        if (cityMetaSpawnParent != null)
        {
            cityMetaSpawnParent.gameObject.SetActive(true);

            foreach (var city in _cityReferences)
            {
                if (city.isLocked)
                {
                    if (city.activeCityPart != null) city.activeCityPart.SetActive(true);
                    if (city.disabledCityPart != null) city.disabledCityPart.SetActive(true);
                    if (city.hidingLayer != null) city.hidingLayer.SetActive(true);
                    
                    foreach(GameObject drone in city.devilDrones)
                    {
                        if (drone != null) drone.SetActive(true);
                    }
                }
                else
                {
                    if (city.activeCityPart != null)
                    {
                        city.activeCityPart.SetActive(true);
                        city.activeCityPart.layer = LayerMask.NameToLayer("Default");
                    }
                    if (city.disabledCityPart != null) city.disabledCityPart.SetActive(false);
                    if (city.hidingLayer != null) city.hidingLayer.SetActive(false);
                    
                    foreach(GameObject drone in city.devilDrones)
                    {
                        if (drone != null) drone.SetActive(false);
                    }
                }
            }
        }
        
        UnlockNextCity(0.25f);
        
    }
    
    // Public method to trigger the next city unlock animation
    public void UnlockNextCity(float cityMetaLevelLoadDelay)
    {
        CityMeta cityToUnlock = _cityReferences.FirstOrDefault(c => c.cityID == _nextCityToUnlockID);
        
        if (cityToUnlock != null && cityToUnlock.isLocked)
        {
            AnimateUnlockedCityMeta(cityToUnlock.cityID, cityMetaLevelLoadDelay);
        }
        else
        {
            // If all cities are unlocked or an error occurred, do nothing
            Debug.Log("No more cities to unlock or city is already unlocked.");
        }
    }

    private void AnimateUnlockedCityMeta(int cityID, float cityMetaLevelLoadDelay)
    {
        CityMeta cityToUnlock = _cityReferences.FirstOrDefault(c => c.cityID == cityID);

        if (cityToUnlock == null) return;
        
        if (cityToUnlock.hidingLayer != null)
        {
            cityToUnlock.hidingLayer.SetActive(true);
            _CityMetaAnimation.MovePawnDevil();

            Vector3 originalHidingLayerScale = cityToUnlock.hidingLayer.transform.localScale;

            cityToUnlock.hidingLayer.transform.DOScaleZ(0f, cityMetaLevelLoadDelay)
                .SetEase(Ease.OutSine) 
                .OnComplete(() => {
                    
                    cityToUnlock.hidingLayer.SetActive(false);
                    cityToUnlock.hidingLayer.transform.localScale = originalHidingLayerScale;
                    
                    if (cityToUnlock.disabledCityPart != null) cityToUnlock.disabledCityPart.SetActive(false);
                    if (cityToUnlock.activeCityPart != null) cityToUnlock.activeCityPart.SetActive(true);
                    
                    if (cityToUnlock.activeCityPart != null)
                    {
                        cityToUnlock.activeCityPart.layer = LayerMask.NameToLayer("Default");
                        if (completionEffects != null)
                        {
                            _instantiatedCompletionEffects = Instantiate(completionEffects, cityToUnlock.activeCityPart.transform.position, Quaternion.identity);
                            _instantiatedCompletionEffects.transform.SetParent(cityToUnlock.activeCityPart.transform);
                        }
                    }

                    GameManager.Instance.StartCoroutine(DestroyDevilDronesSequentially(cityToUnlock.devilDrones));
                    
                    // --- Progression Update ---
                    cityToUnlock.isLocked = false;
                    _nextCityToUnlockID++;
                    CurrentCityMetaIndex = cityToUnlock.cityID;
                    GameManager.Instance.progressionManager.SaveCityMetaStates(_cityReferences, _nextCityToUnlockID);
                });
        }
        else
        {
            // --- Progression Update ---
            cityToUnlock.isLocked = false;
            _nextCityToUnlockID++;
            CurrentCityMetaIndex = cityToUnlock.cityID;

            if (cityToUnlock.disabledCityPart != null) cityToUnlock.disabledCityPart.SetActive(false);
            if (cityToUnlock.activeCityPart != null)
            {
                cityToUnlock.activeCityPart.SetActive(true);
                cityToUnlock.activeCityPart.layer = LayerMask.NameToLayer("Default");
            }
            GameManager.Instance.StartCoroutine(DestroyDevilDronesSequentially(cityToUnlock.devilDrones));
            GameManager.Instance.progressionManager.SaveCityMetaStates(_cityReferences, _nextCityToUnlockID);
        }
    }

    private IEnumerator DestroyDevilDronesSequentially(List<GameObject> drones)
    {
        if (drones == null || drones.Count == 0) yield break;

        foreach (GameObject drone in drones)
        {
            if (drone != null)
            {
                Vector3 originalScale = drone.transform.localScale;
                float scaleIncrease = 1.2f;
                float animDuration = 0.2f;

                drone.transform.DOScale(originalScale * scaleIncrease, animDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() =>
                    {
                        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.MetaEnemyKill);
                        if (droneDestroyEffects != null)
                        {
                            Instantiate(droneDestroyEffects, drone.transform.position, Quaternion.identity);
                        }
                        Destroy(drone);
                    });
                
                yield return new WaitForSeconds(animDuration + 0.1f);
            }
        }
    }
    
    public void DeactivateCityMetaView()
    {
        if (_instantiatedCompletionEffects != null)
        {
            Destroy(_instantiatedCompletionEffects);
            _instantiatedCompletionEffects = null;
        }
        
        if (cityMetaSpawnParent != null)
        {
            cityMetaSpawnParent.gameObject.SetActive(false);
            foreach (var city in _cityReferences)
            {
                if (city.activeCityPart != null) city.activeCityPart.SetActive(false);
                if (city.disabledCityPart != null) city.disabledCityPart.SetActive(true);
                if (city.hidingLayer != null) city.hidingLayer.SetActive(false);
                foreach(GameObject drone in city.devilDrones)
                {
                    if (drone != null) drone.SetActive(false);
                }
            }
        }
    }
}