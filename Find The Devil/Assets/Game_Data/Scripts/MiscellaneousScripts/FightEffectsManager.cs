using UnityEngine;
using System.Collections; // Required for Coroutines

public class FightEffectsManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    [Tooltip("An array of GameObject prefabs to use as fight effects.")]
    public GameObject[] fightEffectPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("The central position where effects will be spawned.")]
    public Transform spawnPoint;
    [Tooltip("The time delay between spawning each effect.")]
    public float spawnInterval = 0.1f;
    [Tooltip("The duration for which each individual effect will be active before being destroyed.")]
    public float effectLifetime = 1.0f;
    [Tooltip("The total duration for which fight effects will be generated. Set to 0 for continuous.")]
    public float totalGenerationDuration = 3.0f; 

    [Header("Position Randomization")]
    [Tooltip("The radius around the spawn point within which effects will randomly appear.")]
    public float positionSpreadRadius = 0.5f; // New: Random position spread

    [Header("Rotation Settings")]
    [Tooltip("The minimum and maximum random Z-axis rotation (in degrees) for effects.")]
    [Range(0, 360)] public float minZRotation = 0f;
    [Range(0, 360)] public float maxZRotation = 360f;
    [Tooltip("If true, X and Y rotations will also be randomized.")]
    public bool randomizeXYRotation = false;
    [Tooltip("The minimum and maximum random X-axis rotation (in degrees) for effects (if Randomize XY Rotation is true).")]
    [Range(0, 360)] public float minXYRotation = 0f;
    [Range(0, 360)] public float maxXYRotation = 360f;

    [Header("Scale Settings")]
    [Tooltip("If true, effects will be scaled randomly between min and max scale.")]
    public bool randomizeScale = false; // New: Random scale toggle
    [Tooltip("The minimum scale for effects (if Randomize Scale is true).")]
    public float minScale = 0.8f; // New: Minimum scale
    [Tooltip("The maximum scale for effects (if Randomize Scale is true).")]
    public float maxScale = 1.2f; // New: Maximum scale
    [Tooltip("The fixed scale for effects (if Randomize Scale is false).")]
    public float fixedScale = 1.0f; // New: Fixed scale

    private Coroutine _effectGenerationCoroutine;
    private bool _isGeneratingEffects = false;

    public void StartFightEffects()
    {
        if (fightEffectPrefabs == null || fightEffectPrefabs.Length == 0)
        {
            Debug.LogWarning("FightEffectsManager: No fight effect prefabs assigned! Please assign them in the Inspector.");
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("FightEffectsManager: No spawn point assigned! Effects will spawn at manager's position.");
            spawnPoint = this.transform; // Default to manager's position
        }
        
        //new check
            GameManager.Instance.uiManager.HidePanel(UIPanelType.GameOverlayPanel);
            GameManager.Instance.playerController.ResetTools();
        //---------------------------------------------------
        
        GameManager.Instance.audioManager.PlayLoopingSFX(AudioManager.GameSound.Fighting,true);
        StopFightEffects(); // Stop any existing generation before starting new one
        _effectGenerationCoroutine = StartCoroutine(GenerateEffectsRoutine());
    }

    public void StopFightEffects()
    {
       
        if (_effectGenerationCoroutine != null)
        {
          
            StopCoroutine(_effectGenerationCoroutine);
            
            _effectGenerationCoroutine = null;
        }
        _isGeneratingEffects = false;
    }

    private IEnumerator GenerateEffectsRoutine()
    {
        _isGeneratingEffects = true;
        float startTime = Time.time;

        while (_isGeneratingEffects)
        {
            if (totalGenerationDuration > 0 && Time.time - startTime >= totalGenerationDuration)
            {
                _isGeneratingEffects = false;
                break;
            }

            SpawnRandomEffect();
            yield return new WaitForSeconds(spawnInterval);
        }
        
        //new check
        
        GameManager.Instance.uiManager.ShowPanel(UIPanelType.GameOverlayPanel);
        GameManager.Instance.playerController.SetupTools();
        //---------------------------------------------------

        
        GameManager.Instance.audioManager.StopLoopingSFX();
        Debug.Log("Fight effects generation finished.");
    }

    private void SpawnRandomEffect()
    {
        if (fightEffectPrefabs == null || fightEffectPrefabs.Length == 0) return;

        // 1. Pick a random effect prefab
        GameObject chosenPrefab = fightEffectPrefabs[Random.Range(0, fightEffectPrefabs.Length)];

        // 2. Generate a random position within the spread radius
        Vector3 randomOffset = Random.insideUnitSphere * positionSpreadRadius;
        Vector3 spawnPosition = spawnPoint.position + randomOffset;

        // 3. Generate a random rotation
        Quaternion randomRotation;
        if (randomizeXYRotation)
        {
            randomRotation = Quaternion.Euler(
                Random.Range(minXYRotation, maxXYRotation),
                Random.Range(minXYRotation, maxXYRotation),
                Random.Range(minZRotation, maxZRotation)
            );
        }
        else
        {
            randomRotation = Quaternion.Euler(
                0, 
                0,
                Random.Range(minZRotation, maxZRotation)
            );
        }

        // 4. Instantiate the effect
        GameObject effectInstance = Instantiate(chosenPrefab, spawnPosition, randomRotation);

        // 5. Apply scale
        if (randomizeScale)
        {
            float randomScaleValue = Random.Range(minScale, maxScale);
            effectInstance.transform.localScale = new Vector3(randomScaleValue, randomScaleValue, randomScaleValue);
        }
        else
        {
            effectInstance.transform.localScale = new Vector3(fixedScale, fixedScale, fixedScale);
        }

        // 6. Destroy the effect after its lifetime
        Destroy(effectInstance, effectLifetime);
    }

    private void OnDisable()
    {
        StopFightEffects();
    }

    private void OnDestroy()
    {
        StopFightEffects();
    }
}