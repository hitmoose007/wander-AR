using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance = null;

    public GameObject DownloadMaps;

    // Public static property to access instance
    public static FirebaseManager Instance
    {
        get
        {
#if UNITY_EDITOR
            // Find existing instance in the editor if not playing and instance is null
            if (instance == null && !Application.isPlaying)
            {
                instance = FindObjectOfType<FirebaseManager>();
            }
#endif
            // Log error if instance is still null when accessed
            if (instance == null)
            {
                Debug.LogError(
                    "No FirebaseManager instance found. Ensure one exists in the scene."
                );
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: Only if you want it to persist across scene changes
            InitializeFirebase();
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Ensures that only one instance exists
        }
    }

    private void InitializeFirebase()
    {
        //player preferences set map id
        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Firebase is ready for use
                    //debug if home ui is active
                    Debug.Log("heallo");
                    DownloadMaps.SetActive(true);
                    Debug.Log("heallo poop");
                    //check the status
                    Debug.Log(DownloadMaps.activeSelf);
                    Debug.Log("Firebase is ready for use");
                    // Initialize other Firebase features here if necessary
                }
                else
                {
                    Debug.LogError(
                        $"Could not resolve all Firebase dependencies: {dependencyStatus}"
                    );
                }
            });
    }
}
