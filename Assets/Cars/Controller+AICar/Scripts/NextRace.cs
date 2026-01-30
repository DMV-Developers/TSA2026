using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextRace : MonoBehaviour
{

    private PrometeoWaypointAI waypointAI;

    void Start()
    {
        waypointAI = GetComponent<PrometeoWaypointAI>();
    }

    void Update()
    {
        if (waypointAI.IsComplete())
        {
            SceneManager.LoadScene(1);
        }
    }
}
