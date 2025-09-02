using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The speed and direction of the offset animation.")]
    [SerializeField] private Vector2 animationSpeed = new Vector2(0.1f, 0.0f);

    [Tooltip("The name of the material's texture property to animate. Typically '_MainTex'.")]
    [SerializeField] private string textureName = "_MainTex";

    // Private variables to store component references and current state
    private Renderer _renderer;
    private Vector2 _currentOffset;
    private Material _materialInstance;

    void Start()
    {
        // Get the Renderer component attached to this GameObject
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("MaterialAnimation: Renderer component not found on this GameObject. Please add one.", this);
            enabled = false; // Disable the script if no renderer is found
            return;
        }
        
        // Get a unique instance of the material to animate.
        // This is important to prevent affecting other objects that share the same material.
        _materialInstance = _renderer.material;

        // Get the initial texture offset
        _currentOffset = _materialInstance.GetTextureOffset(textureName);
    }

    void Update()
    {
        if (_materialInstance == null) return;

        // Calculate the new offset by adding the animation speed scaled by time
        _currentOffset += animationSpeed * Time.deltaTime;

        // Apply the new offset to the material's texture
        _materialInstance.SetTextureOffset(textureName, _currentOffset);
    }
}