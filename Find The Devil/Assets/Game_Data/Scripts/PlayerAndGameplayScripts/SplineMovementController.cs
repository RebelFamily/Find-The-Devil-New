using UnityEngine;

public class SplineMovementController : MonoBehaviour
{
    // // Consider using a third-party spline asset or implementing your own
    // // For simplicity, let's assume SplineData contains Vector3 points
    // private SplineData currentSpline;
    // private int currentSplinePointIndex = 0;
    // private float moveSpeed = 5f; // Example speed

    public void Init()
    {
        Debug.Log("SplineMovementController Initialized.");
    }

    public void MoveAlongSpline(SplineData spline)
    {
       // currentSpline = spline;
        // currentSplinePointIndex = 0;
        // // Reset player position to start of spline
        // if (currentSpline != null && currentSpline.Points.Count > 0)
        // {
        //     transform.position = currentSpline.Points[0];
        // }
        // Debug.Log($"Moving along spline with {currentSpline.Points.Count} points.");
    }

    private void Update()
    {
        // if (currentSpline == null || currentSpline.Points.Count <= 1 || currentSplinePointIndex >= currentSpline.Points.Count -1)
        // {
        //     // Reached end of spline or no spline
        //     return;
        // }
        //
        // Vector3 targetPosition = currentSpline.Points[currentSplinePointIndex + 1];
        // transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        //
        // if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        // {
        //     currentSplinePointIndex++;
        //     if (currentSplinePointIndex >= currentSpline.Points.Count - 1)
        //     {
        //         Debug.Log("Reached end of spline.");
        //         // Signal LevelManager that path is complete
        //         GameManager.Instance.OnLevelCompleted(GameManager.Instance.LevelManager.GetCurrentLevelData().GetLevelId());
        //     }
        // }
        // // Optional: Make player look at target position
        // transform.LookAt(targetPosition);
    }
}
