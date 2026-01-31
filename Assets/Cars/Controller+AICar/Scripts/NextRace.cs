using System.Collections;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextRace : MonoBehaviour
{
    public int nextSceneIndex;
    private PrometeoWaypointAI carAI;
    public TMPro.TextMeshPro progressText;

    private AsyncOperation load;

    void Awake()
    {
        CrashReportHandler.enableCaptureExceptions = false;
    }
    void Start()
    {
        carAI = GetComponent<PrometeoWaypointAI>();
        load = SceneManager.LoadSceneAsync(nextSceneIndex);
        load.allowSceneActivation = false;
    }

    void Update()
    {
        if (progressText != null)
            progressText.text = "Loading: " + (load.progress * 100).ToString() + "%";
        if (carAI == null)
        {
            Debug.Log("Blah");
            if (load.progress >= 0.89999)
                load.allowSceneActivation = true;
        }
        else
        {
            if (carAI.IsComplete() && load.progress >= 0.89999)
                load.allowSceneActivation = true;
        }
    }

}
