using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // Required for DOTween

public class SplashManager : MonoBehaviour
{
    [SerializeField] private string mainGameplaySceneName = "MainGameplay";
    [SerializeField] private float initialSplashDuration = 0.5f;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text loadingPercentageText;
    [SerializeField] private GameObject loadingUIPanel;
    [SerializeField] private float minLoadingFillDuration = 0.5f; // Minimum time for filler animation
    [SerializeField] private float maxLoadingFillDuration = 1.5f; // Maximum time for filler animation

    private Tween _fillTween;

    void Awake()
    {
        if (loadingUIPanel != null)
        {
            loadingUIPanel.SetActive(false);
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 0;
        }
        if (loadingPercentageText != null)
        {
            loadingPercentageText.text = "0%";
        }
    }

    void Start()
    {
        StartCoroutine(StartLoadingProcess());
    }

    private IEnumerator StartLoadingProcess()
    {
        // Initial splash screen duration
        yield return new WaitForSeconds(initialSplashDuration);

        // Activate loading UI panel
        if (loadingUIPanel != null)
        {
            loadingUIPanel.SetActive(true);
        }
        else
        {
            // Fallback if panel is not assigned, ensure slider/text are active
            if (loadingSlider != null) loadingSlider.gameObject.SetActive(true);
            if (loadingPercentageText != null) loadingPercentageText.gameObject.SetActive(true);
        }

        // Start the filler animation first
        StartCoroutine(PerformFillerAnimationAndLoadScene());
    }

    private IEnumerator PerformFillerAnimationAndLoadScene()
    {
        float currentDisplayProgress = 0f;
        float randomFillDuration = Random.Range(minLoadingFillDuration, maxLoadingFillDuration);

        // Perform the filler animation to 100%
        _fillTween = DOTween.To(() => currentDisplayProgress, x => currentDisplayProgress = x, 1f, randomFillDuration)
            .SetEase(Ease.OutSine) // Smooth easing for the fill
            .OnUpdate(() =>
            {
                if (loadingSlider != null)
                {
                    loadingSlider.value = currentDisplayProgress;
                }
                if (loadingPercentageText != null)
                {
                    loadingPercentageText.text = $"{Mathf.RoundToInt(currentDisplayProgress * 100)}%";
                }
            })
            .OnComplete(() =>
            {
                // Ensure UI is exactly 100% when complete
                if (loadingSlider != null) loadingSlider.value = 1f;
                if (loadingPercentageText != null) loadingPercentageText.text = "100%";
                _fillTween = null; // Clear the tween reference
            });

        // Wait for the filler animation to complete
        yield return _fillTween.WaitForCompletion();

        // Filler animation is done, now load the gameplay scene directly
        SceneManager.LoadScene(mainGameplaySceneName);

        // IMPORTANT: The loadingUIPanel will remain active as per your request.
        // If you intended for it to be deactivated after the scene loads and the new scene is ready,
        // that logic would typically go in the next scene's manager (e.g., GameManager Init).
    }

    // No longer needed as we're not using asyncLoad.allowSceneActivation = false;
    // private IEnumerator LoadGameplaySceneAsync() { ... }

    void OnDisable()
    {
        _fillTween?.Kill();
    }

    void OnDestroy()
    {
        _fillTween?.Kill();
    }
}