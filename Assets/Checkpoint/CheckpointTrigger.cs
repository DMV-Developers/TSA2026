using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex = 0; // Used to order checkpoints
    public Color gizmoColor = Color.green;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if a car with RespawnSystem entered
        RespawnSystem respawnSystem = other.GetComponent<RespawnSystem>();
        if (respawnSystem != null)
        {
            respawnSystem.SetCheckpoint(transform.position, transform.rotation);
            Debug.Log($"Checkpoint {checkpointIndex} reached!");
        }
    }
    
    // Visualize checkpoint in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Checkpoint {checkpointIndex}");
#endif
    }
}