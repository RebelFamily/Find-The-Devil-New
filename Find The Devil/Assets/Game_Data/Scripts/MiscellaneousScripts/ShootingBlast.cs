using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterFX
{
    // [System.Serializable]
    // public class BulletData
    // {
    //     public ParticleSystem Muzzle;
    //     public ParticleSystem Bullet;
    //     public GameObject DefaultTrails;
    //     public ParticleSystem HitEffect;
    //     public Texture2D MappingTexture;
    //     public float BulletSpeed = 100f;
    // }

    public class ShootingBlast : MonoBehaviour
    {
        public ParticleSystem Muzzle => bulletDatas[currentBulletIndex].Muzzle;
        public ParticleSystem Bullet => bulletDatas[currentBulletIndex].Bullet;

        public GameObject DefaultTrails => bulletDatas[currentBulletIndex].DefaultTrails;
        public ParticleSystem HitEffect => bulletDatas[currentBulletIndex].HitEffect;
        public float BulletSpeed = 100f;

        public Transform BulletStartPoint;

        [SerializeField]
        public Texture2D MappingTexture => bulletDatas[currentBulletIndex].MappingTexture;

        public List<BulletData> bulletDatas = new List<BulletData>();

        public int currentBulletIndex = 0;

        public List<Material> materials = new List<Material>();

        public bool ModifyMaterials = false;

        void OnDestroy()
        {
            foreach (Material material in materials)
            {
                Destroy(material);
            }
            materials = new List<Material>();
        }

        // --- CHANGED: Now takes a target position directly ---
        public void ShootBullet(Vector3 targetPosition)
        {
            StartCoroutine(ShootBulletCoroutine(targetPosition));
        }

        // --- CHANGED: Coroutine now accepts a targetPosition parameter ---
        public IEnumerator ShootBulletCoroutine(Vector3 targetPosition)
        {
            // --- REMOVED: All raycast and mouse input logic is now gone ---
            // The target position is received as a parameter from the calling script.
            
            BulletStartPoint.LookAt(targetPosition);
            
            // This offset logic can stay here as it's part of the visual effect
            targetPosition -= 0.5f * (targetPosition - BulletStartPoint.position).normalized;

            var muzzle = Instantiate(Muzzle, BulletStartPoint.position, BulletStartPoint.rotation);
            var bullet = Instantiate(Bullet, BulletStartPoint.position, BulletStartPoint.rotation);

            GameObject Trail = null;
            if (DefaultTrails != null)
            {
                Trail = Instantiate(DefaultTrails, BulletStartPoint.position, BulletStartPoint.rotation);
                SetRampTexture(Trail.gameObject);
            }
            SetRampTexture(bullet.gameObject);
            SetRampTexture(muzzle.gameObject);
            
            bullet.transform.parent = transform;
            
            var time = Vector3.Distance(BulletStartPoint.position, targetPosition) / BulletSpeed;
            var timer = 0f;
            while (timer < time)
            {
                timer += Time.deltaTime;
                bullet.transform.position += bullet.transform.forward * BulletSpeed * Time.deltaTime;
                if (Trail != null)
                {
                    Trail.transform.position = bullet.transform.position;
                }
                yield return null;
            }

            Destroy(bullet.gameObject);
            if (Trail != null)
            {
                Destroy(Trail);
            }

            var hitEffect = Instantiate(HitEffect, targetPosition, bullet.transform.rotation); // Use the final target position for the impact
            SetRampTexture(hitEffect.gameObject);

            // Clean up instantiated effects
            StartCoroutine(DestroyAfterParticlesFinish(muzzle.gameObject));
            StartCoroutine(DestroyAfterParticlesFinish(hitEffect.gameObject));
        }

        private IEnumerator DestroyAfterParticlesFinish(GameObject objToDestroy)
        {
            ParticleSystem[] particleSystems = objToDestroy.GetComponentsInChildren<ParticleSystem>();
            if (particleSystems.Length > 0)
            {
                foreach (var ps in particleSystems)
                {
                    yield return new WaitUntil(() => ps.isStopped);
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (objToDestroy != null)
            {
                Destroy(objToDestroy);
            }
        }

        public void SetRampTexture(GameObject target)
        {
            if (MappingTexture)
            {
                ParticleSystemRenderer renderer = target.GetComponent<ParticleSystemRenderer>();
                if (renderer)
                {
                    MUtils.MApplyLutTexturesToParticles(target.GetComponent<ParticleSystem>(), MappingTexture);
                }

                ParticleSystem[] systems = target.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem system in systems)
                {
                    MUtils.MApplyLutTexturesToParticles(system, MappingTexture);
                }

                TrailRenderer[] trails = target.GetComponentsInChildren<TrailRenderer>();
                foreach (TrailRenderer trail in trails)
                {
                    ApplyGradientToTrails(new TrailRenderer[] { trail });
                }
            }
        }

        void ApplyGradientToTrails(TrailRenderer[] trails)
        {
            if (MappingTexture == null)
            {
                Debug.LogError("Gradient texture is not assigned!");
                return;
            }

            Gradient trailGradient = new Gradient();

            var step = 64;
            int colorCount = (MappingTexture.width / step) + 1;

            GradientColorKey[] colorKeys = new GradientColorKey[colorCount];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorCount];

            for (int i = 0; i < colorCount; i += 1)
            {
                Color color = MappingTexture.GetPixel(i * step, 0);
                var alpha = MappingTexture.GetPixel(i * step, 0).a;
                float time = i / (float)(colorCount - 1);
                colorKeys[i] = new GradientColorKey(color, time);
                alphaKeys[i] = new GradientAlphaKey(alpha, time);
            }

            trailGradient.colorKeys = colorKeys;
            trailGradient.alphaKeys = alphaKeys;

            foreach (TrailRenderer trail in trails)
            {
                trail.colorGradient = trailGradient;
            }
        }
    }
}