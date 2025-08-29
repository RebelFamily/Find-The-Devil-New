using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class CoinAnimator : MonoBehaviour
{
    [Header("Coin Animation Setup")]
    public GameObject coinPrefab;
    public Transform startTransform;
    public Transform targetTransform;

    [Header("Coin Animation Settings")]
    public int numberOfCoins = 10;
    public float animationDuration = 1.0f;
    public float spreadRadius = 0.5f;
    public float rotationSpeed = 360f;

    [Header("Coin Arc Motion Settings")]
    public float jumpPower = 1.0f;

    [Header("Coin Animation Events")]
    public UnityEvent onAllCoinsArrived;

    private int _coinsToAnimate;
    private int _coinsArrivedCount;
    private Vector3 _coinSizeOverride = Vector3.zero;

    [Header("Garbage Animation Settings")]
    public bool animateGarbageOnStart = false; 
    public bool loopGarbageAnimation = false;
    public GameObject[] garbagePrefabs;
    public Transform garbageStartTransform;
    public Transform garbageTargetTransform;
    public int numberOfGarbageItems = 3;
    public float garbageAnimationDuration = 0.8f;
    public float garbageJumpPower = 3.0f;
    public float garbageSpreadRadius = 1.0f;
    public float garbageRotationSpeed = 720f;
    public float garbageSpawnDelay = 0.1f;

    [Header("Garbage Animation Events")]
    public UnityEvent onAllGarbageThrown;

    private int _garbageToAnimate;
    private int _garbageThrownCount;
    private Coroutine _garbageAnimationCoroutine;
    

    private void Start()
    {
        if (animateGarbageOnStart)
        {
            AnimateGarbage();
        }
    }
    
    public void CoinSize(float size)
    {
        _coinSizeOverride = new Vector3(size, size, size);
    }
    
    public void AnimateCoins()
    {
        if (coinPrefab == null || startTransform == null || targetTransform == null)
        {
            Debug.LogWarning("CoinAnimator: Coin animation is missing required references. Please assign all prefabs and transforms.");
            return;
        }

        _coinsArrivedCount = 0;
        _coinsToAnimate = numberOfCoins;
 
        if (_coinsToAnimate <= 0)
        {
            onAllCoinsArrived.Invoke();
            return;
        }

        for (int i = 0; i < _coinsToAnimate; i++)
        {
            Vector2 randomCirclePoint = Random.insideUnitCircle * spreadRadius;
            Vector3 spawnPosition = new Vector3(
                startTransform.position.x + randomCirclePoint.x,
                startTransform.position.y,
                startTransform.position.z + randomCirclePoint.y
            );

            GameObject coinInstance = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

            if (_coinSizeOverride != Vector3.zero)
            {
                coinInstance.transform.localScale = _coinSizeOverride;
            }
            
            Sequence coinSequence = DOTween.Sequence();
            
            coinSequence.Append(coinInstance.transform.DOScale(coinInstance.transform.localScale, 0.2f).From(Vector3.zero).SetEase(Ease.OutBack));

            coinSequence.Join(coinInstance.transform.DOJump(targetTransform.position, jumpPower, 1, animationDuration)
                .SetEase(Ease.Linear));

            Vector3 randomRotation = new Vector3(
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed
            );
            coinSequence.Join(coinInstance.transform.DORotate(randomRotation, animationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));
            
            coinSequence.OnComplete(() =>
            {
                Destroy(coinInstance);
                _coinsArrivedCount++;
                
                if (_coinsArrivedCount >= _coinsToAnimate)
                {
                    onAllCoinsArrived.Invoke();
                }
            });
        }
    }

    public void AnimateCoinsWithBlast()
    {
        if (coinPrefab == null || startTransform == null || targetTransform == null)
        {
            Debug.LogWarning("CoinAnimator: AnimateCoinsWithBlast is missing required references. Please assign all prefabs and transforms.");
            return;
        }

        _coinsArrivedCount = 0;
        _coinsToAnimate = numberOfCoins;

        if (_coinsToAnimate <= 0)
        {
            onAllCoinsArrived.Invoke();
            return;
        }

        StartCoroutine(AnimateCoinsRoutine());
    }
    public void AnimateCoinsDevilDeath()
    {
        if (coinPrefab == null || startTransform == null || targetTransform == null)
        {
            Debug.LogWarning("CoinAnimator: AnimateCoinsWithBlast is missing required references. Please assign all prefabs and transforms.");
            return;
        }

        _coinsArrivedCount = 0;
        _coinsToAnimate = numberOfCoins;

        if (_coinsToAnimate <= 0)
        {
            onAllCoinsArrived.Invoke();
            return;
        }

        StartCoroutine(AnimateDevilDeathCoinsRoutine());
    }

    private IEnumerator AnimateCoinsRoutine()
    {
        float blastDuration = animationDuration * 0.5f;
        float travelDuration = animationDuration * 0.5f;
        float spawnDelay = 0.025f; // Delay between each coin spawn
    
        
        for (int i = 0; i < _coinsToAnimate; i++)
        {
            GameObject coinInstance = Instantiate(coinPrefab, startTransform.position, Quaternion.identity);
    
            
            coinInstance.transform.localScale = _coinSizeOverride ;
         
    
            Vector3 blastEndPosition = startTransform.position + Random.insideUnitSphere * spreadRadius;
    
            Sequence coinSequence = DOTween.Sequence();
    
            // First phase: Blast out to a random position
            coinSequence.Append(coinInstance.transform.DOScale(coinInstance.transform.localScale, 0.2f).From(Vector3.zero).SetEase(Ease.OutBack));
            coinSequence.Join(coinInstance.transform.DOJump(blastEndPosition, jumpPower, 1, blastDuration).SetEase(Ease.OutQuad));
    
            // Second phase: Animate from the blasted position to the target
            coinSequence.Append(coinInstance.transform.DOMove(targetTransform.position, travelDuration).SetEase(Ease.InSine));
    
            // Continuous rotation throughout the whole animation
            Vector3 randomRotation = new Vector3(
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed
            );
            coinSequence.Join(coinInstance.transform.DORotate(randomRotation, animationDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            
            coinSequence.OnComplete(() =>
            {
                Destroy(coinInstance);
                _coinsArrivedCount++;
                
                if (_coinsArrivedCount >= _coinsToAnimate)
                {
                    onAllCoinsArrived.Invoke();
                }
            });
    
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private IEnumerator AnimateDevilDeathCoinsRoutine()
    {
        float blastDuration = animationDuration * 0.5f;
        float travelDuration = animationDuration * 0.5f;
        float spawnDelay = 0.02f;
        GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Meta_CoinsSpend);
        for (int i = 0; i < _coinsToAnimate; i++)
        {
           
            GameObject coinInstance = Instantiate(coinPrefab, startTransform.position, Quaternion.identity);
    
            coinInstance.transform.localScale = _coinSizeOverride;
    
            Vector3 blastEndPosition = startTransform.position + Random.insideUnitSphere * spreadRadius;

            Sequence coinSequence = DOTween.Sequence();
            
            coinSequence.Append(coinInstance.transform.DOScale(coinInstance.transform.localScale, 0.2f).From(Vector3.zero).SetEase(Ease.OutBack));
            coinSequence.Join(coinInstance.transform.DOJump(blastEndPosition, jumpPower, 1, blastDuration).SetEase(Ease.OutQuad));
            
            coinSequence.Append(coinInstance.transform.DOMove(targetTransform.position, travelDuration).SetEase(Ease.InSine));
            coinSequence.Join(coinInstance.transform.DOScale(0.01f, travelDuration).SetEase(Ease.InBack));


            Vector3 randomRotation = new Vector3(
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed,
                Random.Range(1, 5) * rotationSpeed
            );
            coinSequence.Join(coinInstance.transform.DORotate(randomRotation, animationDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));

            coinSequence.OnComplete(() =>
            {
                Destroy(coinInstance);
            });
 
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void AnimateGarbage()
    {
        if (garbagePrefabs == null || garbagePrefabs.Length == 0 || garbageStartTransform == null || garbageTargetTransform == null)
        {
            Debug.LogWarning("CoinAnimator: Garbage animation is missing required references or prefabs. Please assign garbagePrefabs, garbageStartTransform, and garbageTargetTransform.");
            if (!loopGarbageAnimation) onAllGarbageThrown.Invoke(); 
            return;
        }

        _garbageThrownCount = 0; 
        _garbageToAnimate = numberOfGarbageItems;

        if (_garbageToAnimate <= 0 && !loopGarbageAnimation)
        {
            onAllGarbageThrown.Invoke();
            return;
        }
        else if (_garbageToAnimate <= 0 && loopGarbageAnimation)
        {
             Debug.LogWarning("CoinAnimator: numberOfGarbageItems is 0 but loopGarbageAnimation is true. No garbage will be animated.");
             return;
        }

        if (_garbageAnimationCoroutine != null)
        {
            StopCoroutine(_garbageAnimationCoroutine);
        }

        _garbageAnimationCoroutine = StartCoroutine(SpawnAndAnimateGarbageRoutine());
    }

    private IEnumerator SpawnAndAnimateGarbageRoutine()
    {
        do
        {
            for (int i = 0; i < numberOfGarbageItems; i++)
            {
                GameObject randomGarbagePrefab = garbagePrefabs[Random.Range(0, garbagePrefabs.Length)];
                
                GameObject garbageInstance = Instantiate(randomGarbagePrefab, garbageStartTransform.position, Quaternion.identity);
                garbageInstance.transform.localScale = Vector3.zero;

                Vector3 burstStartPos = garbageStartTransform.position + Random.insideUnitSphere * garbageSpreadRadius;

                Sequence garbageSequence = DOTween.Sequence();

                garbageSequence.Append(garbageInstance.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

                garbageSequence.Join(garbageInstance.transform.DOJump(garbageTargetTransform.position, garbageJumpPower, 1, garbageAnimationDuration)
                    .SetEase(Ease.InQuad));

                Vector3 randomRotation = new Vector3(
                    Random.Range(1, 5) * garbageRotationSpeed,
                    Random.Range(1, 5) * garbageRotationSpeed,
                    Random.Range(1, 5) * garbageRotationSpeed
                );
                garbageSequence.Join(garbageInstance.transform.DORotate(randomRotation, garbageAnimationDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear));
                
                garbageSequence.OnComplete(() =>
                {
                    Destroy(garbageInstance);
                    if (!loopGarbageAnimation) 
                    {
                        _garbageThrownCount++;
                        if (_garbageThrownCount >= numberOfGarbageItems)
                        {
                            onAllGarbageThrown.Invoke();
                            _garbageAnimationCoroutine = null;
                        }
                    }
                });

                yield return new WaitForSeconds(garbageSpawnDelay);
            }

            if (loopGarbageAnimation)
            {
                _garbageThrownCount = 0;
                yield return null;
            }

        } while (loopGarbageAnimation);

        _garbageAnimationCoroutine = null;
    }

    private void OnDisable()
    {
        
        if (_garbageAnimationCoroutine != null)
        {
            StopCoroutine(_garbageAnimationCoroutine);
            _garbageAnimationCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }
}