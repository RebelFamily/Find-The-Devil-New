using UnityEngine;
using System.Collections;
using System.Linq;

public class ExpressionController : MonoBehaviour
{
    public enum ExpressionTypeDevil
    {
        EvilTalk,
        EvilAngrySmile,
        EvilSmile,
        EvilAngry,
        EvilSadWoow,
        EyeBlink,
    }

    [Header("Core Components")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    [SerializeField] private Material eyesMaterial;
    [SerializeField] private Material noseMaterial;
    [SerializeField] private Material mouthMaterial;

    private Material _runtimeEyesMaterial;
    private Material _runtimeNouthMaterial;
    private Material _runtimeMouthMaterial;

    private Texture _originalEyesTexture;
    private Texture _originalNoseTexture;
    private Texture _originalMouthTexture;

    [Header("-------------******Devil Expression******-------------")]
    public bool isDevilExpression;

    [Header("Devil Blend Shape Names (Case-sensitive)")]
    [SerializeField] private string evilTalkBlendShapeName = "Evil Talk";
    [SerializeField] private string evilAngrySmileBlendShapeName = "Evil Angry Smile";
    [SerializeField] private string evilSmileBlendShapeName = "Evil Smile";
    [SerializeField] private string evilAngryBlendShapeName = "Evil Angry";
    [SerializeField] private string evilSadWoowBlendShapeName = "Evil Sad Woow";
    [SerializeField] private string devilEyeBlinkBlendShapeName = "Eye Blink";

    [Header("Devil Talking Animation Settings")]
    [SerializeField] private bool enableDevilTalkingAnimation = false;
    [SerializeField] private float devilTalkingAnimationSpeed = 0.2f;
    [SerializeField] private float devilMinTalkWeight = 0f;
    [SerializeField] private float devilMaxTalkWeight = 100f;
    [SerializeField] private float devilTalkInterval = 0.05f;

    [Header("Devil Blink Animation Settings")]
    [SerializeField] private bool enableDevilBlinkAnimation = false;
    [SerializeField] private float devilBlinkDuration = 0.1f;
    [SerializeField] private float devilMinDelayBetweenBlinks = 2f;
    [SerializeField] private float devilMaxDelayBetweenBlinks = 5f;
    [SerializeField] private float devilBlinkClosedWeight = 100f;
    [SerializeField] private float devilBlinkOpenWeight = 0f;

    [Header("Devil Evil Smile Settings")]
    [SerializeField] private bool enableDevilEvilSmile = false;
    [SerializeField] private float devilEvilSmileAnimationSpeed = 1f;
    [SerializeField] private float devilMinEvilSmileWeight = 0f;
    [SerializeField] private float devilMaxEvilSmileWeight = 100f;
    [SerializeField] private float devilMinDelayBetweenEvilSmiles = 3f;
    [SerializeField] private float devilMaxDelayBetweenEvilSmiles = 8f;

    [Header("Devil Evil Angry Smile Settings")]
    [SerializeField] private bool enableDevilEvilAngrySmile = false;
    [SerializeField] private float devilEvilAngrySmileAnimationSpeed = 1f;
    [SerializeField] private float devilMinEvilAngrySmileWeight = 0f;
    [SerializeField] private float devilMaxEvilAngrySmileWeight = 100f;
    [SerializeField] private float devilMinDelayBetweenEvilAngrySmiles = 3f;
    [SerializeField] private float devilMaxDelayBetweenEvilAngrySmiles = 8f;

    [Header("Devil Angry Settings")]
    [SerializeField] private bool enableDevilAngry = false;
    [SerializeField] private float devilAngryAnimationSpeed = 1f;
    [SerializeField] private float devilMinAngryWeight = 0f;
    [SerializeField] private float devilMaxAngryWeight = 100f;
    [SerializeField] private float devilMinDelayBetweenAngry = 3f;
    [SerializeField] private float devilMaxDelayBetweenAngry = 8f;

    [Header("Devil Sad Woow Settings")]
    [SerializeField] private bool enableDevilSadWoow = false;
    [SerializeField] private float devilSadWoowAnimationSpeed = 1f;
    [SerializeField] private float devilMinSadWoowWeight = 0f;
    [SerializeField] private float devilMaxSadWoowWeight = 100f;
    [SerializeField] private float devilMinDelayBetweenSadWoow = 3f;
    [SerializeField] private float devilMaxDelayBetweenSadWoow = 8f;


    [Header("-------------******NPC Expression Textures******-------------")]
    [SerializeField] private Texture2D npcOriginalEyesTexture;
    [SerializeField] private Texture2D npcOriginalMouthTexture;

    [Header("------Talking Animation Settings (NPC)------")]
    [SerializeField] private bool enableTalkingAnimation = false;
    [SerializeField] private float talkingAnimationInterval = 0.1f;
    [SerializeField] private Texture2D[] talkingMouthTextures;
    [SerializeField] private float minDelayBetweenTalk = 1f;
    [SerializeField] private float maxDelayBetweenTalk = 2f;
    [SerializeField] private float minTalkDuration = 1f;
    [SerializeField] private float maxTalkDuration = 2f;

    [Header("Blink Animation Settings (NPC)")]
    [SerializeField]bool enableBlinkAnimation;
    [SerializeField] private Texture2D blinkEyeOpenTexture;
    [SerializeField] private Texture2D blinkEyeClosedTexture;
    [SerializeField] private float blinkDuration = 0.05f;
    [SerializeField] private float minDelayBetweenBlinks = 2f;
    [SerializeField] private float maxDelayBetweenBlinks = 5f;

    [Header("Death Expression Settings (NPC)")]
    [SerializeField] private Texture2D deathEyesTexture;
    [SerializeField] private Texture2D deathMouthTexture;

    [Header("Yawn Expression Settings (NPC)")]
    [SerializeField] private Texture2D yawnEyesTexture;
    [SerializeField] private Texture2D yawnMouthTexture;

    [Header("Terrify Expression Settings (NPC)")]
    [SerializeField] private Texture2D terrifyEyesTexture;
    [SerializeField] private Texture2D terrifyMouthTexture;

    private Coroutine _npcTalkingCoroutine;
    private Coroutine _npcBlinkingCoroutine;

    private Coroutine _devilTalkingCoroutine;
    private Coroutine _devilBlinkingCoroutine;
    private Coroutine _devilEvilSmileCoroutine;
    private Coroutine _devilEvilAngrySmileCoroutine; // New
    private Coroutine _devilAngryCoroutine; // New
    private Coroutine _devilSadWoowCoroutine; // New

    private bool _isDead = false;
    private bool _isYawning = false;
    private bool _isTerrified = false;

    private int _evilTalkBlendShapeIndex = -1;
    private int _evilAngrySmileBlendShapeIndex = -1;
    private int _evilSmileBlendShapeIndex = -1;
    private int _evilAngryBlendShapeIndex = -1;
    private int _evilSadWoowBlendShapeIndex = -1;
    private int _devilEyeBlinkBlendShapeIndex = -1;

    private void Awake()
    {
        if (skinnedMeshRenderer == null)
        {
            enabled = false;
            return;
        }

        if (!isDevilExpression)
        {
            AssignSpecifiedMaterials();
            _runtimeEyesMaterial = skinnedMeshRenderer.materials[0];
            _runtimeMouthMaterial = skinnedMeshRenderer.materials[1];
            _runtimeNouthMaterial = skinnedMeshRenderer.materials[2];
        }

        if (isDevilExpression)
        {
            Mesh mesh = skinnedMeshRenderer.sharedMesh;
            _evilTalkBlendShapeIndex = mesh.GetBlendShapeIndex(evilTalkBlendShapeName);
            _evilAngrySmileBlendShapeIndex = mesh.GetBlendShapeIndex(evilAngrySmileBlendShapeName);
            _evilSmileBlendShapeIndex = mesh.GetBlendShapeIndex(evilSmileBlendShapeName);
            _evilAngryBlendShapeIndex = mesh.GetBlendShapeIndex(evilAngryBlendShapeName);
            _evilSadWoowBlendShapeIndex = mesh.GetBlendShapeIndex(evilSadWoowBlendShapeName);
            _devilEyeBlinkBlendShapeIndex = mesh.GetBlendShapeIndex(devilEyeBlinkBlendShapeName);

            if (_evilTalkBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{evilTalkBlendShapeName}' not found on mesh.", this);
            if (_evilAngrySmileBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{evilAngrySmileBlendShapeName}' not found on mesh.", this);
            if (_evilSmileBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{evilSmileBlendShapeName}' not found on mesh.", this);
            if (_evilAngryBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{evilAngryBlendShapeName}' not found on mesh.", this);
            if (_evilSadWoowBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{evilSadWoowBlendShapeName}' not found on mesh.", this);
            if (_devilEyeBlinkBlendShapeIndex == -1) Debug.LogWarning($"Blend shape '{devilEyeBlinkBlendShapeName}' not found on mesh.", this);
        }

        if (!isDevilExpression)
        {
            if (npcOriginalEyesTexture != null)
            {
                _originalEyesTexture = npcOriginalEyesTexture;
            }
            else
            {
                Debug.LogWarning("NPC Original Eyes Texture is not assigned. Using current material texture as original.", this);
                _originalEyesTexture = _runtimeEyesMaterial.mainTexture;
            }

            if (npcOriginalMouthTexture != null)
            {
                _originalMouthTexture = npcOriginalMouthTexture;
            }
            else
            {
                Debug.LogWarning("NPC Original Mouth Texture is not assigned. Using current material texture as original.", this);
                _originalMouthTexture = _runtimeMouthMaterial.mainTexture;
            }

            _originalNoseTexture = _runtimeNouthMaterial.mainTexture;

            _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
            _runtimeMouthMaterial.mainTexture = _originalMouthTexture;


            if (enableTalkingAnimation)
            {
                Talking(true);
            }

           
            if (enableBlinkAnimation)
            {
                Blink(true);
            }
        }
        else
        {
            if (enableDevilTalkingAnimation)
            {
                DevilTalking(true);
            }

            if (enableDevilBlinkAnimation)
            {
                DevilBlink(true);
            }

            if (enableDevilEvilSmile)
            {
                DevilEvilSmile(true);
            }

            if (enableDevilEvilAngrySmile) // New
            {
                DevilEvilAngrySmile(true);
            }

            if (enableDevilAngry) // New
            {
                DevilAngry(true);
            }

            if (enableDevilSadWoow) // New
            {
                DevilSadWoow(true);
            }
        }
    }

    private void AssignSpecifiedMaterials()
    {
        Material[] currentMaterials = skinnedMeshRenderer.materials;
        int minimumRequiredSlots = 3;

        if (currentMaterials.Length < minimumRequiredSlots)
        {
            Material[] newMaterialsArray = new Material[minimumRequiredSlots];
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                newMaterialsArray[i] = currentMaterials[i];
            }
            currentMaterials = newMaterialsArray;
        }

        if (eyesMaterial != null)
        {
            currentMaterials[0] = eyesMaterial;
        }
        else
        {
            Debug.LogWarning("ExpressionController: Eyes Material is not assigned in the Inspector. Using existing material at slot 0.", this);
        }

        if (mouthMaterial != null)
        {
            currentMaterials[1] = mouthMaterial;
        }
        else
        {
            Debug.LogWarning("ExpressionController: Mouth Material is not assigned in the Inspector. Using existing material at slot 1.", this);
        }

        if (noseMaterial != null)
        {
            currentMaterials[2] = noseMaterial;
        }
        else
        {
            Debug.LogWarning("ExpressionController: Nose Material is not assigned in the Inspector. Using existing material at slot 2.", this);
        }

        skinnedMeshRenderer.materials = currentMaterials;
    }

    private void StopAllExpressions()
    {
        if (_npcTalkingCoroutine != null)
        {
            StopCoroutine(_npcTalkingCoroutine);
            _npcTalkingCoroutine = null;
        }
        if (_npcBlinkingCoroutine != null)
        {
            StopCoroutine(_npcBlinkingCoroutine);
            _npcBlinkingCoroutine = null;
        }
        if (!isDevilExpression)
        {
            _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
            _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
        }


        if (_devilTalkingCoroutine != null)
        {
            StopCoroutine(_devilTalkingCoroutine);
            _devilTalkingCoroutine = null;
        }
        if (_devilBlinkingCoroutine != null)
        {
            StopCoroutine(_devilBlinkingCoroutine);
            _devilBlinkingCoroutine = null;
        }
        if (_devilEvilSmileCoroutine != null)
        {
            StopCoroutine(_devilEvilSmileCoroutine);
            _devilEvilSmileCoroutine = null;
        }
        if (_devilEvilAngrySmileCoroutine != null) // New
        {
            StopCoroutine(_devilEvilAngrySmileCoroutine);
            _devilEvilAngrySmileCoroutine = null;
        }
        if (_devilAngryCoroutine != null) // New
        {
            StopCoroutine(_devilAngryCoroutine);
            _devilAngryCoroutine = null;
        }
        if (_devilSadWoowCoroutine != null) // New
        {
            StopCoroutine(_devilSadWoowCoroutine);
            _devilSadWoowCoroutine = null;
        }

        if (isDevilExpression)
        {
            ResetAllBlendShapes();
        }
    }

    public void ResetAllBlendShapes()
    {
        if (skinnedMeshRenderer == null) return;

        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }

    public void SetDevilExpression(ExpressionTypeDevil expressionType, float weight)
    {
        if (!isDevilExpression)
        {
            Debug.LogWarning($"SetDevilExpression called on non-Devil character: {gameObject.name}", this);
            return;
        }

        if (_devilTalkingCoroutine == null && _devilBlinkingCoroutine == null && _devilEvilSmileCoroutine == null &&
            _devilEvilAngrySmileCoroutine == null && _devilAngryCoroutine == null && _devilSadWoowCoroutine == null) // Updated
        {
             ResetAllBlendShapes();
        }


        int blendShapeIndex = -1;
        string blendShapeName = "";

        switch (expressionType)
        {
            case ExpressionTypeDevil.EvilTalk:
                blendShapeIndex = _evilTalkBlendShapeIndex;
                blendShapeName = evilTalkBlendShapeName;
                break;
            case ExpressionTypeDevil.EvilAngrySmile:
                blendShapeIndex = _evilAngrySmileBlendShapeIndex;
                blendShapeName = evilAngrySmileBlendShapeName;
                break;
            case ExpressionTypeDevil.EvilSmile:
                blendShapeIndex = _evilSmileBlendShapeIndex;
                blendShapeName = evilSmileBlendShapeName;
                break;
            case ExpressionTypeDevil.EvilAngry:
                blendShapeIndex = _evilAngryBlendShapeIndex;
                blendShapeName = evilAngryBlendShapeName;
                break;
            case ExpressionTypeDevil.EvilSadWoow:
                blendShapeIndex = _evilSadWoowBlendShapeIndex;
                blendShapeName = evilSadWoowBlendShapeName;
                break;
            case ExpressionTypeDevil.EyeBlink:
                blendShapeIndex = _devilEyeBlinkBlendShapeIndex;
                blendShapeName = devilEyeBlinkBlendShapeName;
                break;
            default:
                Debug.LogWarning($"Unknown Devil ExpressionType: {expressionType}", this);
                return;
        }

        if (blendShapeIndex != -1)
        {
            
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, Mathf.Clamp(weight, 0f, 100f));
        }
        else
        {
            Debug.LogWarning($"Blend shape '{blendShapeName}' for {expressionType} not found or invalid index.", this);
        }
    }

    public void DevilTalking(bool isTalking)
    {
       
        if (!isDevilExpression) return;

        if (isTalking)
        {
            StopDevilContinuousAnimationsExcept(null);
        }
        
        SetDevilExpression(ExpressionTypeDevil.EvilTalk, isTalking ? devilMaxTalkWeight : devilMinTalkWeight);

        if (isTalking)
        {
            if (_devilTalkingCoroutine == null)
            {
                _devilTalkingCoroutine = StartCoroutine(AnimateDevilTalking());
            }
        }
        else
        {
            if (_devilTalkingCoroutine != null)
            {
                StopCoroutine(_devilTalkingCoroutine);
                _devilTalkingCoroutine = null;
            }
            if (_evilTalkBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_evilTalkBlendShapeIndex, devilMinTalkWeight);
            }
        }
    }

    private IEnumerator AnimateDevilTalking()
    {
        float currentWeight = skinnedMeshRenderer.GetBlendShapeWeight(_evilTalkBlendShapeIndex);
        bool increasing = currentWeight < devilMaxTalkWeight;

        while (true)
        { 
            if (!isDevilExpression)
            {
                if (_evilTalkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilTalkBlendShapeIndex, devilMinTalkWeight);
                _devilTalkingCoroutine = null;
                yield break;
            }
            
            if (_evilTalkBlendShapeIndex != -1) 
            {
                currentWeight += (increasing ? 1 : -1) * (100f / devilTalkingAnimationSpeed) * Time.deltaTime;

                if (increasing)
                {
                    if (currentWeight >= devilMaxTalkWeight)
                    {
                        currentWeight = devilMaxTalkWeight;
                        increasing = false; 
                    }
                }
                else
                {
                    if (currentWeight <= devilMinTalkWeight)
                    {
                        currentWeight = devilMinTalkWeight;
                        increasing = true;
                    }
                }
                skinnedMeshRenderer.SetBlendShapeWeight(_evilTalkBlendShapeIndex, currentWeight);
            }
            yield return null;
        }
    }

    
    
    
    public void DevilBlink(bool isBlinking)
    {
        if (!isDevilExpression) return;

        if (isBlinking)
        {
            StopDevilContinuousAnimationsExcept(null);
        }

        if (isBlinking)
        {
            if (_devilBlinkingCoroutine == null)
            {
                _devilBlinkingCoroutine = StartCoroutine(AnimateDevilBlinking());
            }
        }
        else
        {
            if (_devilBlinkingCoroutine != null)
            {
                StopCoroutine(_devilBlinkingCoroutine);
                _devilBlinkingCoroutine = null;
            }
            if (_devilEyeBlinkBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkOpenWeight);
            }
        }
    }

    private IEnumerator AnimateDevilBlinking()
    {
        while (true)
        {
            if (!isDevilExpression)
            {
                if (_devilEyeBlinkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkOpenWeight);
                _devilBlinkingCoroutine = null;
                yield break;
            }

            float delay = Random.Range(devilMinDelayBetweenBlinks, devilMaxDelayBetweenBlinks);
            yield return new WaitForSeconds(delay);

            if (!isDevilExpression)
            {
                if (_devilEyeBlinkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkOpenWeight);
                _devilBlinkingCoroutine = null;
                yield break;
            }

            if (_devilEyeBlinkBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkClosedWeight);
                yield return new WaitForSeconds(devilBlinkDuration);
                skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkOpenWeight);
            }
        }
    }

