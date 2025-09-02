using Dreamteck.Splines;
using UnityEngine;

public class SplineData : MonoBehaviour
{
    [SerializeField] private SplineComputer _splinePath;


    public SplineComputer GetSplineData()
    {
        return _splinePath;
    }

    public void ActivateTools()
    {
        
        if(GameManager.Instance.levelManager.isLevelFail)
            return;
        
        if(GameManager.Instance.levelManager.GlobalLevelNumber == 0)
            GameManager.Instance.playerController.SetupTools();


        if (GameManager.Instance.levelManager._currentLevelNumber == 7)
        {
                GameManager.Instance.playerController.SetupTools();
        }
            
        
        
        if(!GameManager.Instance.uiManager.GetVIPGunAnimationCheck())
            GameManager.Instance.playerController.SetupTools();
        
    }

    public void DeactivateTools()
    {
        GameManager.Instance.uiManager.HidePanel(UIPanelType.GameOverlayPanel);
        GameManager.Instance.playerController.ResetTools();
    }

    public void StopMovementFor(float time)
    {
        StartCoroutine(GameManager.Instance.playerController.StopMovementFor(time));
    }
 
    public void SetNewMovementSpeed(float speed)
    {
        GameManager.Instance.playerController.SetNewMovementSpeed(speed);
    }
    public void SetNewMovementSpeed(float speed,float wait)
    {
        
        StartCoroutine(GameManager.Instance.playerController.SetNewMovementSpeed(speed,wait));
    }

    public void ReverseMovement(float speed)
    {
        GameManager.Instance.playerController.SetNewMovementSpeed(-speed);
        GameManager.Instance.playerController.SetNewMovementSpeed(0);
        //GameManager.Instance.playerController.ResetCameraAngles(0);
        Vector3 targetRotation = new Vector3(0f, 180, 0f);
        GameManager.Instance.playerController.RotateToTarget(targetRotation, 0f);   
        
        targetRotation = new Vector3(0f, -180, 0f);

        GameManager.Instance.playerController.RotateToTarget(targetRotation, 1f);   
        GameManager.Instance.playerController.SetNewMovementSpeed(-speed);
        
    }


}
 