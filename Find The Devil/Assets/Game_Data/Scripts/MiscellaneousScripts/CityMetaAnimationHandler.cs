using UnityEngine;
using DG.Tweening; 
using Dreamteck.Splines; 
using System.Collections;
using System.Collections.Generic;

public class CityMetaAnimationHandler : MonoBehaviour
{
    [Header("Devil Pawn Settings")]
    [SerializeField] private GameObject pawnDevil;
    [SerializeField] private SplineFollower _splineFollower; 
    [SerializeField] private float initialJumpHeight = 2f;
    [SerializeField] private float initialJumpDuration = 0.5f;
    [SerializeField] private float continuousJumpHeight = 0.5f;
    [SerializeField] private float continuousJumpDuration = 0.3f;
    [SerializeField] private float splineFollowSpeed = 0.25f;

    [Header("Particle Effects")]
    [SerializeField] private List<ParticleSystem> _jumpParticles;
    [SerializeField] private float _particleSpawnInterval = 0.5f;

    private Tween _continuousJumpTween;
    private Coroutine _particleCoroutine;

    private const string SplinePawnPositionKey = "PawnSplinePosition";

    private void OnApplicationQuit()
    {
        SavePawnSplinePosition();
    }
    
    private void OnDisable()
    {
        SavePawnSplinePosition();
    }

    public void MovePawnDevil()
    {
        if (pawnDevil == null)
        {
            Debug.LogWarning("pawnDevil GameObject is not assigned. Cannot animate.");
            return;
        }
        if (_splineFollower == null)
        {
            Debug.LogWarning("SplineFollower is not assigned. Cannot animate pawn devil movement along spline.");
            return;
        }
        if (_splineFollower.spline == null)
        {
            Debug.LogWarning("SplineFollower has no spline assigned. Cannot animate pawn devil movement.");
            return;
        }

        pawnDevil.SetActive(true);

        pawnDevil.transform.DOKill(); 
        if (_continuousJumpTween != null && _continuousJumpTween.IsActive())
        {
            _continuousJumpTween.Kill();
        }
        _splineFollower.follow = false;
        _splineFollower.followSpeed = 0;

        double savedPercent = (double)PlayerPrefs.GetFloat(SplinePawnPositionKey, 0f);
        _splineFollower.SetPercent(savedPercent);
        
        Sequence initialAnimationSequence = DOTween.Sequence();
        initialAnimationSequence.Append(
            pawnDevil.transform.DOLocalJump(pawnDevil.transform.localPosition, initialJumpHeight, 1, initialJumpDuration)
                .SetEase(Ease.OutQuad)
        );
        
        float currentYAngleForZero = pawnDevil.transform.localEulerAngles.y;
        float relativeYChangeForZero = 0f - currentYAngleForZero;

        initialAnimationSequence.Append(
            pawnDevil.transform.DOLocalRotate(new Vector3(0f, relativeYChangeForZero, 0f), initialJumpDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutQuad)
        );
        
        initialAnimationSequence.OnComplete(() =>
        {
            _splineFollower.followSpeed = splineFollowSpeed;
            _splineFollower.follow = true;

            StartContinuousJumping();
            // Start the particle effect only after the initial jump is complete
            //_particleCoroutine = StartCoroutine(SpawnParticlesContinuously());
        });

        initialAnimationSequence.Play();
    }