    public void DevilEvilSmile(bool isSmiling)
    {
        if (!isDevilExpression)
        {
            Debug.LogWarning($"DevilEvilSmile called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
            return;
        }

        if (isSmiling)
        {
            StopDevilContinuousAnimationsExcept(null);
        }

        if (isSmiling)
        {
            if (_devilEvilSmileCoroutine == null)
            {
                _devilEvilSmileCoroutine = StartCoroutine(AnimateDevilEvilSmile());
            }
        }
        else
        {
            if (_devilEvilSmileCoroutine != null)
            {
                StopCoroutine(_devilEvilSmileCoroutine);
                _devilEvilSmileCoroutine = null;
            }
            if (_evilSmileBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_evilSmileBlendShapeIndex, devilMinEvilSmileWeight);
            }
        }
    }

    private IEnumerator AnimateDevilEvilSmile()
    {
        float currentWeight = devilMinEvilSmileWeight;
        bool increasing = true;

        while (true)
        {
            if (!isDevilExpression)
            {
                if (_evilSmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSmileBlendShapeIndex, devilMinEvilSmileWeight);
                _devilEvilSmileCoroutine = null;
                yield break;
            }

            if (_evilSmileBlendShapeIndex != -1)
            {
                currentWeight += (increasing ? 1 : -1) * (100f / devilEvilSmileAnimationSpeed) * Time.deltaTime;

                if (increasing)
                {
                    if (currentWeight >= devilMaxEvilSmileWeight)
                    {
                        currentWeight = devilMaxEvilSmileWeight;
                        increasing = false;
                        yield return new WaitForSeconds(devilMinDelayBetweenEvilSmiles);
                    }
                }
                else
                {
                    if (currentWeight <= devilMinEvilSmileWeight)
                    {
                        currentWeight = devilMinEvilSmileWeight;
                        increasing = true;
                        yield return new WaitForSeconds(Random.Range(devilMinDelayBetweenEvilSmiles, devilMaxDelayBetweenEvilSmiles));
                    }
                }
                skinnedMeshRenderer.SetBlendShapeWeight(_evilSmileBlendShapeIndex, currentWeight);
            }
            yield return null;
        }
    }

