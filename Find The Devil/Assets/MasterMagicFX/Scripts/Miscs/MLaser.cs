using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterFX
{
    public class MLaser : MonoBehaviour
    {
        public float LaserDistance = 100f;
        public Transform LaserHead;
        public List<ParticleSystem> LaserBodies;

        public ParticleSystem Laser;

        public ParticleSystem LaserStart;
        public ParticleSystem LaserStop;
        public float HitOffset; // This variable is currently not used in the length calculation.

        // New: Size handler for the laser's width/thickness
        public float LaserWidth = 0.1f; 

        private Vector3 _targetEndPoint; // Changed from EndPos for clarity, but functions identically

        public Transform EndPos;
        void OnValidate()
        {
            UpdateLaser();
        }

        void UpdateLaser()
        {
            if (LaserHead != null)
            {
                LaserHead.localPosition = new Vector3(0, 0, LaserDistance);
            }

            if (LaserBodies != null)
            {
                foreach (var laserBody in LaserBodies)
                {
                    if (laserBody == null)
                    {
                        continue;
                    }

                    var mainModule = laserBody.main;
 
                    mainModule.startSize3D = true; 

                    // Apply LaserWidth to startSizeX (and startSizeZ for uniform thickness)
                    mainModule.startSizeX = LaserWidth;
                    mainModule.startSizeY = LaserDistance;
                    mainModule.startSizeZ = LaserWidth; // Apply LaserWidth for depth as well (if needed for 3D appearance)
                }
            }
        }

        void Start()
        {
          
            if (LaserStart != null)
            {
                // Instantiate LaserStart at the MLaser's root position, as a child of this MLaser.
                // This assumes LaserStart is the effect at the very beginning of the laser beam.
                ParticleSystem startInstance = Instantiate(LaserStart, transform.position, transform.rotation);
                startInstance.transform.SetParent(transform); 
            }
        }

        public void SetLaser(Vector3 start, Vector3 end)
        {
            transform.position = start;
            _targetEndPoint = end; // Use _targetEndPoint for consistency
            LaserDistance = Vector3.Distance(start, end);
            transform.LookAt(end);
            UpdateLaser();
        }
 
        public void StopLaser()
        {
            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Stop();
            }
            if (LaserStop != null)
            {
                // Instantiate at the stored exact end point (_targetEndPoint) in world space
                ParticleSystem stopInstance = Instantiate(LaserStop, _targetEndPoint, Quaternion.identity);
                stopInstance.transform.localPosition = Vector3.zero;
                stopInstance.transform.LookAt(_targetEndPoint);

                // Orient the stop effect to match the laser's last direction
                stopInstance.transform.rotation = transform.rotation; 

                // Apply length scaling to the stop effect (if it's a stretched particle system)
                var mainModule = stopInstance.main;
                mainModule.startSize3D = true; 
                mainModule.startSizeX = stopInstance.main.startSizeX.constant;; // Use LaserWidth for the stop effect's thickness
                mainModule.startSizeY = LaserDistance; // Use LaserDistance for the stop effect's length
                mainModule.startSizeZ = 1; // Use LaserWidth for the stop effect's depth
            }
        }

         
    }
}