    private void StartContinuousJumping()
    {
        if (_continuousJumpTween != null && _continuousJumpTween.IsActive())
        {
            _continuousJumpTween.Kill();
        }
        
        _continuousJumpTween = pawnDevil.transform.DOLocalJump(Vector3.zero, continuousJumpHeight, 1, continuousJumpDuration)
            .SetEase(Ease.OutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(pawnDevil.gameObject);
    }
    
    public void MovePawnDevilToNextPointAndJump(double startPercent, double endPercent)
    {
        if (pawnDevil == null || _splineFollower == null || _splineFollower.spline == null)
        {
            Debug.LogWarning("Pawn or SplineFollower not assigned. Cannot perform animation.");
            return;
        }

        if (_continuousJumpTween != null && _continuousJumpTween.IsActive())
        {
            _continuousJumpTween.Kill();
        }

        // Stop the particle coroutine before starting the new movement
        if (_particleCoroutine != null)
        {
            StopCoroutine(_particleCoroutine);
        }
        
        _splineFollower.follow = false;
  
        Sequence moveAndJumpSequence = DOTween.Sequence();

        Vector3 endPosition = (Vector3)_splineFollower.spline.EvaluatePosition(endPercent);
        
        moveAndJumpSequence.Append(pawnDevil.transform.DOMove(endPosition, 1f).SetEase(Ease.Linear));
 
        moveAndJumpSequence.OnComplete(() =>
        {
            StartContinuousJumping();
            // Restart the particle coroutine after the move is complete
            //_particleCoroutine = StartCoroutine(SpawnParticlesContinuously());
            _splineFollower.SetPercent(endPercent);
            _splineFollower.follow = true;
            _splineFollower.followSpeed = splineFollowSpeed;
            
            SavePawnSplinePosition();
        });

        moveAndJumpSequence.Play();
    }

    public void StopPawnDevilAnimation()
    {
        if (pawnDevil != null)
        {
            pawnDevil.transform.DOKill();
            
            
            // float currentYAngleForNinety = pawnDevil.transform.localEulerAngles.y;
            // float relativeYChangeForNinety = 90f - currentYAngleForNinety;
            //
            // pawnDevil.transform.DOLocalRotate(new Vector3(0f, relativeYChangeForNinety, 0f), 0.2f, RotateMode.LocalAxisAdd)
            //     .SetEase(Ease.OutQuad);
            // pawnDevil.GetComponent<Animator>().enabled = true;
            
            // Calculate the camera's position
            Vector3 cameraPosition = Camera.main.transform.position;

            // Make the pawnDevil look at the camera's X and Z position (ignoring height)
            // We use DOLookAt for a smooth rotation towards the camera.
            pawnDevil.transform.DOLookAt(
                new Vector3(cameraPosition.x, pawnDevil.transform.position.y, cameraPosition.z), 
                0.2f, 
                AxisConstraint.Y, // Only rotate around the Y (up) axis
                Vector3.up
            ).SetEase(Ease.OutQuad);

            pawnDevil.GetComponent<Animator>().enabled = true;
            
        }

        if (_splineFollower != null)
        {
            _splineFollower.follow = false;
            _splineFollower.followSpeed = 0;
        }

        if (_continuousJumpTween != null && _continuousJumpTween.IsActive())
        {
            _continuousJumpTween.Kill();
        }
        
        _particleCoroutine = StartCoroutine(SpawnParticlesContinuously());

        // // Make sure to stop the particle coroutine here
        // if (_particleCoroutine != null)
        // {
        //     
        //     StopCoroutine(_particleCoroutine);
        // }
        
        
        
        if (_jumpParticles != null)
        {
            foreach (var particle in _jumpParticles)
            {
                if (particle != null)
                {
                    particle.Stop();
                    particle.gameObject.SetActive(false);
                }
            }
        }
    }
    
    public void SavePawnSplinePosition()
    {
        if (_splineFollower != null)
        {
            PlayerPrefs.SetFloat(SplinePawnPositionKey, (float)_splineFollower.GetPercent());
            PlayerPrefs.Save();
            Debug.Log($"Pawn position saved at: {_splineFollower.GetPercent() * 100}%");
        }
    }

    private IEnumerator SpawnParticlesContinuously()
    {
        if (_jumpParticles == null || _jumpParticles.Count == 0)
        {
            Debug.LogWarning("No particle systems assigned to _jumpParticles list.");
            yield break;
        }

        while (true)
        {
            foreach (var particle in _jumpParticles)
            {
                if (particle != null)
                {
                    if (!particle.gameObject.activeSelf)
                    {
                        particle.gameObject.SetActive(true);
                    }
                    
                    if (!particle.isPlaying)
                    {
                        particle.Play();
                    }
                }
            }
            yield return new WaitForSeconds(_particleSpawnInterval);
        }
    }
}