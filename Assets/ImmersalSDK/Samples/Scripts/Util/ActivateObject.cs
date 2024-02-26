using UnityEngine;
using Firebase;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;

public class ActivateObject : MonoBehaviour
{
    public GameObject ContentUI;
    private GameObject NavUI;
    private GameObject WaypointUI;
    private Queue<System.Action> actionsToRunOnMainThread = new Queue<System.Action>();

    public bool activateForEditor = false;

    private bool done = false;

    private bool done2;

    void Start()
    {
        done2 = false;
        Debug.Log("ActivateObjectttt Start");
        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("ActivateObject Start: CheckAndFixDependenciesAsync");
                if (task.IsCompleted)
                {
                    Firebase.DependencyStatus dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        Debug.Log("irebase is ready for use");
                        // ActivateContentUI();
                        // ActivateContentUI();
                        // ContentUI.SetActive(true);
                        Debug.Log("ActivateObject Start: Firebase is ready for use");

                        // Firebase is ready for use, enqueue any actions that need to run on the main thread
                        // EnqueueActionOnMainThread(() =>
                        // {
                        //     // if (activateForEditor)
                        //     // {
                        //     Debug.Log("ActivateObject Start: activateForEditor");
                        //     ActivateContentUI();
                        //     ActivateNavUI();
                        //     ActivateWaypointUI();
                        //     // Optionally activate other UI elements as needed
                        //     // }
                        // });
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
        // Debug.Log("ActivateObject Update");
        // Execute all actions queued to run on the main thread
        // Debug.Log(
        //     "ActivateObject Update: actionsToRunOnMainThread.Count: "
        //         + actionsToRunOnMainThread.Count
        // );
        // Debug.Log("ActivateObject Update: done2: " + done2);
        while (actionsToRunOnMainThread.Count > 0 )
        {
            Debug.Log("ActivateObject Update: actionsToRunOnMainThread.Count > 0");
            actionsToRunOnMainThread.Dequeue().Invoke();
        }

        //check if ar space exists
        // if (GameObject.Find("AR Space") != null)
        // {
        //     ActivateContentUI();
        //     ActivateNavUI();
        //     ActivateWaypointUI();
        // }
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
        //que action to run on main thread
        EnqueueActionOnMainThread(() =>
        {
            Debug.Log("ActivateObject ActivateContentUI: EnqueueActionOnMainThread");
            ContentUI.SetActive(true);
        });

        while (actionsToRunOnMainThread.Count > 0)
        {
            Debug.Log("ActivateObject Update: actionsToRunOnMainThread.Count > 0");
            actionsToRunOnMainThread.Dequeue().Invoke();
        }
    }

    public void ActivateNavUI()
    {
        NavUI.SetActive(true);
    }

    public void ActivateWaypointUI()
    {
        WaypointUI.SetActive(true);
    }

    public void Done2()
    {
        Debug.Log("Done2");
        done2 = true;
        Debug.Log("ActivateObject Done2: done2: " + done2);

        while (actionsToRunOnMainThread.Count > 0 && done2)
        {
            Debug.Log("ActivateObject Update: actionsToRunOnMainThread.Count > 0");
            actionsToRunOnMainThread.Dequeue().Invoke();
        }
    }
}
