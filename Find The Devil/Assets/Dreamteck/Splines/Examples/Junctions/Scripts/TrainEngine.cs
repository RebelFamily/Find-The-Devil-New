using UnityEngine;

namespace Dreamteck.Splines.Examples.Junctions.Scripts
{
    public class TrainEngine : MonoBehaviour
    {
        public SplineTracer tracer;
        private double lastPercent = 0.0;
        public Wagon wagon;

        private void Awake()
        {
            wagon = GetComponent<Wagon>();
        }
        
        private void OnEnable()
        {
            tracer = GetComponent<SplineTracer>();
            tracer.onMotionApplied += OnMotionApplied;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void OnMotionApplied()
        {
            //Apply the wagon's offset (this will recursively apply the offsets to the rest of the wagons in the chain)
            if (wagon.segment.spline == null)
                GameObject.FindGameObjectWithTag("Respawn").GetComponent<SplineComputer>();
            
            lastPercent = tracer.result.percent;
            wagon.UpdateOffset();
        }
    }
}