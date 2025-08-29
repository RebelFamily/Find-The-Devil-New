namespace ITHappy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;
    using System.Linq;

    [Serializable]
    public struct LevelRotationSettings
    {
        public int levelNumber;
        public Vector3 targetYRotation;
        
        // --- NEW: Added new rotation target for City Meta. ---
        public Vector3 citymetarotation;
        
        public float rotationDuration;
        public bool rotateRelative;
        public Ease easeType;
    }

    public class RotationAnimator : MonoBehaviour
    {
        public GameObject targetObject;

        [SerializeField]
        private List<LevelRotationSettings> m_LevelRotations = new List<LevelRotationSettings>();

        private Quaternion _originalRotation;

        void Start()
        {
            if (targetObject == null)
            {
                targetObject = this.gameObject;
            }

            if (targetObject != null)
            {
                _originalRotation = targetObject.transform.localRotation;
            }
        }
      
        public void StartRotationForLevel(int levelNumber)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("RotationAnimator: Target object is not assigned.", this);
                return;
            }

            LevelRotationSettings rotationSettings = m_LevelRotations.FirstOrDefault(rs => rs.levelNumber == levelNumber);

            if (rotationSettings.targetYRotation == Vector3.zero && rotationSettings.rotationDuration == 0)
            {
                Debug.LogWarning($"RotationAnimator: No primary rotation settings found for level {levelNumber}. Skipping animation.", this);
                return;
            }
            
            // Call the new general function to perform the rotation
            RotateToTarget(
                rotationSettings.targetYRotation,
                rotationSettings.rotationDuration,
                rotationSettings.easeType,
                rotationSettings.rotateRelative);
        }
        
       
        public void StartCityMetaRotationForLevel(int levelNumber)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("RotationAnimator: Target object is not assigned.", this);
                return;
            }

            LevelRotationSettings rotationSettings = m_LevelRotations.FirstOrDefault(rs => rs.levelNumber == levelNumber);

            if (rotationSettings.citymetarotation == Vector3.zero && rotationSettings.rotationDuration == 0)
            {
                Debug.LogWarning($"RotationAnimator: No City Meta rotation settings found for level {levelNumber}. Skipping animation.", this);
                return;
            }
            
            // Call the new general function to perform the rotation
            RotateToTarget(
                rotationSettings.citymetarotation,
                rotationSettings.rotationDuration,
                rotationSettings.easeType,
                rotationSettings.rotateRelative);
        }
        
        /// <summary>
        /// Rotates the target object to a specified rotation with custom parameters.
        /// </summary>
        /// <param name="targetRotation">The target Euler angles for the rotation.</param>
        /// <param name="duration">The duration of the rotation animation.</param>
        /// <param name="easeType">The ease type for the rotation animation.</param>
        /// <param name="isRelative">If true, the rotation will be relative to the current rotation. Otherwise, it will be an absolute rotation.</param>
        public void RotateToTarget(Vector3 targetRotation, float duration, Ease easeType = Ease.InOutQuad, bool isRelative = false)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("RotationAnimator: Target object is not assigned. Cannot perform rotation.", this);
                return;
            }
            
            targetObject.transform.DOKill(true);

            Vector3 finalRotationTarget = targetRotation;

            if (!isRelative)
            {
                Vector3 currentRotation = targetObject.transform.localEulerAngles;
                finalRotationTarget = new Vector3(currentRotation.x, targetRotation.y, currentRotation.z);
            }

            Debug.Log($"RotationAnimator: Starting rotation. Target: {finalRotationTarget}, Duration: {duration}, Relative: {isRelative}");

            Tween rotationTween = targetObject.transform.DORotate(finalRotationTarget, duration, RotateMode.Fast)
                .SetEase(easeType);

            if (isRelative)
            {
                rotationTween.SetRelative(true);
            }
        
            rotationTween.OnComplete(() => {
                Debug.Log("RotationAnimator: Rotation to target complete.");
            });
        }
        
        public void ResetRotationToOriginalRotation(float time)
        {
            if (targetObject == null)
            {
                return;
            }
            
            targetObject.transform.DOKill(true);
            
            targetObject.transform.DOLocalRotate(_originalRotation.eulerAngles, time)
                .SetEase(m_LevelRotations.FirstOrDefault().easeType) // Using the ease type from the first entry as a default
                .OnComplete(() => {
                    Debug.Log("RotationAnimator: Reset to original rotation complete.");
                });
        }
        
        
        
    }
}