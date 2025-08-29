using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BuildingSmokeHandler : MonoBehaviour
{
    [SerializeField] private float _throwDistance = 5f;
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _animationDuration = 1.0f;
    [SerializeField] private float _randomRotationMultiplier = 360f;

    [SerializeField] private RuntimeAnimatorController _slapedAwayAnimatorController;
    [SerializeField] private Transform _newParentTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MetaSmoke"))
        {
            other.gameObject.SetActive(false);
            return;
        }

        if (other.CompareTag("Devil"))
        {
            other.GetComponent<Animator>().runtimeAnimatorController = _slapedAwayAnimatorController;
            Transform devilTransform = other.transform;
            
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.MetaEnemyKill);
            devilTransform.parent = _newParentTransform;

            devilTransform.DOKill();
            
            Vector3 throwDirection = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                1f,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized; 

            
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Devil_Weeeee);
            Vector3 targetPosition = devilTransform.position + throwDirection * _throwDistance;

            devilTransform.DOJump(targetPosition, _jumpHeight, 1, _animationDuration).SetEase(Ease.OutSine);

            Vector3 randomRotation = UnityEngine.Random.insideUnitSphere * _randomRotationMultiplier;
            devilTransform.DORotate(devilTransform.rotation.eulerAngles + randomRotation, _animationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Destroy(other.gameObject);
                });
            return;
        }

        if (other.CompareTag("KingDevil"))
        {
            other.GetComponent<Animator>().runtimeAnimatorController = _slapedAwayAnimatorController;
            if (GameManager.Instance == null || GameManager.Instance.playerController == null || GameManager.Instance.playerController.enemyLookAt == null)
            {
                Debug.LogWarning("KingDevil target is not available. Make sure GameManager, PlayerController, and enemyLookAt are all properly initialized.", this);
                return;
            }
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.MetaEnemyKill);
            Transform kingDevilTransform = other.transform; 
            
            kingDevilTransform.parent = _newParentTransform;

            kingDevilTransform.DOKill();
            
            Sequence kingDevilSequence = DOTween.Sequence();

            kingDevilSequence.Join(kingDevilTransform.DOJump(GameManager.Instance.playerController.scannerSwitchPos.position, 5, 1, 1.5f).SetEase(Ease.OutSine));
            
            kingDevilSequence.Join(kingDevilTransform.DOLookAt(GameManager.Instance.playerController.scannerSwitchPos.position, 1.5f, AxisConstraint.None));
            GameManager.Instance.audioManager.PlaySFX(AudioManager.GameSound.Devil_Weeeee);
            Vector3 yAxisRotation = new Vector3(0, 0, UnityEngine.Random.Range(3, 5) * 45f); 
            kingDevilSequence.Join(kingDevilTransform.DORotate(yAxisRotation, 1.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            
            kingDevilSequence.OnComplete(() =>
            {
                Destroy(other.gameObject);
            });
        }
    }
}