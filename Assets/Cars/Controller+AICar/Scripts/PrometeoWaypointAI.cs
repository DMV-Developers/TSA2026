using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(PrometeoCarController))]
public class PrometeoWaypointAI : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [Tooltip("Parent container holding waypoint objects")]
    public Transform waypointContainer;

    [Tooltip("OR manually assign waypoints (overrides container)")]
    public Transform[] waypoints;

    [Tooltip("Distance to waypoint before moving to next")]
    [Range(2f, 20f)]
    public float waypointReachDistance = 5f;

    [Tooltip("Loop back to first waypoint when finished")]
    public bool loopWaypoints = true;

    [Tooltip("Slow down when approaching waypoint")]
    public bool slowAtWaypoints = true;

    [Header("Visual Settings")]
    [Tooltip("Automatically change waypoint colors")]
    public bool autoColorWaypoints = true;

    [Tooltip("Material for normal waypoints")]
    public Material normalWaypointMaterial;

    [Tooltip("Material for current target waypoint")]
    public Material activeWaypointMaterial;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color waypointColor = Color.green;
    public Color activeWaypointColor = Color.red;

    [Header("Spline Path (UNDER DEVELOPMENT)")]
    public SplineContainer splineContainer;
    public Mesh SphereMesh;
    public bool showGeneratedPoints = false;

    // Private variables
    private PrometeoCarController carController;
    private int currentWaypointIndex = 0;
    private bool waypointsComplete = false;
    private MeshRenderer[] waypointRenderers;

    void Start()
    {
        // Get reference to car controller
        carController = GetComponent<PrometeoCarController>();
        if (!carController.isAI)
        {
            enabled = false;
        }

        // Auto-load waypoints from container if not manually assigned
        if ((waypoints == null || waypoints.Length == 0) && waypointContainer != null)
        {
            //LoadWaypointsFromContainer();
            GeneratePointsOnSpline(2000);
        }

        // Validation
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("PrometeoWaypointAI: No waypoints assigned!");
            enabled = false;
            return;
        }

        // Cache waypoint renderers for material swapping
        if (autoColorWaypoints)
        {
            CacheWaypointRenderers();
            InitializeWaypointMaterials();
        }

        // Set first waypoint as target
        SetCurrentWaypoint();
    }

    void GeneratePointsOnSpline(int numberOfSamples = 100)
    {
        Vector3[] pathPoints = new Vector3[numberOfSamples];
        for (int i = 0; i < numberOfSamples; i++)
        {
            float t = (float)i / (numberOfSamples - 1);
            Vector3 position = splineContainer.EvaluatePosition(splineContainer.Splines[0], t);
            pathPoints[i] = position;
        }
        waypoints = new Transform[numberOfSamples];
        for (int i = 0; i < numberOfSamples; i++)
        {
            GameObject waypointObj = new GameObject($"wp_{i}");
            waypointObj.transform.position = pathPoints[i];
            waypointObj.transform.parent = waypointContainer;
            waypointObj.AddComponent<MeshRenderer>();
            waypointObj.AddComponent<MeshFilter>().mesh = SphereMesh;
            waypointObj.SetActive(showGeneratedPoints);

            waypoints[i] = waypointObj.transform;
        }
        Debug.Log($"PrometeoWaypointAI: Loaded {numberOfSamples} waypoints from container '{waypointContainer.name}'");
    }

    void LoadWaypointsFromContainer()
    {
        int childCount = waypointContainer.childCount;
        waypoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            waypoints[i] = waypointContainer.GetChild(i);
        }

        Debug.Log($"PrometeoWaypointAI: Loaded {childCount} waypoints from container '{waypointContainer.name}'");
    }

    void CacheWaypointRenderers()
    {
        waypointRenderers = new MeshRenderer[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                waypointRenderers[i] = waypoints[i].GetComponent<MeshRenderer>();

                if (waypointRenderers[i] == null)
                {
                    Debug.LogWarning($"PrometeoWaypointAI: Waypoint {i} ({waypoints[i].name}) has no MeshRenderer!");
                }
            }
        }
    }

    void InitializeWaypointMaterials()
    {
        if (normalWaypointMaterial == null)
        {
            Debug.LogWarning("PrometeoWaypointAI: Normal waypoint material not assigned!");
            return;
        }

        // Set all waypoints to normal material
        for (int i = 0; i < waypointRenderers.Length; i++)
        {
            if (waypointRenderers[i] != null)
            {
                waypointRenderers[i].material = normalWaypointMaterial;
            }
        }
    }

    void Update()
    {
        if (waypointsComplete || waypoints.Length == 0)
            return;

        // Check if current waypoint is reached
        if (IsWaypointReached())
        {
            MoveToNextWaypoint();
        }

        // Optional: Adjust speed near waypoints
        if (slowAtWaypoints)
        {
            AdjustSpeedNearWaypoint();
        }
    }

    bool IsWaypointReached()
    {
        if (carController.aiTarget == null)
            return false;

        float distance = Vector3.Distance(transform.position, carController.aiTarget.position);
        return distance <= waypointReachDistance;
    }

    void MoveToNextWaypoint()
    {
        // Reset previous waypoint to normal color
        if (autoColorWaypoints)
        {
            SetWaypointMaterial(currentWaypointIndex, normalWaypointMaterial);
        }

        currentWaypointIndex++;

        // Check if we've reached the end
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopWaypoints)
            {
                waypointsComplete = true;
                currentWaypointIndex = 0; // Loop back to start
            }
            else
            {
                waypointsComplete = true;
                Debug.Log("PrometeoWaypointAI: All waypoints reached!");
                return;
            }
        }

        SetCurrentWaypoint();
    }

    void SetCurrentWaypoint()
    {
        if (waypoints[currentWaypointIndex] == null)
        {
            Debug.LogError($"PrometeoWaypointAI: Waypoint {currentWaypointIndex} is null!");
            return;
        }

        carController.aiTarget = waypoints[currentWaypointIndex];

        // Highlight current waypoint
        if (autoColorWaypoints)
        {
            SetWaypointMaterial(currentWaypointIndex, activeWaypointMaterial);
        }

        Debug.Log($"PrometeoWaypointAI: Moving to waypoint {currentWaypointIndex}");
    }

    void SetWaypointMaterial(int index, Material material)
    {
        if (material == null || waypointRenderers == null || waypointRenderers[index] == null)
            return;

        waypointRenderers[index].material = material;
    }

    void AdjustSpeedNearWaypoint()
    {
        float distance = Vector3.Distance(transform.position, carController.aiTarget.position);

        // Slow down when within 2x reach distance
        if (distance < waypointReachDistance * 2f)
        {
            float speedMultiplier = Mathf.Clamp01(1.5f * distance / waypointReachDistance);
            carController.aiMaxSpeed = Mathf.Lerp(30f, 80f, speedMultiplier);
        }
        else
        {
            carController.aiMaxSpeed = 80f; // Reset to normal speed
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || waypoints == null || waypoints.Length == 0)
            return;

        // Draw waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            // Color based on if it's the current waypoint
            bool isActive = Application.isPlaying && i == currentWaypointIndex;
            Gizmos.color = isActive ? activeWaypointColor : waypointColor;

            // Draw sphere at waypoint
            Gizmos.DrawWireSphere(waypoints[i].position, waypointReachDistance);

            // Draw waypoint number
#if UNITY_EDITOR
            UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 2f, $"WP {i}");
#endif

            // Draw line to next waypoint
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = waypointColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else if (loopWaypoints && i == waypoints.Length - 1 && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
    }

    // Public methods for external control
    public void ResetWaypoints()
    {
        if (autoColorWaypoints && waypointRenderers != null)
        {
            SetWaypointMaterial(currentWaypointIndex, normalWaypointMaterial);
        }

        currentWaypointIndex = 0;
        waypointsComplete = false;
        SetCurrentWaypoint();
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;

        if (autoColorWaypoints)
        {
            CacheWaypointRenderers();
            InitializeWaypointMaterials();
        }

        ResetWaypoints();
    }

    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }

    public bool IsComplete()
    {
        return waypointsComplete;
    }
}