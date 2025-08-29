using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class DialogueData
{
    public int levelNumber;
    [TextArea(3, 5)]
    public string[] dialogues;
}

public class LevelObjectivesPanel : UIPanel
{
    [SerializeField] private GameObject enemyIconContainer;
    [SerializeField] private GameObject enemyIconPrefab;
    [SerializeField] private Text coinsText;
    [SerializeField] private GameObject coinsObj;
    
    private List<GameObject> _enemyIcons = new List<GameObject>();

    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text dialogueText;
    [SerializeField] private float dialogueDisplayDuration = 3.5f;
    [SerializeField] private float wordDisplayDelay = 0.1f;
    
    [SerializeField] private List<DialogueData> kingDevilDialogueSets;

    private Coroutine _cutsceneCoroutine;

    [SerializeField] private GameObject rescuedNPCCountPanel;
    [SerializeField] private Image rescuedCountImage;
    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private float countUpdateDelay = 0.5f;
    [SerializeField] private Button skipBtn;


    [SerializeField] private GameObject getVIPGun;
    [SerializeField] private Image btnFiller;
    [SerializeField] private float vipGunFillAnimationDuration = 4.0f;
    [SerializeField] private float vipGunScaleAnimationDuration = 0.3f;
    
    private Tween _vipGunFillerTween;
    private Tween _vipGunScaleTween;

    private bool tryingThisWeapon;
    private bool isVIPGunAnime;

    
    private int _totalCoinTarget = 0;
    private void Start()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (rescuedNPCCountPanel != null)
        {
            rescuedNPCCountPanel.SetActive(false); 
        }

        if (getVIPGun != null)
        {
            getVIPGun.SetActive(false); 
        }
        if (btnFiller != null)
        {
            btnFiller.fillAmount = 1f;
        }

