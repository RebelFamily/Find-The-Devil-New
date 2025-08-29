using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PilotoStudio
{
    public class BeamEmitter : MonoBehaviour
    {
        [Header("Core Beam Settings")]
        [SerializeField]
        private List<LineRenderer> beams = new List<LineRenderer>();
        [SerializeField]
        private List<ParticleSystem> beamSystems = new List<ParticleSystem>();
        
        [Space]
        [SerializeField]
        public float beamLifetime;
        [SerializeField]
        private float beamFormationTime;

        [Header("Dynamic Beam Settings")]
        [SerializeField]
        private float _beamWidth = 0.1f;
        [SerializeField]
        private float _particleRate = 100f;

        [Header("Target & Impact")]
        [SerializeField]
        public Transform beamTarget;
        [SerializeField]
        public GameObject beamTargetHitFX;

        #region Initialization

        private void GetChildLineRenderers()
        {
            beams.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out LineRenderer _lineRenderer))
                {
                    beams.Add(_lineRenderer);
                }
            }
        }

        private void GetChildBeamEmitters()
        {
            beamSystems.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out ParticleSystem _ps))
                {
                    var sh = _ps.shape;
                    if (sh.shapeType == ParticleSystemShapeType.SingleSidedEdge)
                    {
                        beamSystems.Add(_ps);
                    }
                }
            }
        }

        private void AssignBeamThickness()
        {
            foreach (LineRenderer _line in beams)
            {
                _line.widthMultiplier = _beamWidth;
            }
        }

        private void AssignParticleDensity()
        {
            foreach (ParticleSystem _ps in beamSystems)
            {
                var emission = _ps.emission;
                var rateOverTime = emission.rateOverTime;
                rateOverTime.constant = _particleRate;
                emission.rateOverTime = rateOverTime;
            }
        }

        private void AssignChildBeamsToArray()
        {
            GetChildLineRenderers();
            GetChildBeamEmitters();
        }

        #endregion

        #region Beam Lifecycle

        public void PlayBeam()
        {
            if (beamTarget == null)
            {
                Debug.LogError("Beam target is not assigned. Cannot play beam.");
                Destroy(gameObject);
                return;
            }

            AssignChildBeamsToArray();
            AssignBeamThickness();
            AssignParticleDensity();

            StopAllCoroutines();
            
            PlayEdgeSystems();
            PlayLineRenderers();

            if (beamLifetime == 0)
            {
                StartCoroutine(BeamStart());
            }
            else
            {
                StartCoroutine(BeamPlayComplete());
            }
        }

        private IEnumerator BeamStart()
        {
            float elapsedTime = 0f;

            while (elapsedTime <= beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = Mathf.Lerp(0, _beamWidth, elapsedTime / beamFormationTime);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            for (int i = 0; i < beams.Count; i++)
            {
                beams[i].widthMultiplier = _beamWidth;
            }
        }

        private IEnumerator BeamPlayComplete()
        {
            float elapsedTime = 0f;
            while (elapsedTime <= beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = Mathf.Lerp(0, _beamWidth, elapsedTime / beamFormationTime);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            for (int i = 0; i < beams.Count; i++)
            {
                beams[i].widthMultiplier = _beamWidth;
            }

            yield return new WaitForSeconds(beamLifetime);

            float dissipationTime = 0f;
            while (dissipationTime <= beamFormationTime)
            {
                for (int i = 0; i < beams.Count; i++)
                {
                    beams[i].widthMultiplier = Mathf.Lerp(_beamWidth, 0, dissipationTime / beamFormationTime);
                }
                dissipationTime += Time.deltaTime;
                yield return null;
            }
            for (int i = 0; i < beams.Count; i++)
            {
                beams[i].widthMultiplier = 0;
            }

            Destroy(gameObject);
        }

        #endregion

        #region Continuous Updates

        private void Update()
        {
            if (beamTarget == null) return;
            
            PlayEdgeSystems();
            PlayLineRenderers();
            UpdateParticleDensity();
            UpdateImpactFX();
        }

        private void UpdateParticleDensity()
        {
            float distance = Vector3.Distance(this.transform.position, beamTarget.position);
            distance -= 5f;

            foreach (ParticleSystem _ps in beamSystems)
            {
                var emission = _ps.emission;
                var rateOverTime = emission.rateOverTime;
                
                if (distance > 0)
                {
                    float distanceMultiplier = 1 + (distance / 5);
                    rateOverTime.constant = _particleRate * distanceMultiplier;
                }
                else
                {
                    rateOverTime.constant = _particleRate;
                }

                emission.rateOverTime = rateOverTime;
            }
        }

        private void UpdateImpactFX()
        {
            if (beamTargetHitFX == null) return;

            beamTargetHitFX.transform.position = beamTarget.position;
            beamTargetHitFX.transform.LookAt(this.transform.position);
        }
        
        private void PlayLineRenderers()
        {
            foreach (LineRenderer _line in beams)
            {
                _line.useWorldSpace = true;
                _line.SetPosition(0, _line.transform.position);
                _line.SetPosition(1, beamTarget.position);
            }
        }

        private void PlayEdgeSystems()
        {
            foreach (ParticleSystem _ps in beamSystems)
            {
                Vector3 direction = beamTarget.position - _ps.transform.position;
                
                // --- FIXED: Check for a non-zero vector before calculating rotation ---
                if (direction.sqrMagnitude > 0)
                {
                    Quaternion _lookRotation = Quaternion.LookRotation(direction);
                    _ps.gameObject.transform.rotation = _lookRotation;
                }

                var sh = _ps.shape;
                sh.rotation = new Vector3(0, 90, 0);
                float beamLength = direction.magnitude / 2;
                sh.radius = beamLength;
                sh.position = new Vector3(0, 0, beamLength);
            }
        }

        #endregion
    }
}