using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class RespawnSystem : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Time the car is frozen after respawn")]
    public float freezeTime = 2f;
    
    [Tooltip("Initial spawn position (used if no checkpoint reached)")]
    public Transform initialSpawnPoint;
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    // Private variables
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private bool hasCheckpoint = false;
    private bool isFrozen = false;
    private Rigidbody carRigidbody;
    private PrometeoCarController carController;
    
    // Store original values to restore after freeze
    private bool wasKinematic;
    
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<PrometeoCarController>();
        
        // Set initial spawn point
        if (initialSpawnPoint != null)
        {
            lastCheckpointPosition = initialSpawnPoint.position;
            lastCheckpointRotation = initialSpawnPoint.rotation;
            hasCheckpoint = true;
        }
        else
        {
            // Use current position as initial spawn
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
            hasCheckpoint = true;
        }
        
        if (showDebugMessages)
        {
            Debug.Log("RespawnSystem: Press R to respawn at last checkpoint");
        }
    }
    
    void Update()
    {
        // Listen for R key press (only if not AI and not already frozen)
        if (!isFrozen && Input.GetKeyDown(KeyCode.R))
        {
            if (carController != null && carController.isAI)
            {
                // Don't allow manual respawn for AI
                return;
            }
            
            Respawn();
        }
    }
    
    // Called by CheckpointTrigger when passing through
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        hasCheckpoint = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"Checkpoint saved at {position}");
        }
    }
    
    // Respawn the car at the last checkpoint
    public void Respawn()
    {
        if (!hasCheckpoint)
        {
            Debug.LogWarning("RespawnSystem: No checkpoint to respawn at!");
            return;
        }
        
        if (isFrozen)
        {
            Debug.LogWarning("RespawnSystem: Already respawning!");
            return;
        }
        
        if (showDebugMessages)
        {
            Debug.Log("Respawning at checkpoint...");
        }
        
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        isFrozen = true;
        
        // Stop all velocity
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        
        // Teleport to checkpoint
        transform.position = lastCheckpointPosition;
        transform.rotation = lastCheckpointRotation;
        
        // Freeze the car
        wasKinematic = carRigidbody.isKinematic;
        carRigidbody.isKinematic = true;
        
        // Disable car controller temporarily
        if (carController != null)
        {
            carController.enabled = false;
        }
        
        if (showDebugMessages)
        {
            Debug.Log($"Car frozen for {freezeTime} seconds...");
        }
        
        // Wait for freeze time
        yield return new WaitForSeconds(freezeTime);
        
        // Unfreeze
        carRigidbody.isKinematic = wasKinematic;
        
        // Re-enable car controller
        if (carController != null)
        {
            carController.enabled = true;
        }
        
        isFrozen = false;
        
        if (showDebugMessages)
        {
            Debug.Log("Car unfrozen! You can move now.");
        }
    }
    
    // Public method to check if car is currently frozen
    public bool IsFrozen()
    {
        return isFrozen;
    }
}