using UnityEngine;
using Firebase;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ActivateObject : MonoBehaviour
{
    public GameObject ContentUI;
    public GameObject NavUI;
    public GameObject WaypointUI;
    private Queue<System.Action> actionsToRunOnMainThread = new Queue<System.Action>();

    public bool activateForEditor = false;

    void Start()
    {
        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Firebase.DependencyStatus dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        // Firebase is ready for use, enqueue any actions that need to run on the main thread
                        EnqueueActionOnMainThread(() =>
                        {
                            if (activateForEditor)
                            {
                                ActivateContentUI();
                                ActivateNavUI();
                                ActivateWaypointUI();
                                // Optionally activate other UI elements as needed
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError(
                            $"Could not resolve all Firebase dependencies: {dependencyStatus}"
                        );
                        // Handle the case where Firebase is not available
                    }
                }
            });
    }

    void Update()
    {
        // Execute all actions queued to run on the main thread
        while (actionsToRunOnMainThread.Count > 0)
        {
            actionsToRunOnMainThread.Dequeue().Invoke();
        }

        //check if ar space exists
    }

    // Enqueues an action to be performed on the main thread
    private void EnqueueActionOnMainThread(System.Action action)
    {
        lock (actionsToRunOnMainThread)
        {
            actionsToRunOnMainThread.Enqueue(action);
        }
    }

    public void ActivateContentUI()
    {
        ContentUI.SetActive(true);
    }

    public void ActivateNavUI()
    {
        NavUI.SetActive(true);
    }

    public void ActivateWaypointUI()
    {
        WaypointUI.SetActive(true);
    }
}
