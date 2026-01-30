using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SplineBarrierPlacer : MonoBehaviour
{
    [Header("Spline Reference")]
    [Tooltip("The SplineContainer to place barriers along")]
    public SplineContainer splineContainer;
    
    [Header("Barrier Settings")]
    [Tooltip("Prefab of the concrete jersey barrier")]
    public GameObject barrierPrefab;
    
    [Tooltip("Distance from spline center (left and right)")]
    [Range(1f, 20f)]
    public float offsetDistance = 5f;
    
    [Tooltip("Spacing between each barrier")]
    [Range(0.1f, 10f)]
    public float barrierSpacing = 1f;
    
    [Header("Placement Options")]
    [Tooltip("Place barriers on the left side")]
    public bool placeLeftSide = true;
    
    [Tooltip("Place barriers on the right side")]
    public bool placeRightSide = true;
    
    [Tooltip("Auto-rotate barriers to follow spline direction")]
    public bool autoRotate = true;
    
    [Tooltip("How barriers should rotate relative to spline")]
    public BarrierRotationMode rotationMode = BarrierRotationMode.ParallelToSpline;
    
    [Tooltip("Additional rotation offset (fine-tune alignment)")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Header("Ground Snapping")]
    [Tooltip("Snap barriers to terrain/ground surface")]
    public bool snapToGround = true;
    
    [Tooltip("Height to start raycast from above barrier position")]
    [Range(10f, 500f)]
    public float raycastHeight = 100f;
    
    [Tooltip("Maximum distance to raycast downward")]
    [Range(10f, 500f)]
    public float raycastDistance = 200f;
    
    [Tooltip("Layers to consider as ground (e.g., Terrain, Ground)")]
    public LayerMask groundLayerMask = -1;
    
    [Tooltip("Vertical offset from ground surface")]
    public float groundOffset = 0f;
    
    [Header("Organization")]
    [Tooltip("Parent object to hold all barriers (auto-created if null)")]
    public Transform barrierParent;
    
    [Header("Preview")]
    public bool showGizmos = true;
    public Color leftGizmoColor = Color.red;
    public Color rightGizmoColor = Color.blue;
    public bool showRaycastGizmos = false;
    
    private List<GameObject> spawnedBarriers = new List<GameObject>();
    
    public enum BarrierRotationMode
    {
        ParallelToSpline,
        PerpendicularToSpline,
        CustomOffset
    }
    
    [ContextMenu("Generate Barriers")]
    public void GenerateBarriers()
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineBarrierPlacer: No SplineContainer assigned!");
            return;
        }
        
        if (barrierPrefab == null)
        {
            Debug.LogError("SplineBarrierPlacer: No barrier prefab assigned!");
            return;
        }
        
        if (splineContainer.Splines.Count == 0)
        {
            Debug.LogError("SplineBarrierPlacer: SplineContainer has no splines!");
            return;
        }
        
        ClearBarriers();
        
        if (barrierParent == null)
        {
            GameObject parentObj = new GameObject("Barriers");
            parentObj.transform.parent = transform;
            parentObj.transform.localPosition = Vector3.zero;
            barrierParent = parentObj.transform;
        }
        
        Spline spline = splineContainer.Splines[0];
        float splineLength = spline.GetLength();
        int barrierCount = Mathf.CeilToInt(splineLength / barrierSpacing);
        
        Debug.Log($"Generating {barrierCount} barriers along spline (length: {splineLength:F2} units)");
        
        int successCount = 0;
        int failCount = 0;
        
        for (int i = 0; i <= barrierCount; i++)
        {
            float t = (float)i / barrierCount;
            
            Vector3 splinePosition = splineContainer.EvaluatePosition(spline, t);
            Vector3 splineTangent = splineContainer.EvaluateTangent(spline, t);
            splineTangent = Vector3.Normalize(splineTangent);
            
            Vector3 rightDirection = Vector3.Cross(splineTangent, Vector3.up);
            rightDirection = Vector3.Normalize(rightDirection);
            Vector3 leftDirection = -rightDirection;
            
            if (placeLeftSide)
            {
                Vector3 leftPosition = splinePosition + (leftDirection * offsetDistance);
                if (PlaceBarrier(leftPosition, splineTangent, "Left"))
                    successCount++;
                else
                    failCount++;
            }
            
            if (placeRightSide)
            {
                Vector3 rightPosition = splinePosition + (rightDirection * offsetDistance);
                if (PlaceBarrier(rightPosition, splineTangent, "Right"))
                    successCount++;
                else
                    failCount++;
            }
        }
        
        Debug.Log($"Barrier generation complete! Success: {successCount}, Failed: {failCount}");
    }
    
    private bool PlaceBarrier(Vector3 position, Vector3 tangent, string side)
    {
        Vector3 finalPosition = position;
        
        if (snapToGround)
        {
            Vector3 rayStart = position + Vector3.up * raycastHeight;
            RaycastHit hit;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance, groundLayerMask))
            {
                finalPosition = hit.point + Vector3.up * groundOffset;
                
                if (showRaycastGizmos)
                {
                    Debug.DrawLine(rayStart, hit.point, Color.green, 2f);
                }
            }
            else
            {
                Debug.LogWarning($"SplineBarrierPlacer: Failed to find ground at position {position}. Barrier not placed.");
                
                if (showRaycastGizmos)
                {
                    Debug.DrawLine(rayStart, rayStart + Vector3.down * raycastDistance, Color.red, 2f);
                }
                
                return false;
            }
        }
        
        GameObject barrier = Instantiate(barrierPrefab, finalPosition, Quaternion.identity, barrierParent);
        
        if (autoRotate)
        {
            Quaternion rotation = Quaternion.identity;
            
            switch (rotationMode)
            {
                case BarrierRotationMode.ParallelToSpline:
                    rotation = Quaternion.LookRotation(tangent, Vector3.up);
                    break;
                    
                case BarrierRotationMode.PerpendicularToSpline:
                    Vector3 perpendicular = Vector3.Cross(tangent, Vector3.up);
                    perpendicular = Vector3.Normalize(perpendicular);
                    rotation = Quaternion.LookRotation(perpendicular, Vector3.up);
                    break;
                    
                case BarrierRotationMode.CustomOffset:
                    rotation = Quaternion.identity;
                    break;
            }
            
            barrier.transform.rotation = rotation * Quaternion.Euler(rotationOffset);
        }
        
        barrier.name = $"Barrier_{side}_{spawnedBarriers.Count}";
        spawnedBarriers.Add(barrier);
        
        return true;
    }
    
    [ContextMenu("Clear Barriers")]
    public void ClearBarriers()
    {
        foreach (GameObject barrier in spawnedBarriers)
        {
            if (barrier != null)
            {
                DestroyImmediate(barrier);
            }
        }
        spawnedBarriers.Clear();
        
        if (barrierParent != null)
        {
            int childCount = barrierParent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(barrierParent.GetChild(i).gameObject);
            }
        }
        
        Debug.Log("All barriers cleared!");
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || splineContainer == null || splineContainer.Splines.Count == 0)
            return;
        
        Spline spline = splineContainer.Splines[0];
        float splineLength = spline.GetLength();
        int previewCount = Mathf.CeilToInt(splineLength / barrierSpacing);
        
        for (int i = 0; i <= previewCount; i += 5)
        {
            float t = (float)i / previewCount;
            
            Vector3 splinePosition = splineContainer.EvaluatePosition(spline, t);
            Vector3 splineTangent = splineContainer.EvaluateTangent(spline, t);
            splineTangent = Vector3.Normalize(splineTangent);
            Vector3 rightDirection = Vector3.Cross(splineTangent, Vector3.up);
            rightDirection = Vector3.Normalize(rightDirection);
            Vector3 leftDirection = -rightDirection;
            
            if (placeLeftSide)
            {
                Vector3 leftPosition = splinePosition + (leftDirection * offsetDistance);
                
                if (snapToGround)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(leftPosition + Vector3.up * raycastHeight, Vector3.down, out hit, raycastDistance, groundLayerMask))
                    {
                        leftPosition = hit.point + Vector3.up * groundOffset;
                    }
                }
                
                Gizmos.color = leftGizmoColor;
                Gizmos.DrawWireCube(leftPosition, Vector3.one * 0.5f);
            }
            
            if (placeRightSide)
            {
                Vector3 rightPosition = splinePosition + (rightDirection * offsetDistance);
                
                if (snapToGround)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(rightPosition + Vector3.up * raycastHeight, Vector3.down, out hit, raycastDistance, groundLayerMask))
                    {
                        rightPosition = hit.point + Vector3.up * groundOffset;
                    }
                }
                
                Gizmos.color = rightGizmoColor;
                Gizmos.DrawWireCube(rightPosition, Vector3.one * 0.5f);
            }
        }
    }
}