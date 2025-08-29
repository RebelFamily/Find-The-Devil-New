using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class ITweenMagic : MonoBehaviour
{

    // Use this for initialization

    //Space
    public enum MovementType
    {
        WorldSpace,
        UISpace
        
    }

    [Tooltip("Space in which tween will take place")]
    public MovementType ITweenSpace;


    [Tooltip("tweeen easeType Movement")]
    public Ease EaseTypeMovement;
    [Tooltip("tweeen easeType Rotation")]
    public Ease EaseTypeRotation;
    [Tooltip("tweeen easeType Scale")]
    public Ease EaseTypeScale;

    //Time
    [Tooltip("Time(sec) for Tween ")]
    public float timeMovement = 1;
    [Tooltip("Time(sec) for Tween ")]
    public float timeRotation = 1;
    [Tooltip("Time(sec) for Tween ")]
    public float timeScale = 1;

    //Delay Time
    [Tooltip("Delay Time(sec) for Tween to start ")]
    public float delayMovement;
    [Tooltip("Delay Time(sec) for Tween to start ")]
    public float delayRotation;
    [Tooltip("Delay Time(sec) for Tween to start ")]
    public float delayScale;


    //Loop types
    public enum LoopTypeCustom
    {
        none,
        pingpong,
        loop
    }


    public LoopTypeCustom LoopTypeMovement;
    public LoopTypeCustom LoopTypeRotation;
    public LoopTypeCustom LoopTypeScale;

    //Tween Type
    public bool Movement;
    public bool Rotation;
    public bool Scale;

    //For World
    public Vector3 initialPosition3D;
    public Vector3 targetPosition3D;

    //For UI
    public Vector2 initialPosition2D;
    public Vector2 targetPosition2D;


    public Vector3 initialRotation;
    public Vector3 targetRotation;


    public Vector3 initialScale;
    public Vector3 targetScale;

    public UnityEvent movementTweenCompleteEvent;
	public UnityEvent rotationTweenCompleteEvent;
	public UnityEvent scaleTweenCompleteEvent;
    // Use this for initialization
    private void OnDisable()
    {
        //transform.DOTogglePause();
    }
    void OnEnable()
    {
	    //transform.DOTogglePause();
	    //Movement
	    if (Movement)
	    {
		    if (ITweenSpace.Equals(MovementType.UISpace))
		    {						
			    PlayUiMovement();
		    }
		    else
		    {
			    PlayWorldMovement();
		    }

	    }
	    

	    //Rotation
	    if (Rotation)
	    {
		    PlayRotation();
	    }

	    //Scale
	    if (Scale)
	    {
		    PlayScale();
	    }

	    //Destroy(gameObject);

    }



    #region LoopTypes


    private int loopCountMove, loopCountRotate, loopCountScale;
    
    LoopType GetLoopTypeMovement()
    {
	    if (LoopTypeMovement == LoopTypeCustom.none)
	    {
		    loopCountMove = 1;
		    return LoopType.Incremental;
	    }
	    else  if (LoopTypeMovement == LoopTypeCustom.pingpong)
	    {
		    loopCountMove = -1;
		    return LoopType.Yoyo;
	    }
	    else
	    {
		    loopCountMove = -1;
		    return LoopType.Restart;
	    }
    }
    
    
    LoopType GetLoopTypeRotate()
    {
	    if (LoopTypeRotation == LoopTypeCustom.none)
	    {
		    loopCountRotate = 1;
		    return LoopType.Incremental;
	    }
	    else  if (LoopTypeRotation == LoopTypeCustom.pingpong)
	    {
		    loopCountRotate = -1;
		    return LoopType.Yoyo;
	    }
	    else  //if (LoopTypeRotation == LoopTypeCustom.loop)
	    {
		    loopCountRotate = -1;
		    return LoopType.Restart;
	    }
	    
    }
    
    LoopType GetLoopTypeScale()
    {
	    if (LoopTypeScale == LoopTypeCustom.none)
	    {
		    loopCountScale = 1;
		    return LoopType.Incremental;
	    }
	    else  if (LoopTypeScale == LoopTypeCustom.pingpong)
	    {
		    loopCountScale = -1;
		    return LoopType.Yoyo;
	    }
	    else // if (LoopTypeScale == LoopTypeCustom.loop)
	    {
		    loopCountScale = -1;
		    return LoopType.Restart;
	    }
    }

    #endregion
    
    
    


    //2D OnUpdate EventListners
    public void MoveGuiElement(Vector2 position)
    {
        GetComponent<RectTransform>().anchoredPosition = position;
    }


    //3D Event Listners
    public void MoveObject(Vector3 position)
    {
        GetComponent<Transform>().localPosition = position;
    }

    public void RotateObject(Vector3 rotation)
    {
        GetComponent<Transform>().localEulerAngles = rotation;
    }

    public void ScaleObject(Vector3 scale)
    {
        GetComponent<Transform>().localScale = scale;
    }

	#region MovementMethods

	public void PlayUiMovement()
	{
		GetComponent<RectTransform>().anchoredPosition = initialPosition2D;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", 
			GetComponent<RectTransform>().anchoredPosition, "to", targetPosition2D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "onupdate", "MoveGuiElement",
			"oncomplete", "OnMovementTweenCompleted", "easetype", EaseTypeMovement.ToString()));*/

		var loopType = GetLoopTypeMovement();
		GetComponent<RectTransform>().DOLocalMove(targetPosition2D, timeMovement).SetEase(EaseTypeMovement).
			SetDelay(delayMovement).SetLoops(loopCountMove , loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);
		
		
		
	}

	public void PlayWorldMovement()
	{
		transform.localPosition = initialPosition3D;				
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", GetComponent<Transform>().localPosition, "to", targetPosition3D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "oncomplete", "OnMovementTweenCompleted", "onupdate", "MoveObject","easetype", EaseTypeMovement.ToString()));*/
		
		var loopType = GetLoopTypeMovement();
		gameObject.transform.DOMove(targetPosition3D, timeMovement).SetEase(EaseTypeMovement).SetDelay(delayMovement).
			SetLoops(loopCountMove , loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);

	}

	public void PlayScale()
	{
		transform.localScale = initialScale;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayScale, "from", GetComponent<Transform>().localScale, "to", targetScale, "time", timeScale,
			"looptype", LoopTypeScale.ToString(), "onupdate", "ScaleObject","oncomplete", "OnScaleTweenCompleted", "easetype", EaseTypeScale.ToString()));*/
		var loopType = GetLoopTypeScale();
		gameObject.transform.DOScale(targetScale, timeMovement).SetEase(EaseTypeScale).SetDelay(delayScale).
			SetLoops(loopCountScale , loopType).SetUpdate(true)
			.OnComplete(OnScaleTweenCompleted);
		
		
	}
	public void PlayRotation()
	{

		transform.localEulerAngles = initialRotation;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayRotation, "from", GetComponent<Transform>().localEulerAngles, "to", targetRotation, 
			"time", timeRotation, "looptype", LoopTypeRotation.ToString(), "onupdate", "RotateObject","oncomplete", "OnRotationTweenCompleted", "easetype", EaseTypeRotation.ToString()));*/
		var loopType = GetLoopTypeRotate();
		gameObject.transform.DORotate(targetRotation, timeRotation).SetEase(EaseTypeRotation).SetDelay(delayRotation).
			SetLoops(loopCountRotate , loopType).SetUpdate(true)
			.OnComplete(OnRotationTweenCompleted);
	}

	#endregion

	#region CodeMethods
    public void PlayForwardUiMovement()
    {
        GetComponent<RectTransform>().anchoredPosition = initialPosition2D;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", GetComponent<RectTransform>().anchoredPosition, "to", targetPosition2D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "onupdate", "MoveGuiElement","oncomplete", "OnMovementTweenCompleted", "easetype", EaseTypeMovement.ToString()));*/
		var loopType = GetLoopTypeMovement();
		GetComponent<RectTransform>().DOLocalMove(targetPosition2D, timeMovement).SetEase(EaseTypeMovement).
			SetDelay(delayMovement).SetLoops(loopCountMove , loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);
    }

    public void PlayReverseUiMovement()
    {
        GetComponent<RectTransform>().anchoredPosition = targetPosition2D;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", GetComponent<RectTransform>().anchoredPosition, "to", initialPosition2D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "onupdate", "MoveGuiElement","oncomplete", "OnMovementTweenCompleted", "easetype", EaseTypeMovement.ToString()));*/
		var loopType = GetLoopTypeMovement();
		gameObject.transform.DOLocalMove(initialPosition2D, timeMovement).SetEase(EaseTypeMovement).
			SetDelay(delayMovement).SetLoops(loopCountMove ,  loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);
    }

	public void PlayForwardWorldMovement()
    {
	    transform.localPosition = initialPosition3D;				
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", GetComponent<Transform>().localPosition, "to", targetPosition3D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "oncomplete", "OnMovementTweenCompleted", "onupdate", "MoveObject","easetype", EaseTypeMovement.ToString()));*/
		var loopType = GetLoopTypeMovement();
		gameObject.transform.DOMove(targetPosition3D, timeMovement).SetEase(EaseTypeMovement).SetDelay(delayMovement).
			SetLoops(loopCountMove ,  loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);
    }

	public void PlayReverseWorldMovement()
	{
		transform.localPosition = targetPosition3D;				
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayMovement, "from", GetComponent<Transform>().localPosition, "to", initialPosition3D,
			"time", timeMovement, "looptype", LoopTypeMovement.ToString(), "oncomplete", "OnMovementTweenCompleted", "onupdate", "MoveObject","easetype", EaseTypeMovement.ToString()));*/
		var loopType = GetLoopTypeMovement();
		gameObject.transform.DOMove(initialPosition3D, timeMovement).SetEase(EaseTypeMovement).SetDelay(delayMovement).
			SetLoops(loopCountMove ,  loopType).SetUpdate(true)
			.OnComplete(OnMovementTweenCompleted);
	}

    public void PlayForwardScale()
    {
	    transform.localScale = initialScale;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayScale, "from", GetComponent<Transform>().localScale, "to", targetScale, "time", timeScale,
			"looptype", LoopTypeCustom.none, "onupdate", "ScaleObject","oncomplete", "OnScaleTweenCompleted", "easetype", EaseTypeScale.ToString()));*/
		var loopType = GetLoopTypeScale();
		gameObject.transform.DOScale(targetScale, timeScale).SetEase(EaseTypeScale).SetDelay(delayScale).
			SetLoops(loopCountScale , loopType).SetUpdate(true)
			.OnComplete(OnScaleTweenCompleted);
    }

    public void PlayReverseScale()
    {
	    transform.localScale = targetScale;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayScale, "from", GetComponent<Transform>().localScale, "to", initialScale, "time", timeScale,
			"looptype", LoopTypeCustom.none, "onupdate", "ScaleObject","oncomplete", "OnScaleTweenCompleted", "easetype", EaseTypeScale.ToString()));*/
		var loopType = GetLoopTypeScale();
		gameObject.transform.DOScale(initialScale, timeScale).SetEase(EaseTypeScale).SetDelay(delayScale).
			SetLoops(loopCountScale , loopType).SetUpdate(true)
			.OnComplete(OnScaleTweenCompleted);
    }

    public void PlayForwardRotation()
    {
	    transform.localEulerAngles = initialRotation;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayRotation, "from", GetComponent<Transform>().localEulerAngles, "to", targetRotation, 
			"time", timeRotation, "looptype", LoopTypeRotation.ToString(), "onupdate", "RotateObject","oncomplete", "OnRotationTweenCompleted", "easetype", EaseTypeRotation.ToString()));*/
		var loopType = GetLoopTypeRotate();
		gameObject.transform.DORotate(targetRotation, timeRotation).SetEase(EaseTypeRotation).SetDelay(delayRotation).
			SetLoops(loopCountRotate , loopType).SetUpdate(true)
			.OnComplete(OnRotationTweenCompleted);

    }

    public void PlayReverseRotation()
    {
	    transform.localEulerAngles = targetRotation;
		/*iTween.ValueTo(this.gameObject, iTween.Hash("delay", delayRotation, "from", GetComponent<Transform>().localEulerAngles, "to", initialRotation,
			"time", timeRotation, "looptype", LoopTypeRotation.ToString(), "onupdate", "RotateObject","oncomplete", "OnRotationTweenCompleted", "easetype", EaseTypeRotation.ToString()));*/
		
		var loopType = GetLoopTypeRotate();
		gameObject.transform.DORotate(initialRotation, timeRotation).SetEase(EaseTypeRotation).
			SetDelay(delayRotation).SetLoops(loopCountRotate , loopType).SetUpdate(true)
			.OnComplete(OnRotationTweenCompleted);
    }


	#endregion

	#region Events
	void OnMovementTweenCompleted()
    {
	    movementTweenCompleteEvent?.Invoke();
    }

	void OnRotationTweenCompleted()
	{
		rotationTweenCompleteEvent?.Invoke();
	}

	void OnScaleTweenCompleted()
	{
		scaleTweenCompleteEvent?.Invoke();
	}

	#endregion


	 

	
	
	
		
}