        if (enemyIconContainer != null) enemyIconContainer.SetActive(false);
        if (coinsText != null) coinsText.gameObject.SetActive(false);
        if (coinsObj != null) coinsObj.gameObject.SetActive(false);
        
    }

    private void OnEnable()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        if (rescuedNPCCountPanel != null)
        {
            rescuedNPCCountPanel.SetActive(false);
        }
        
        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
            _cutsceneCoroutine = null; 
        }

        _vipGunFillerTween?.Kill();
        _vipGunFillerTween = null;
        _vipGunScaleTween?.Kill();
        _vipGunScaleTween = null;
        
        //ActivateVIPGunFeature();
    }

    public void Reset()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        if (rescuedNPCCountPanel != null)
        {
            rescuedNPCCountPanel.SetActive(false);
        }
        
        if (enemyIconContainer != null)
        {
            foreach (Transform child in enemyIconContainer.transform)
            {
                Destroy(child.gameObject);
            }
            _enemyIcons.Clear();
            enemyIconContainer.SetActive(false);
        }

        if (coinsText != null) coinsText.gameObject.SetActive(false);
        if (coinsObj != null) coinsObj.gameObject.SetActive(false);
        
        if (getVIPGun != null)
        {
            getVIPGun.transform.localScale = Vector3.zero;
            getVIPGun.SetActive(false);
        }
        if (btnFiller != null)
        {
            btnFiller.fillAmount = 1f;
        }
        _vipGunFillerTween?.Kill();
        _vipGunFillerTween = null;
        _vipGunScaleTween?.Kill();
        _vipGunScaleTween = null;
    }


    private void OnDisable()
    {
        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
            _cutsceneCoroutine = null;
        }
        _vipGunFillerTween?.Kill(); 
        _vipGunFillerTween = null; 
        _vipGunScaleTween?.Kill(); 
        _vipGunScaleTween = null;
        
        if (getVIPGun != null)
        {
            getVIPGun.transform.localScale = Vector3.zero;
            getVIPGun.SetActive(false);
        }

        if (enemyIconContainer != null) enemyIconContainer.SetActive(false);
        if (coinsText != null) coinsText.gameObject.SetActive(false);
        if (coinsObj != null) coinsObj.gameObject.SetActive(false);
        
    }

    private void OnDestroy()
    {
        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
            _cutsceneCoroutine = null;
        }
        _vipGunFillerTween?.Kill(); 
        _vipGunFillerTween = null; 
        _vipGunScaleTween?.Kill(); 
        _vipGunScaleTween = null;
    }

    public void ActivateVIPGunFeature()
    {
        if (enemyIconContainer != null) enemyIconContainer.SetActive(false);
        if (coinsText != null) coinsText.gameObject.SetActive(false);
        if (coinsObj != null) coinsObj.gameObject.SetActive(false);
        Debug.Log("ActivateVIPGunFeature()");
        if (/*GameManager.Instance.levelManager._currentLevelNumber > 0*/ 
            GameManager.Instance.levelManager.GlobalLevelNumber > 0
            && GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue) 
        {
            if (getVIPGun != null)
            {
                tryingThisWeapon = false;
                getVIPGun.SetActive(true);
                getVIPGun.transform.localScale = Vector3.one; 
                isVIPGunAnime = true;
                if (btnFiller != null)
                {
                   
                    _vipGunFillerTween?.Kill(); 
                    btnFiller.fillAmount = 1f; 
                    
                    _vipGunFillerTween = btnFiller.DOFillAmount(0f, vipGunFillAnimationDuration)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => {
                            _vipGunScaleTween = getVIPGun.transform.DOScale(0f, vipGunScaleAnimationDuration)
                                .SetEase(Ease.InBack)
                                .OnComplete(() => {
                                    getVIPGun.SetActive(false);
                                    ActivateLevelObjectivesDisplay();
                                    _vipGunScaleTween = null;
                                });
                            _vipGunFillerTween = null;
                        });
                }
                else
                {
                    if (getVIPGun != null) getVIPGun.SetActive(false);
                    ActivateLevelObjectivesDisplay();
                }
            }
            else
            {
                ActivateLevelObjectivesDisplay();
            }
        }
        else
        {
            if (getVIPGun != null)
            {
                getVIPGun.transform.localScale = Vector3.zero;
                getVIPGun.SetActive(false);
            }
            _vipGunFillerTween?.Kill();
            _vipGunFillerTween = null;
            _vipGunScaleTween?.Kill();
            _vipGunScaleTween = null;
            ActivateLevelObjectivesDisplay();
        }
    }

    
    public void OnGetVIPGunButtonClicked(string vipGunID)
    {
        // AdsCaller.Instance.ShowRewardedAd((() =>
        // {
            
            if (!isVIPGunAnime || getVIPGun == null || !getVIPGun.activeSelf)
            {
                Debug.LogWarning("OnGetVIPGunButtonClicked called when VIP Gun feature is not active. Ignoring.");
                return;
            }

            _vipGunFillerTween?.Kill();
            _vipGunFillerTween = null;
            _vipGunScaleTween?.Kill(); 
            _vipGunScaleTween = null;

            Debug.Log($"Attempting to unlock VIP Gun: {vipGunID}");
            GameManager.Instance.playerController.EquipLaserGun(GameManager.Instance.shopManager.GetTryWeaponPrefab(vipGunID));
            tryingThisWeapon = true;
            
            
            GameManager.Instance._waitForTryGun = true;   
            
            ActivateLevelObjectivesDisplay();
            
            Debug.Log($"VIP Gun {vipGunID} unlocked successfully!");
            

            getVIPGun.transform.DOScale(0f, vipGunScaleAnimationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    getVIPGun.SetActive(false);
                    isVIPGunAnime = false; 
                    
                });
        
       // }));
        
    }

    private void ActivateLevelObjectivesDisplay()
    {
        if (GameManager.Instance.levelManager.CurrentLevel.GetLevelType() != LevelType.Rescue)
        {
           
            Debug.Log("ActivateLevelObjectivesDisplay()");
            UpdateEnemiesDisplay(0, GameManager.Instance.levelManager._currentEnemyNumber);
            if (enemyIconContainer != null) enemyIconContainer.SetActive(true);
            
            UpdateCoinsDisplay(GameManager.Instance.economyManager.Coins);
            if (coinsText != null) coinsText.gameObject.SetActive(true);
            if (coinsObj != null) coinsObj.gameObject.SetActive(true);
        }

        if (/*GameManager.Instance.levelManager._currentLevelNumber > 0 */ 
             GameManager.Instance.levelManager.GlobalLevelNumber > 0)
        {
            
            if (!tryingThisWeapon)
            {
                tryingThisWeapon = false;
                GameManager.Instance.playerController.EquipLaserGun(GameManager.Instance.shopManager.GetEquippedWeaponPrefab());
            }
            
           // GameManager.Instance._waitForTryGun = false;
            Debug.Log("ActivateLevelObjectivesDisplay()");
            GameManager.Instance.playerController.SetupTools();
        }
        
        isVIPGunAnime = false;
    }

    public void UpdateEnemiesDisplay(int currentKilledEnemies, int totalEnemiesInLevel)
    {
        if (enemyIconContainer == null || enemyIconPrefab == null)
        {
            return;
        }
       
        if (_enemyIcons.Count != totalEnemiesInLevel)
        {
            foreach (Transform child in enemyIconContainer.transform)
            {
                Destroy(child.gameObject);
            }
            _enemyIcons.Clear();

            for (int i = 0; i < totalEnemiesInLevel; i++)
            {
                GameObject iconGO = Instantiate(enemyIconPrefab, enemyIconContainer.transform);
                if (iconGO != null)
                {
                    _enemyIcons.Add(iconGO);
                }
            }
        }

        for (int i = 0; i < _enemyIcons.Count; i++)
        {
            if (_enemyIcons[i] != null)
            {
                _enemyIcons[i].transform.GetChild(0).gameObject.SetActive(i < currentKilledEnemies);
            }
        }
    }

    public void UpdateCoinsDisplay(int currentCoins) 
    {
        if (coinsText != null)
        {
            coinsText.text = currentCoins.ToString();
        }
    }

    private Sequence _coinAnimationSequence;

    public void AddCoinsToUI(int newCoinCount)
    {
        if (coinsObj == null || coinsText == null)
        {
            UpdateCoinsDisplay(newCoinCount);
            return;
        }
    
        // int value = 0;
        //
        // value = GameManager.Instance.levelManager.levelCoinsTOGet / GameManager.Instance.levelManager._currentEnemyNumber;
        // GameManager.Instance.levelManager.levelCoinsTOGet -= value;
        // Debug.Log("newcoins count = " + value + " " + GameManager.Instance.levelManager.levelCoinsTOGet);
        GameManager.Instance.economyManager.AddCoins( newCoinCount);
      
        int currentCoinCount=0;
        
        int newTotalCoinCount = GameManager.Instance.economyManager.GetCoins();
        
        coinsObj.transform.DOShakeScale(1.35f, 0.3f, 5, 90, true).OnComplete(() => coinsObj.transform.localScale = Vector3.one);
        
        if (_coinAnimationSequence == null)
        {
            _coinAnimationSequence = DOTween.Sequence();
        }
    
        // Animate the text from the old value to the new total value
        _coinAnimationSequence.Append(DOTween.To(() => currentCoinCount, x => currentCoinCount = x, newTotalCoinCount, 2f)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() => coinsText.text = Mathf.RoundToInt(currentCoinCount).ToString())
            .OnComplete(() => UpdateCoinsDisplay(newTotalCoinCount)));
        
    }
    
    
    
    public void StartKingDevilCutscene()
    {
        int currentLevelNumber = GameManager.Instance.levelManager._currentLevelNumber;

        DialogueData dialogueForLevel = kingDevilDialogueSets.FirstOrDefault(d => d.levelNumber == currentLevelNumber);

        if (dialogueForLevel == null || dialogueBox == null || dialogueText == null || dialogueForLevel.dialogues.Length == 0)
        {
            Debug.LogWarning($"No King Devil dialogue found for level {currentLevelNumber}.");
            return;
        }

        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
        }

        _cutsceneCoroutine = StartCoroutine(PlayKingDevilDialogues(dialogueForLevel.dialogues));
    }

    private IEnumerator PlayKingDevilDialogues(string[] dialoguesToPlay)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = "";
        yield return new WaitForSeconds(0.5f);
        
        foreach (string line in dialoguesToPlay)
        {
            dialogueText.text = "";
            string[] words = line.Split(' ');

            foreach (string word in words)
            {
                dialogueText.text += word + " ";
                GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Devil_Dialogue);
                yield return new WaitForSeconds(wordDisplayDelay);
            }
            
            yield return new WaitForSeconds(dialogueDisplayDuration);
        }

        dialogueBox.SetActive(false);
        _cutsceneCoroutine = null;
    }
    
    public void StartRescuedNPCCutscene(int targetCount)
    {
        if (rescuedNPCCountPanel == null || rescuedCountImage == null || numberSprites == null || numberSprites.Length < 10)
        {
            return;
        }

        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
        }
        
        rescuedNPCCountPanel.SetActive(true);
        
        if (targetCount >= 0 && targetCount < numberSprites.Length)
        {
            rescuedCountImage.sprite = numberSprites[targetCount];
        }
        
    }
    
    public void OnSkipButtonClicked()
    {
        Debug.Log("Skip button clicked. Triggering skip all releases.");

        // Check if the skip button reference is valid
        if (skipBtn != null)
        {
            // Disable the button to prevent multiple presses during the skip process
            skipBtn.interactable = false;
        }

        // Call the skip function on the CaptureDataContainer to instantly finish the animations
        // You must have a reference to the CaptureDataContainer in your GameManager for this to work.
        if (GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.GoToLastSplinePoint();
        }
    }
}