    // New: DevilEvilAngrySmile
    public void DevilEvilAngrySmile(bool isSmiling)
    {
        if (!isDevilExpression)
        {
            Debug.LogWarning($"DevilEvilAngrySmile called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
            return;
        }

        if (isSmiling)
        {
            StopDevilContinuousAnimationsExcept(null);
        }

        if (isSmiling)
        {
            if (_devilEvilAngrySmileCoroutine == null)
            {
                _devilEvilAngrySmileCoroutine = StartCoroutine(AnimateDevilEvilAngrySmile());
            }
        }
        else
        {
            if (_devilEvilAngrySmileCoroutine != null)
            {
                StopCoroutine(_devilEvilAngrySmileCoroutine);
                _devilEvilAngrySmileCoroutine = null;
            }
            if (_evilAngrySmileBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_evilAngrySmileBlendShapeIndex, devilMinEvilAngrySmileWeight);
            }
        }
    }

    private IEnumerator AnimateDevilEvilAngrySmile()
    {
        float currentWeight = devilMinEvilAngrySmileWeight;
        bool increasing = true;

        while (true)
        {
            if (!isDevilExpression)
            {
                if (_evilAngrySmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngrySmileBlendShapeIndex, devilMinEvilAngrySmileWeight);
                _devilEvilAngrySmileCoroutine = null;
                yield break;
            }

            if (_evilAngrySmileBlendShapeIndex != -1)
            {
                currentWeight += (increasing ? 1 : -1) * (100f / devilEvilAngrySmileAnimationSpeed) * Time.deltaTime;

                if (increasing)
                {
                    if (currentWeight >= devilMaxEvilAngrySmileWeight)
                    {
                        currentWeight = devilMaxEvilAngrySmileWeight;
                        increasing = false;
                        yield return new WaitForSeconds(devilMinDelayBetweenEvilAngrySmiles);
                    }
                }
                else
                {
                    if (currentWeight <= devilMinEvilAngrySmileWeight)
                    {
                        currentWeight = devilMinEvilAngrySmileWeight;
                        increasing = true;
                        yield return new WaitForSeconds(Random.Range(devilMinDelayBetweenEvilAngrySmiles, devilMaxDelayBetweenEvilAngrySmiles));
                    }
                }
                skinnedMeshRenderer.SetBlendShapeWeight(_evilAngrySmileBlendShapeIndex, currentWeight);
            }
            yield return null;
        }
    }

