using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase; 

public class InputManager : MonoBehaviour
{
    public delegate void OnTapAction(Vector2 position);
    public static event OnTapAction OnTap;

    public delegate void OnDragAction(Vector2 delta);
    public static event OnDragAction OnDrag;
    

    public void Init()
    {
        Debug.Log("InputManager Initialized.");
        // If using new Input System, enable actions here
        // tapAction.performed += ctx => OnTap?.Invoke(ctx.ReadValue<Vector2>());
        // dragAction.performed += ctx => OnDrag?.Invoke(ctx.ReadValue<Vector2>());
        // tapAction.Enable();
        // dragAction.Enable();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
          //  Debug.Log("Touch count is : " + Input.touchCount);
            OnTap?.Invoke(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            OnDrag?.Invoke(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        }
        // Consider touch input specific logic for mobile
        if (Input.touchCount > 0)
        {
            Debug.Log("Touch count is : " + Input.touchCount);
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnTap?.Invoke(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnDrag?.Invoke(touch.deltaPosition);
            }
        } 
    } 
}