using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

[Serializable]
public struct CapturedNPCData
{
    [Header("For Devil Head")]
    public bool isDevilHead;
    public Transform devilHeadPosition;
    public GameObject devilHeadGameObject; 
    
    [Header("For Cage Object")]
    public GameObject cageGameObject;
    public GameObject openCageGameObject;
    public GameObject cageGateGameObject;
   
    [Header("For Cage NPC")]
    public GameObject npcToRelease; // The NPC object to be animated
    public Transform npcReleasePosition; // The final position for the NPC
    public Animator npcAnimatorController;
    public ExpressionController npcExpression;
    public bool isAProps;
    public bool isCaptured;
   
    public CapturedNPCData(GameObject cage, GameObject openCage, GameObject cageGate, GameObject npc, Transform npcReleasePos, Animator npcAnim, ExpressionController npcExpression, bool isAProp, bool isDevilHead, Transform devilHeadPos, GameObject devilHeadGo)
    {
        cageGameObject = cage;
        openCageGameObject = openCage;
        cageGateGameObject = cageGate;
        npcToRelease = npc;
        npcReleasePosition = npcReleasePos;
        npcAnimatorController = npcAnim;
        this.npcExpression = npcExpression;
        this.isAProps = isAProp;
        isCaptured = true;
        this.isDevilHead = isDevilHead;
        devilHeadPosition = devilHeadPos;
        devilHeadGameObject = devilHeadGo;
    }
}

public class CaptureDataContainer : MonoBehaviour
{
    public RuntimeAnimatorController resscueAnimator;
    public Material RescueEyeTexture;
    public List<CapturedNPCData> allCapturedNPCsData = new List<CapturedNPCData>();

    public int _capturedNPC = 0;
    
    [SerializeField] private float cageAnimationDuration = 1.0f;
    [SerializeField] private float cageGateOpenAngle = -180f; 
    [SerializeField] private float devilHeadReleaseDuration = 1.5f; 

    private bool _isAnimatingRelease = false;
    private int _releasedCount = 0;
    public bool isCutScenePlayed = false;

    private void Awake()
    {
        _capturedNPC = allCapturedNPCsData.Count;
    }

    public void StartCutScene()
    {
        isCutScenePlayed = true;
        GameManager.Instance.playerController.ResetTools();
        GameManager.Instance.playerController.SetNewMovementSpeed(2);
        
        if(GameManager.Instance.levelManager._currentLevelNumber != 11)
            GameManager.Instance.playerController.StartCameraRotationForLevel(GameManager.Instance.levelManager._currentLevelNumber);
    }

    public void ReleaseOneNPC()
    {
        if (GameManager.Instance.playerController._capturedSceneSkipped)
        {
            return;
        }
        
        if (_releasedCount >= allCapturedNPCsData.Count || _isAnimatingRelease)
        {
            return;
        }

        if (GameManager.Instance.levelManager._currentLevelNumber == 11)
        {
            GameManager.Instance.audioManager.StopMusic();
        }
        
        _isAnimatingRelease = true;
        
        CapturedNPCData npcData = allCapturedNPCsData[_releasedCount];

        PrepareNPCForRelease(npcData);

        if (npcData.isDevilHead)
        {
            StartCoroutine(ReleaseDevilHead(npcData));
        }
        else
        {
            ReleaseRegularCage(npcData);
        }
    }

    public void SkipAllReleases()
    {
        if (GameManager.Instance.playerController._capturedSceneSkipped)
        {
            GameManager.Instance.playerController._capturedSceneSkipped = false;


            if (!isCutScenePlayed)
            {
                Debug.Log("Skipping all NPC release animations.");
                GameManager.Instance.playerController.StartCameraRotationForLevel(GameManager.Instance.levelManager._currentLevelNumber);
            }

            StopAllCoroutines();
            _isAnimatingRelease = false;

            
            for (int i = 0; i < allCapturedNPCsData.Count; i++)
            {
                CapturedNPCData npcData = allCapturedNPCsData[i];
                
                if (npcData.isCaptured == false)
                {
                    continue; 
                }

                if (npcData.cageGameObject != null)
                {
                    npcData.cageGameObject.SetActive(false);
                }

                if (npcData.openCageGameObject != null)
                {
                    npcData.openCageGameObject.SetActive(true);
                }

                if (npcData.cageGateGameObject != null)
                {

                    Vector3 finalRotation = npcData.cageGateGameObject.transform.localRotation.eulerAngles +
                                            new Vector3(0, cageGateOpenAngle, 0);
                    npcData.cageGateGameObject.transform.localRotation = Quaternion.Euler(finalRotation);
                }

                if (!npcData.isAProps)
                {
                    if (npcData.npcAnimatorController != null)
                    {
                        npcData.npcAnimatorController.runtimeAnimatorController = resscueAnimator;
                    }

                    if (npcData.npcExpression != null && RescueEyeTexture != null)
                    {
                        npcData.npcExpression.SetEyesMaterial(RescueEyeTexture);
                    }
                }

                if (npcData.isDevilHead)
                {
                    if (npcData.devilHeadGameObject != null && npcData.devilHeadPosition != null)
                    {
                        npcData.devilHeadGameObject.GetComponent<Animator>().enabled = false;
                        npcData.devilHeadGameObject.transform.position = npcData.devilHeadPosition.position;
                    }

                    if (npcData.npcToRelease != null && npcData.npcReleasePosition != null)
                    {
                        npcData.npcToRelease.transform.position = npcData.npcReleasePosition.position;
                    }
                }

                GameManager.Instance.levelManager.UpdateCapturedNPCSkipped();
                npcData.isCaptured = false;
                allCapturedNPCsData[i] = npcData;
            }

     
            _releasedCount = allCapturedNPCsData.Count;
            _capturedNPC = 0;

           
            StartCoroutine(CallLevelCompletion());
        }
    }
    private void PrepareNPCForRelease(CapturedNPCData npcData)
    {
        if (npcData.cageGameObject != null)
        {
            npcData.cageGameObject.SetActive(false);
        }
        if (npcData.openCageGameObject != null)
        {
            npcData.openCageGameObject.SetActive(true);
        }

        if (!npcData.isAProps)
        {
            if (npcData.npcAnimatorController != null)
            {
                npcData.npcAnimatorController.runtimeAnimatorController = resscueAnimator;
            }
            if (npcData.npcExpression != null && RescueEyeTexture != null)
            {
                npcData.npcExpression.SetEyesMaterial(RescueEyeTexture);
            }
        }
    }

