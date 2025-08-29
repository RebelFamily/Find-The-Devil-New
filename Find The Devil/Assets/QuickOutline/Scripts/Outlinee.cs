using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening; // Ensure DOTween is imported

[DisallowMultipleComponent]
public class Outlinee : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();
    
    public enum Mode
    {
        OutlineAll,      
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly 
    }

    public Mode OutlineMode
    {
        get { return outlineMode; }
        set
        {
            outlineMode = value;
            needsUpdate = true;
        }
    }

    // OutlineColor property to update the material, now set by Update() for continuous animation
    public Color OutlineColor
    {
        get { return currentOutlineColor; }
        set
        {
            currentOutlineColor = value;
            needsUpdate = true;
        }
    }

    // The OutlineWidth property is controlled by DOTween animations
    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            needsUpdate = true; 
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }
    
    [SerializeField]
    private Mode outlineMode;
    
    [SerializeField, Tooltip("The starting color for the continuous outline animation.")]
    private Color outlineColorStart = Color.white;

    [SerializeField, Tooltip("The ending color for the continuous outline animation.")]
    private Color outlineColorEnd = Color.black;
    
    [SerializeField, Range(0.1f, 10f), Tooltip("Speed at which the outline color animates between the start and end colors.")]
    private float colorAnimationSpeed = 1f;

    [SerializeField, Range(0f, 100f)]
    private float outlineWidth = 2f; // This serves as a default/initial width if no animation is happening, or the target width for active state

    [Header("Activation/Deactivation Animation")]
    [SerializeField, Range(0.1f, 5f), Tooltip("Duration of the outline width animation.")]
    private float activationAnimationDuration = 0.3f;
    [SerializeField, Range(0f, 15f), Tooltip("The maximum width of the outline when active.")]
    private float activeOutlineMaxWidth = 15f; 
    [SerializeField]
    private Ease activationEaseType = Ease.OutQuad;
    [SerializeField]
    private Ease deactivationEaseType = Ease.InQuad;

    [Header("Optional")]
    
    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;
    
    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();
    
    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;
    private bool needsUpdate;

    // Control flag for outline visibility and animation state
    // Outline is INACTIVE when CheckCondition is TRUE
    // Outline is ACTIVE (and animating) when CheckCondition is FALSE
    public bool CheckCondition = true; // Default to true, meaning outline is initially inactive

    public GameObject parentObj; // This reference is used for SetActive(false) in LaserFired

    // Private field to hold the current color that UpdateMaterialProperties will read
    private Color currentOutlineColor; 

    // Tween for managing width animation only
    private Tween _widthTween;


    void Awake()
    {
        
        
        if (GameManager.Instance.levelManager._currentLevelNumber == 0 && GameManager.Instance.levelManager.GlobalLevelNumber == 0)
        {
            
            renderers = GetComponentsInChildren<Renderer>();
            
            outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
            outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

            outlineMaskMaterial.name = "OutlineMask (Instance)";
            outlineFillMaterial.name = "OutlineFill (Instance)";
            
            currentOutlineColor = outlineColorStart;
            outlineWidth = 0f; 

            LoadSmoothNormals();
            needsUpdate = true; 

            UpdateMaterialProperties();
        }
    }

 
    void OnEnable()
    {
        if (GameManager.Instance.levelManager._currentLevelNumber == 0 && GameManager.Instance.levelManager.GlobalLevelNumber == 0)
        {
            // Subscribe to events
            ScannerLogic.OnScannerUsed += ScannerUsed;
            PlayerController.OnWeaponSwitched += WeaponSwitched;
            LaserGunController.OnLaserFired += LaserFired;

            // Set initial outline active state based on CheckCondition
            SetOutlineActive(!CheckCondition);
        }
    }

    void ScannerUsed()
    {
       // Debug.Log("Outlinee: ScannerUsed called. Setting CheckCondition = true (Outline Inactive).");
        CheckCondition = true;
        SetOutlineActive(!CheckCondition); // Update outline state
    }

    void WeaponSwitched()
    {
      //  Debug.Log("Outlinee: WeaponSwitched called. Toggling CheckCondition (Outline Toggles).");
        CheckCondition = !CheckCondition;
        SetOutlineActive(!CheckCondition); // Update outline state
    }

    void LaserFired()
    {
       // Debug.Log("Outlinee: LaserFired called. Setting parentObj inactive and CheckCondition = true (Outline Inactive).");
        if (parentObj != null)
        {
            parentObj.SetActive(false); // Deactivates the entire object this Outlinee is likely attached to
        }
        CheckCondition = true;
        SetOutlineActive(!CheckCondition); // Update outline state
    }
    
    /// <summary>
    /// Controls the active state of the outline, including adding/removing materials
    /// and animating the outline width.
    /// </summary>
    /// <param name="active">If true, outline becomes active (width animates up). If false, outline becomes inactive (width animates down).</param>
    private void SetOutlineActive(bool active)
    {
        // Kill existing width tween to prevent conflicts
        _widthTween?.Kill();

        if (active)
        {
            // Outline becoming active:
            // 1. Ensure materials are added (if not already)
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue; // Skip if renderer might have been destroyed
                var materials = renderer.sharedMaterials.ToList();
                if (!materials.Contains(outlineMaskMaterial)) materials.Add(outlineMaskMaterial);
                if (!materials.Contains(outlineFillMaterial)) materials.Add(outlineFillMaterial);
                renderer.materials = materials.ToArray();
            }

            // 2. Animate width from current (likely 0) to activeOutlineMaxWidth
            _widthTween = DOTween.To(() => OutlineWidth, x => OutlineWidth = x, activeOutlineMaxWidth, activationAnimationDuration)
                .SetEase(activationEaseType)
                .OnUpdate(() => UpdateMaterialProperties()); // Force update during tween
        }
        else
        {
            // Outline becoming inactive:
            // 1. Animate width from current to 0
            _widthTween = DOTween.To(() => OutlineWidth, x => OutlineWidth = x, 0f, activationAnimationDuration)
                .SetEase(deactivationEaseType)
                .OnUpdate(() => UpdateMaterialProperties()) // Force update during tween
                .OnComplete(() => {
                    // 2. Remove materials AFTER width animation completes
                    foreach (var renderer in renderers)
                    {
                        if (renderer == null) continue;
                        var materials = renderer.sharedMaterials.ToList();
                        if (materials.Contains(outlineMaskMaterial)) materials.Remove(outlineMaskMaterial);
                        if (materials.Contains(outlineFillMaterial)) materials.Remove(outlineFillMaterial);
                        renderer.materials = materials.ToArray();
                    }
                    _widthTween = null; // Clear reference
                });
        }
    }


    void OnValidate()
    {
        // This ensures the outline appears correctly in editor when values change
        needsUpdate = true;
        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }
    
    void Update()
    {
        if (GameManager.Instance.levelManager._currentLevelNumber == 0 && GameManager.Instance.levelManager.GlobalLevelNumber == 0)
        {
            // Only animate color if CheckCondition is FALSE (meaning outline should be active)
            if (!CheckCondition)
            {
                // Continuous color animation using Mathf.PingPong
                float t = Mathf.PingPong(Time.time * colorAnimationSpeed, 1);
                currentOutlineColor = Color.Lerp(outlineColorStart, outlineColorEnd, t);

                // Apply the current animated color to the material directly every frame
                outlineFillMaterial.SetColor("_OutlineColor", currentOutlineColor);

                // Update other material properties if they need a refresh (e.g., OutlineMode changes)
                if (needsUpdate)
                {
                    needsUpdate = false;
                    UpdateMaterialProperties();
                }
            }
            else
            {
                // If CheckCondition is true, the outline materials are removed by SetOutlineActive(false).
                // No need to set color or update properties if the outline is inactive.
            }
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        ScannerLogic.OnScannerUsed -= ScannerUsed;
        PlayerController.OnWeaponSwitched -= WeaponSwitched;
        LaserGunController.OnLaserFired -= LaserFired;
        
        // Ensure materials are explicitly removed and tweens are killed when disabled
        _widthTween?.Kill();
        SetOutlineActive(false); // This will remove materials if they are still present
    }
    
    void OnDestroy()
    {
        // Destroy material instances
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
        
        // Kill tweens when the object is destroyed
        _widthTween?.Kill();
    }
    
    void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }
            
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
            
            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }
    
    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }
            
            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);
            
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
            
            var renderer = meshFilter.GetComponent<Renderer>();
            
            if (renderer != null)
            {
                CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
            }
        }
        
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                continue;
            }

            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }
 
    List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                continue;
            }
            
            var smoothNormal = Vector3.zero;
            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }
            smoothNormal.Normalize(); 

            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }
    
    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1)
        {
            return;
        }
        
        if (mesh.subMeshCount > materials.Length)
        {
            Debug.LogWarning($"Mesh '{mesh.name}' has more submeshes ({mesh.subMeshCount}) than materials ({materials.Length}). " +
                             "Cannot combine submeshes without sufficient materials. Please ensure your Renderer has enough material slots.");
            return;
        }
        
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }
    
    void UpdateMaterialProperties()
    {
        outlineFillMaterial.SetColor("_OutlineColor", currentOutlineColor); 

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", 0f); // Silhouette only typically has 0 width fill
                break;
        }
    }
}