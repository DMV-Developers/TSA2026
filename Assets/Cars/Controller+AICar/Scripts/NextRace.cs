using System.Collections;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextRace : MonoBehaviour
{
    public int nextSceneIndex;
    private PrometeoWaypointAI carAI;

    private AsyncOperation load;

    void Awake()
    {
        CrashReportHandler.enableCaptureExceptions = false;
    }
    void Start()
    {
        carAI = GetComponent<PrometeoWaypointAI>();
    }

    void Update()
    {
        if (carAI.IsComplete())
        {
                load = SceneManager.LoadSceneAsync(nextSceneIndex);
                load.allowSceneActivation = true;
        }
    }

}