    // New: DevilAngry
    public void DevilAngry(bool isAngry)
    {
        if (!isDevilExpression)
        {
            Debug.LogWarning($"DevilAngry called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
            return;
        }

        if (isAngry)
        {
            StopDevilContinuousAnimationsExcept(null);
        }

        if (isAngry)
        {
            if (_devilAngryCoroutine == null)
            {
                _devilAngryCoroutine = StartCoroutine(AnimateDevilAngry());
            }
        }
        else
        {
            if (_devilAngryCoroutine != null)
            {
                StopCoroutine(_devilAngryCoroutine);
                _devilAngryCoroutine = null;
            }
            if (_evilAngryBlendShapeIndex != -1)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(_evilAngryBlendShapeIndex, devilMinAngryWeight);
            }
        }
    }

    private IEnumerator AnimateDevilAngry()
    {
        float currentWeight = devilMinAngryWeight;
        bool increasing = true;

        while (true)
        {
            if (!isDevilExpression)
            {
                if (_evilAngryBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngryBlendShapeIndex, devilMinAngryWeight);
                _devilAngryCoroutine = null;
                yield break;
            }

            if (_evilAngryBlendShapeIndex != -1)
            {
                currentWeight += (increasing ? 1 : -1) * (100f / devilAngryAnimationSpeed) * Time.deltaTime;

                if (increasing)
                {
                    if (currentWeight >= devilMaxAngryWeight)
                    {
                        currentWeight = devilMaxAngryWeight;
                        increasing = false;
                        yield return new WaitForSeconds(devilMinDelayBetweenAngry);
                    }
                }
                else
                {
                    if (currentWeight <= devilMinAngryWeight)
                    {
                        currentWeight = devilMinAngryWeight;
                        increasing = true;
                        yield return new WaitForSeconds(Random.Range(devilMinDelayBetweenAngry, devilMaxDelayBetweenAngry));
                    }
                }
                skinnedMeshRenderer.SetBlendShapeWeight(_evilAngryBlendShapeIndex, currentWeight);
            }
            yield return null;
        }
    }