    private void ReleaseRegularCage(CapturedNPCData npcData)
    {
        if (npcData.cageGateGameObject != null)
        {
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Cage_DoorOpen);
            npcData.cageGateGameObject.transform.DOLocalRotate(new Vector3(0, cageGateOpenAngle, 0), cageAnimationDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutBack)
                .OnComplete(() => HandleNPCReleased(npcData));
        }
        else
        {
            HandleNPCReleased(npcData);
        }
    }

    private IEnumerator ReleaseDevilHead(CapturedNPCData npcData)
    {
        if (!npcData.isAProps)
        {
            if (npcData.npcToRelease != null && npcData.npcReleasePosition != null)
            {
                npcData.npcToRelease.transform.DOMove(npcData.npcReleasePosition.position, devilHeadReleaseDuration)
                    .SetEase(Ease.InOutSine);
            }
        }

        if (npcData.cageGameObject != null)
        {
            npcData.cageGameObject.transform.DOScaleY(0, devilHeadReleaseDuration)
                .SetEase(Ease.InOutSine);
        }

        if (npcData.devilHeadGameObject != null && npcData.devilHeadPosition != null)
        {
            npcData.devilHeadGameObject.GetComponent<Animator>().enabled = false;
            npcData.devilHeadGameObject.transform.DOMove(npcData.devilHeadPosition.position, devilHeadReleaseDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => HandleNPCReleased(npcData));
        }
        else
        {
            HandleNPCReleased(npcData);
        }
        
        yield return null;
    }
    
    private void HandleNPCReleased(CapturedNPCData releasedNpcData)
    {
        for (int i = 0; i < allCapturedNPCsData.Count; i++)
        {
            if (allCapturedNPCsData[i].Equals(releasedNpcData))
            {
                var temp = allCapturedNPCsData[i];
                temp.isCaptured = false;
                allCapturedNPCsData[i] = temp;
                break;
            }
        }

        _capturedNPC--;
        _releasedCount++;
        _isAnimatingRelease = false;
        
        if (_releasedCount < allCapturedNPCsData.Count)
        {
            GameManager.Instance.uiManager.NPCRescuedCount(_releasedCount);
            GameManager.Instance.levelManager.UpdateCapturedNPC();
        }
        else
        {
            StartCoroutine(CallLevelCompletion());
        }
    }

    IEnumerator CallLevelCompletion()
    {
        isCutScenePlayed = false;
        Debug.Log("CallLevelCompletion() ======");
        GameManager.Instance.levelManager._totalFreedNPC += _releasedCount;
        GameManager.Instance.progressionManager.SaveTotalFreedNPC(GameManager.Instance.levelManager._totalFreedNPC);
        GameManager.Instance.uiManager.NPCRescuedCount(_releasedCount);
        GameManager.Instance.levelManager.UpdateCapturedNPC();
        yield return new WaitForSecondsRealtime(1f);
        
        GameManager.Instance.levelManager.CheckLevelCompletionCount();
    }

    public void AddNPCData(GameObject cage, GameObject openCage, GameObject cageGate, GameObject npc, Transform npcReleasePos, Animator npcAnim, ExpressionController npcExpression, bool isAProp, bool isDevilHead, Transform devilHeadPos, GameObject devilHeadGo)
    {
        allCapturedNPCsData.Add(new CapturedNPCData(cage, openCage, cageGate, npc, npcReleasePos, npcAnim, npcExpression, isAProp, isDevilHead, devilHeadPos, devilHeadGo));
        _capturedNPC = allCapturedNPCsData.Count;
    }

    public void RotateCameraToCell(float value)
    {
        Vector3 targetRotation = new Vector3(0f, value, 0f);

        GameManager.Instance.playerController.RotateToTarget(targetRotation, 0.7f);

        if (GameManager.Instance.levelManager._currentLevelNumber == 11)
        {
            Debug.Log("RotateCameraToCell(float value)");
             Invoke("ReleaseOneNPC",0.5f);
        }
    }
    
    
}