    // New: DevilSadWoow
    public void DevilSadWoow(bool isSadWoow)
    {
        
           // Debug.Log($"DevilSadWoow called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
        if (!isDevilExpression)
        {
            return;
        }

        if (isSadWoow)
        {
           // Debug.Log($"DevilSadWoow called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
            StopDevilContinuousAnimationsExcept(null);
        }

        if (isSadWoow)
        {
            if (_devilSadWoowCoroutine == null)
            {
              //  Debug.Log($"DevilSadWoow called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
                _devilSadWoowCoroutine = StartCoroutine(AnimateDevilSadWoow());
            }
        }
        else
        {
                
              //  Debug.Log($"DevilSadWoow called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
            if (_devilSadWoowCoroutine != null)
            {
                StopCoroutine(_devilSadWoowCoroutine);
                _devilSadWoowCoroutine = null;
            }
            if (_evilSadWoowBlendShapeIndex != -1)
            {
               // Debug.Log($"DevilSadWoow called on non-Devil character: {gameObject.name}. This function is only for Devil expressions.", this);
                skinnedMeshRenderer.SetBlendShapeWeight(_evilSadWoowBlendShapeIndex, devilMinSadWoowWeight);  
            }
        }
    }

    private IEnumerator AnimateDevilSadWoow()
    {
        float currentWeight = devilMinSadWoowWeight;
        bool increasing = true;

        while (true)
        {
            if (!isDevilExpression)
            {
                if (_evilSadWoowBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSadWoowBlendShapeIndex, devilMinSadWoowWeight);
                _devilSadWoowCoroutine = null;
                yield break;
            }

            if (_evilSadWoowBlendShapeIndex != -1)
            {
                currentWeight += (increasing ? 1 : -1) * (100f / devilSadWoowAnimationSpeed) * Time.deltaTime;

                if (increasing)
                {
                    if (currentWeight >= devilMaxSadWoowWeight)
                    {
                        currentWeight = devilMaxSadWoowWeight;
                        increasing = false;
                        yield return new WaitForSeconds(devilMinDelayBetweenSadWoow);
                    }
                }
                else
                {
                    if (currentWeight <= devilMinSadWoowWeight)
                    {
                        currentWeight = devilMinSadWoowWeight;
                        increasing = true;
                        yield return new WaitForSeconds(Random.Range(devilMinDelayBetweenSadWoow, devilMaxDelayBetweenSadWoow));
                    }
                }
                skinnedMeshRenderer.SetBlendShapeWeight(_evilSadWoowBlendShapeIndex, currentWeight);
                // fix it
                skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, currentWeight);
            }
            yield return null;
        }
    }


    public void Talking(bool isTalking)
    {
        if (isDevilExpression) return;

        if ((_isDead || _isYawning || _isTerrified) && isTalking)
        {
            return;
        }

        StopDevilAnimations();

        if (isTalking)
        {
            if (_npcTalkingCoroutine == null)
            {
                _npcTalkingCoroutine = StartCoroutine(AnimateTalking());
            }
        }
        else
        {
            if (_npcTalkingCoroutine != null)
            {
                StopCoroutine(_npcTalkingCoroutine);
                _npcTalkingCoroutine = null;
            }
            if (!_isDead && !_isYawning && !_isTerrified)
            {
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
            }
        }
    }

    private IEnumerator AnimateTalking()
    {
        while (true)
        {
            if (_isDead || _isYawning || _isTerrified || isDevilExpression)
            {
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
                _npcTalkingCoroutine = null;
                yield break;
            }

            float delay = Random.Range(minDelayBetweenTalk, maxDelayBetweenTalk);
            yield return new WaitForSeconds(delay);

            if (_isDead || _isYawning || _isTerrified || isDevilExpression)
            {
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
                _npcTalkingCoroutine = null;
                yield break;
            }

            float talkDuration = Random.Range(minTalkDuration, maxTalkDuration);
            float currentTalkTime = 0f;
            int mouthFrame = 0;

            while (currentTalkTime < talkDuration)
            {
                if (_isDead || _isYawning || _isTerrified || isDevilExpression)
                {
                    _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
                    _npcTalkingCoroutine = null;
                    yield break;
                }

                if (talkingMouthTextures != null && talkingMouthTextures.Any())
                {
                    _runtimeMouthMaterial.mainTexture = talkingMouthTextures[mouthFrame % talkingMouthTextures.Length];
                }
                else
                {
                    _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
                }
                mouthFrame++;

                yield return new WaitForSeconds(talkingAnimationInterval);
                currentTalkTime += talkingAnimationInterval;
            }

            if (!_isDead && !_isYawning && !_isTerrified && !isDevilExpression)
            {
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
            }
        }
    }

    public void Blink(bool isBlinking)
    {
        if (isDevilExpression) return;

        if ((_isDead || _isYawning || _isTerrified) && isBlinking)
        {
            return;
        }

        StopDevilAnimations();

        if (isBlinking)
        {
            if (_npcBlinkingCoroutine == null)
            {
                _npcBlinkingCoroutine = StartCoroutine(AnimateBlinking());
            }
        }
        else
        {
            if (_npcBlinkingCoroutine != null)
            {
                StopCoroutine(_npcBlinkingCoroutine);
                _npcBlinkingCoroutine = null;
            }
            if (!_isDead && !_isYawning && !_isTerrified)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
            }
        }
    }

    private IEnumerator AnimateBlinking()
    {
        while (true)
        {
            if (_isDead || _isYawning || _isTerrified || isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _npcBlinkingCoroutine = null;
                yield break;
            }

            float delay = Random.Range(minDelayBetweenBlinks, maxDelayBetweenBlinks);
            yield return new WaitForSeconds(delay);

            if (_isDead || _isYawning || _isTerrified || isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _npcBlinkingCoroutine = null;
                yield break;
            }

            if (blinkEyeClosedTexture != null)
            {
                _runtimeEyesMaterial.mainTexture = blinkEyeClosedTexture;
                yield return new WaitForSeconds(blinkDuration);
            }

            if (_isDead || _isYawning || _isTerrified || isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _npcBlinkingCoroutine = null;
                yield break;
            }

            if (blinkEyeOpenTexture != null)
            {
                _runtimeEyesMaterial.mainTexture = blinkEyeOpenTexture;
            }
            else
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
            }
        }
    }

    public void SetDeathExpression(bool isDeadState)
    {
        if (isDevilExpression && isDeadState) return;

        _isDead = isDeadState;

        if (isDeadState)
        {
            StopAllExpressions();

            if (!isDevilExpression)
            {
                if (deathEyesTexture != null)
                {
                    _runtimeEyesMaterial.mainTexture = deathEyesTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Death Eyes Texture is not assigned. Eyes will not change for death expression.", this);
                }

                if (deathMouthTexture != null)
                {
                    _runtimeMouthMaterial.mainTexture = deathMouthTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Death Mouth Texture is not assigned. Mouth will not change for death expression.", this);
                }
            }
        }
        else
        {
            if (!isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;

                if (enableTalkingAnimation) Talking(true);
                if (enableBlinkAnimation) Blink(true);
            }
        }
    }

    public void SetYawnExpression(bool isYawningState)
    {
        if (isDevilExpression && isYawningState) return;

        if ((_isDead || _isTerrified) && isYawningState)
        {
            Debug.LogWarning("ExpressionController: Cannot set Yawn expression while character is dead or terrified.", this);
            return;
        }

        _isYawning = isYawningState;

        if (isYawningState)
        {
            StopAllExpressions();

            if (!isDevilExpression)
            {
                if (yawnEyesTexture != null)
                {
                    _runtimeEyesMaterial.mainTexture = yawnEyesTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Yawn Eyes Texture is not assigned. Eyes will not change for yawn expression.", this);
                }

                if (yawnMouthTexture != null)
                {
                    _runtimeMouthMaterial.mainTexture = yawnMouthTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Yawn Mouth Texture is not assigned. Mouth will not change for yawn expression.", this);
                }
            }
        }
        else
        {
            if (!_isDead && !_isTerrified && !isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;

                if (enableTalkingAnimation) Talking(true);
                if (enableBlinkAnimation) Blink(true);
            }
        }
    }

    public void Terrify(bool isTerrifiedState)
    {
        if (isDevilExpression && isTerrifiedState) return;

        if (_isDead && isTerrifiedState)
        {
            Debug.LogWarning("ExpressionController: Cannot set Terrify expression while character is dead.", this);
            return;
        }

        _isTerrified = isTerrifiedState;

        if (isTerrifiedState)
        {
            StopAllExpressions();

            if (!isDevilExpression)
            {
                if (terrifyEyesTexture != null)
                {
                    _runtimeEyesMaterial.mainTexture = terrifyEyesTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Terrify Eyes Texture is not assigned. Eyes will not change for terrify expression.", this);
                }

                if (terrifyMouthTexture != null)
                {
                    _runtimeMouthMaterial.mainTexture = terrifyMouthTexture;
                }
                else
                {
                    Debug.LogWarning("ExpressionController: Terrify Mouth Texture is not assigned. Mouth will not change for terrify expression.", this);
                }
            }
        }
        else
        {
            if (!_isDead && !isDevilExpression)
            {
                _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
                _runtimeMouthMaterial.mainTexture = _originalMouthTexture;

                if (enableTalkingAnimation) Talking(true);
                if (enableBlinkAnimation) Blink(true);
            }
        }
    }

    private void StopDevilAnimations()
    {
        if (_devilTalkingCoroutine != null)
        {
            StopCoroutine(_devilTalkingCoroutine);
            _devilTalkingCoroutine = null;
        }
        if (_devilBlinkingCoroutine != null)
        {
            StopCoroutine(_devilBlinkingCoroutine);
            _devilBlinkingCoroutine = null;
        }
        if (_devilEvilSmileCoroutine != null)
        {
            StopCoroutine(_devilEvilSmileCoroutine);
            _devilEvilSmileCoroutine = null;
        }
        if (_devilEvilAngrySmileCoroutine != null) // New
        {
            StopCoroutine(_devilEvilAngrySmileCoroutine);
            _devilEvilAngrySmileCoroutine = null;
        }
        if (_devilAngryCoroutine != null) // New
        {
            StopCoroutine(_devilAngryCoroutine);
            _devilAngryCoroutine = null;
        }
        if (_devilSadWoowCoroutine != null) // New
        {
            StopCoroutine(_devilSadWoowCoroutine);
            _devilSadWoowCoroutine = null;
        }

        if (_evilTalkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilTalkBlendShapeIndex, 0f);
        if (_devilEyeBlinkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, 0f);
        if (_evilSmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSmileBlendShapeIndex, 0f);
        if (_evilAngrySmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngrySmileBlendShapeIndex, 0f); // New
        if (_evilAngryBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngryBlendShapeIndex, 0f); // New
        if (_evilSadWoowBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSadWoowBlendShapeIndex, 0f); // New
    }

    private void StopDevilContinuousAnimationsExcept(Coroutine exceptionCoroutine)
    {
        if (_devilTalkingCoroutine != null && _devilTalkingCoroutine != exceptionCoroutine)
        {
            StopCoroutine(_devilTalkingCoroutine);
            _devilTalkingCoroutine = null;
            if (_evilTalkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilTalkBlendShapeIndex, devilMinTalkWeight);
        }
        if (_devilBlinkingCoroutine != null && _devilBlinkingCoroutine != exceptionCoroutine)
        {
            StopCoroutine(_devilBlinkingCoroutine);
            _devilBlinkingCoroutine = null;
            if (_devilEyeBlinkBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_devilEyeBlinkBlendShapeIndex, devilBlinkOpenWeight);
        }
        if (_devilEvilSmileCoroutine != null && _devilEvilSmileCoroutine != exceptionCoroutine)
        {
            StopCoroutine(_devilEvilSmileCoroutine);
            _devilEvilSmileCoroutine = null;
            if (_evilSmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSmileBlendShapeIndex, devilMinEvilSmileWeight);
        }
        if (_devilEvilAngrySmileCoroutine != null && _devilEvilAngrySmileCoroutine != exceptionCoroutine) // New
        {
            StopCoroutine(_devilEvilAngrySmileCoroutine);
            _devilEvilAngrySmileCoroutine = null;
            if (_evilAngrySmileBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngrySmileBlendShapeIndex, devilMinEvilAngrySmileWeight);
        }
        if (_devilAngryCoroutine != null && _devilAngryCoroutine != exceptionCoroutine) // New
        {
            StopCoroutine(_devilAngryCoroutine);
            _devilAngryCoroutine = null;
            if (_evilAngryBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilAngryBlendShapeIndex, devilMinAngryWeight);
        }
        if (_devilSadWoowCoroutine != null && _devilSadWoowCoroutine != exceptionCoroutine) // New
        {
            StopCoroutine(_devilSadWoowCoroutine);
            _devilSadWoowCoroutine = null;
            if (_evilSadWoowBlendShapeIndex != -1) skinnedMeshRenderer.SetBlendShapeWeight(_evilSadWoowBlendShapeIndex, devilMinSadWoowWeight);
        }
    }


    private void StopNPCAnimations()
    {
        if (_npcTalkingCoroutine != null)
        {
            StopCoroutine(_npcTalkingCoroutine);
            _npcTalkingCoroutine = null;
        }
        if (_npcBlinkingCoroutine != null)
        {
            StopCoroutine(_npcBlinkingCoroutine);
            _npcBlinkingCoroutine = null;
        }
        if (!isDevilExpression)
        {
            _runtimeMouthMaterial.mainTexture = _originalMouthTexture;
            _runtimeEyesMaterial.mainTexture = _originalEyesTexture;
        }
    }

    public void SetEyesMaterial(Material newEyesMaterial)
    {
        if (isDevilExpression)
        {
            Debug.LogWarning("Attempted to set Eyes Material for Devil character. Material changes are disabled for Devils.", this);
            return;
        }
        if (skinnedMeshRenderer != null && newEyesMaterial != null && skinnedMeshRenderer.materials.Length > 0)
        {
            Material[] mats = skinnedMeshRenderer.materials;
            mats[0] = newEyesMaterial;
            skinnedMeshRenderer.materials = mats;
            this.eyesMaterial = newEyesMaterial;
        }
    }

    public void SetNoseMaterial(Material newNoseMaterial)
    {
        if (isDevilExpression)
        {
            Debug.LogWarning("Attempted to set Nose Material for Devil character. Material changes are disabled for Devils.", this);
            return;
        }
        if (skinnedMeshRenderer != null && newNoseMaterial != null && skinnedMeshRenderer.materials.Length > 2)
        {
            Material[] mats = skinnedMeshRenderer.materials;
            mats[2] = newNoseMaterial;
            skinnedMeshRenderer.materials = mats;
            this.noseMaterial = newNoseMaterial;
        }
    }

    public void SetMouthMaterial(Material newMouthMaterial)
    {
        if (isDevilExpression)
        {
            Debug.LogWarning("Attempted to set Mouth Material for Devil character. Material changes are disabled for Devils.", this);
            return;
        }
        if (skinnedMeshRenderer != null && newMouthMaterial != null && skinnedMeshRenderer.materials.Length > 1)
        {
            Material[] mats = skinnedMeshRenderer.materials;
            mats[1] = newMouthMaterial;
            skinnedMeshRenderer.materials = mats;
            this.mouthMaterial = newMouthMaterial;
        }
    }
}