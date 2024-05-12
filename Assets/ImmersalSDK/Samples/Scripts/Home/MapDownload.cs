using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.Networking;
using System;
using TMPro;
using UnityEngine.UI;
using Immersal.REST;
using UnityEngine.Android;
using UnityEngine.Events;
using Immersal.Samples.Mapping;
using Unity.XR.CoreUtils;
#if PLATFORM_ANDROID
#endif
public class MapDownload : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;

    FirebaseFirestore db;
    FirebaseStorage firebase_storage;

    double currentLatitude;
    double currentLongitude;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        firebase_storage = FirebaseStorage.DefaultInstance;
    }

    void Update()
    {
        UpdateLocation();
    }

    public async void DownloadMaps()
    {
        JobListJobsAsync j = new JobListJobsAsync();

        j.OnResult += async (SDKJobsResult result) =>
        {
            List<SDKJob> jobList = new List<SDKJob>();
            foreach (SDKJob job in result.jobs)
            {
                if (job.type != (int)SDKJobType.Alignment)
                {
                    jobList.Add(job);
                }
            }

            // Make int array
            List<SDKJob> filteredJobs = new List<SDKJob>();
            List<int> firebaseMapsId = new List<int>();

            await db.Collection("map")
                .WhereEqualTo("email", StaticData.userEmail)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Error fetching collection documents: " + task.Exception);
                        return;
                    }

                    QuerySnapshot allMapsQuerySnapshot = task.Result;

                    foreach (DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
                    {
                        Dictionary<string, object> mapData = documentSnapshot.ToDictionary();

                        Debug.Log("mapData[id]: " + int.Parse(mapData["id"].ToString()));

                        foreach (SDKJob job in jobList)
                        {
                            Debug.Log("job.id: " + job.id);

                            //if (job.id == Int32.Parse(mapData["id"].ToString()))
                            if (Int32.Parse(mapData["id"].ToString()) == job.id)
                            {
                                // Maps map = documentSnapshot.ConvertTo<Maps>();
                                GameObject item = Instantiate(listItemPrefab, listItemHolder);

                                // Setting name of map item to id of Firestore document
                                item.name = documentSnapshot.Id;

                                if (mapData.ContainsKey("name") == false)
                                {
                                    Debug.LogWarning("Map name");
                                    continue;
                                }
                                item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    mapData["name"].ToString();

                                item.GetComponent<MapSelect>().mapId = int.Parse(
                                    mapData["id"].ToString()
                                );

                                if (mapData.ContainsKey("thumbnail_reference") == false)
                                {
                                    Debug.LogWarning("Map thumbnail not found");
                                    continue;
                                }

                                string image_ref_path = mapData["thumbnail_reference"] as string;
                                Texture2D texture = null;
                                Action<Texture2D> OnTextureLoaded = (Texture2D newTexture) =>
                                {
                                    texture = newTexture; // Assign the new texture to your original texture variable
                                    if (texture == null)
                                    {
                                        Debug.Log("Couldn't load texture from " + image_ref_path);
                                        return;
                                    }
                                };

                                _ = firebase_storage
                                    .GetReference(image_ref_path)
                                    .GetDownloadUrlAsync()
                                    .ContinueWithOnMainThread(task =>
                                    {
                                        if (!task.IsFaulted && !task.IsCanceled)
                                        {
                                            string downloadUrl = task.Result.ToString();
                                            // Proceed to download the image and convert it into a Texture2D
                                            if (gameObject.activeInHierarchy)
                                            {
                                                StartCoroutine(
                                                    DownloadImage(
                                                        downloadUrl,
                                                        OnTextureLoaded,
                                                        item
                                                    )
                                                );
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError("Failed to get download URL.");
                                        }
                                    });

                                // Save job state to item
                                item.GetComponent<MapSelect>().jobState = job.status;
                                item.GetComponent<MapSelect>().isMapPrivate = true;

                                if (job.status == SDKJobState.Done)
                                {
                                    item.transform
                                        .GetChild(4)
                                        .transform.GetChild(0)
                                        .GetComponent<TextMeshProUGUI>()
                                        .text = "Done";
                                    item.transform.GetChild(4).GetComponent<Image>().color =
                                        new Color32(61, 150, 56, 161);
                                }
                                else if (job.status == SDKJobState.Failed)
                                {
                                    item.transform
                                        .GetChild(4)
                                        .transform.GetChild(0)
                                        .GetComponent<TextMeshProUGUI>()
                                        .text = "Failed";
                                    item.transform.GetChild(4).GetComponent<Image>().color =
                                        new Color32(161, 34, 34, 161);
                                }
                                else
                                {
                                    item.transform
                                        .GetChild(4)
                                        .transform.GetChild(0)
                                        .GetComponent<TextMeshProUGUI>()
                                        .text = "Processing";
                                    item.transform.GetChild(4).GetComponent<Image>().color =
                                        new Color32(212, 194, 59, 161);
                                }
                                Debug.Log("Successfully loaded Firestore Image document.");
                            }
                        }

                        // Debug.Log("Map Name: " + map.Name);

                        //add error checking
                        // Extracting the text
                        //string image_ref_path = map.mapThumbnail.ToString();

                        // fetch image from storage
                        // create a function to fetch image from storage
                        // create lambda expression
                    }
                });
        };

        await j.RunJobAsync();
    }

    public void RequestLocationPermissionAndFetchPublicMapsAsync()
    {
        // Check if the user has location service enabled
        // if (!Input.location.isEnabledByUser)
        // {
        //     Debug.LogWarning("Location service is not enabled by user.");
        //     return;
        // }

        // // Request user permission for location
        // if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        // {
        //     Permission.RequestUserPermission(Permission.FineLocation);

        //     // Wait until the user grants or denies permission
        //     while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        //     {
        //         await System.Threading.Tasks.Task.Yield();
        //     }
        // }

        // // Check if the user granted permission
        // if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        // {
        //     Debug.LogError("User denied location permission.");
        //     return;
        // }

        // // Start location service
        // Input.location.Start();

        // // Wait until the location service initializes
        // int maxWait = 20;
        // while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        // {
        //     await System.Threading.Tasks.Task.Delay(1000);
        //     maxWait--;
        // }

        // // Check if the location service initialized successfully
        // if (maxWait < 1)
        // {
        //     Debug.LogError("Timed out waiting for location service to initialize.");
        //     return;
        // }

        // // Check if the location service started successfully
        // if (Input.location.status == LocationServiceStatus.Failed)
        // {
        //     Debug.LogError("Unable to determine device location.");
        //     return;
        // }

        // // Retrieve the device's current location
        // LocationInfo locationInfo = Input.location.lastData;
        // Debug.Log("Location: " + locationInfo.latitude + ", " + locationInfo.longitude);

        // Fetch public maps after getting the current location
        StartGPS();
        FetchPublicMaps();
    }

    public async void FetchPublicMaps()
    {
        Debug.Log("Fetching public maps");
        //get user location

        double latDelta = 0.001; // approx 1.11 km per degree latitude
        double lngDelta = 0.001; // approx 0.7 km per degree longitude (varie

        List<SDKJob> filteredJobs = new List<SDKJob>();
        List<int> firebaseMapsId = new List<int>();

        await db.Collection("map")
            .WhereEqualTo("private", false)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error fetching collection documents: " + task.Exception);
                    return;
                }

                QuerySnapshot allMapsQuerySnapshot = task.Result;

                foreach (DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
                {
                    Dictionary<string, object> mapData = documentSnapshot.ToDictionary();

                    // Check if the latitude and longitude keys exist in the mapData dictionary
                    if (mapData.ContainsKey("latitude") && mapData.ContainsKey("longitude"))
                    {
                        double mapLatitude = Convert.ToDouble(mapData["latitude"]);
                        double mapLongitude = Convert.ToDouble(mapData["longitude"]);

                        double maxLatitude = m_Latitude + latDelta;
                        double minLatitude = m_Latitude - latDelta;

                        double maxLongitude = m_Longitude + lngDelta;
                        double minLongitude = m_Longitude - lngDelta;

                        // Check if the map's latitude and longitude are within the delta range
                        // if (
                        //     mapLatitude >= minLatitude
                        //     && mapLatitude <= maxLatitude
                        //     && mapLongitude >= minLongitude
                        //     && mapLongitude <= maxLongitude
                        // )
                        //{
                        // The map is within the specified range, proceed with further processing
                        GameObject item = Instantiate(listItemPrefab, listItemHolder);

                        // Setting name of map item to id of Firestore document
                        item.name = documentSnapshot.Id;

                        GameObject statusPanel = item.transform.GetChild(4).gameObject;
                        statusPanel.SetActive(false);

                        if (mapData.ContainsKey("name") == false)
                        {
                            Debug.LogWarning("Map name");
                            continue;
                        }
                        item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = mapData[
                            "name"
                        ].ToString();

                        item.GetComponent<MapSelect>().mapId = int.Parse(mapData["id"].ToString());

                        if (mapData.ContainsKey("thumbnail_reference") == false)
                        {
                            Debug.LogWarning("Map thumbnail not found");
                            continue;
                        }

                        string image_ref_path = mapData["thumbnail_reference"] as string;
                        Texture2D texture = null;
                        Action<Texture2D> OnTextureLoaded = (Texture2D newTexture) =>
                        {
                            texture = newTexture; // Assign the new texture to your original texture variable
                            if (texture == null)
                            {
                                Debug.Log("Couldn't load texture from " + image_ref_path);
                                return;
                            }
                        };

                        firebase_storage
                            .GetReference(image_ref_path)
                            .GetDownloadUrlAsync()
                            .ContinueWithOnMainThread(task =>
                            {
                                if (!task.IsFaulted && !task.IsCanceled)
                                {
                                    string downloadUrl = task.Result.ToString();
                                    // Proceed to download the image and convert it into a Texture2D
                                    if (gameObject.activeInHierarchy)
                                    {
                                        StartCoroutine(
                                            DownloadImage(downloadUrl, OnTextureLoaded, item)
                                        );
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Failed to get download URL.");
                                }
                            });

                        //save job state to item
                        item.GetComponent<MapSelect>().isMapPrivate = false;
                        item.transform.GetChild(2).gameObject.SetActive(false);
                        item.GetComponent<MapSelect>().jobState = SDKJobState.Done;

                        Debug.Log("Successfully loaded Firestore Image document.");
                        //}
                    }
                }
            });
    }

    public void DeleteMap()
    {
        Debug.Log(GetType() + "-" + this.transform.parent.name + "-OnDelete();");

        DocumentReference mapRef = db.Collection("map").Document(this.transform.parent.name);
        mapRef.DeleteAsync();

        Destroy(this.transform.parent.gameObject);
    }

    private IEnumerator DownloadImage(
        string imageUrl,
        Action<Texture2D> onTextureLoaded,
        GameObject obj
    )
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download image: " + www.error);
            }
            else
            {
                // Create a Texture
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(www);
                onTextureLoaded?.Invoke(downloadedTexture); // Invoke the callback

                // Here you can use the texture, for example, apply it to a GameObject to display the image
                // Example: yourGameObject.GetComponent<Renderer>().material.mainTexture = texture;
                if (obj != null)
                {
                    RawImage rawImage = obj.transform
                        .GetChild(0)
                        .GetChild(0)
                        .GetComponent<RawImage>();
                    rawImage.texture = downloadedTexture;
                }
            }
        }
    }

    public UnityEvent onConnect = null;
    public UnityEvent onFailedToConnect = null;
    public UnityEvent onImageLimitExceeded = null;

    public MapperStats stats { get; protected set; } = new MapperStats();

    protected double m_Latitude = 0.0;
    protected double m_Longitude = 0.0;
    protected double m_Altitude = 0.0;
    protected double m_Haccuracy = 0.0;
    protected double m_Vaccuracy = 0.0;
    protected double m_VLatitude = 0.0;
    protected double m_VLongitude = 0.0;
    protected double m_VAltitude = 0.0;

    public bool gpsOn
    {
        get { return Input.location.status == LocationServiceStatus.Running; }
    }

    public string tempImagePath
    {
        get { return string.Format("{0}/Images", Application.persistentDataPath); }
    }

    // public void OnGPSToggleChanged(bool value)
    // {
    //     if (value)
    //     {
    //         Invoke("StartGPS", 0.1f);
    //     }
    //     else
    //     {
    //         Invoke("StopGPS", 0.1f);
    //     }
    // }

#if PLATFORM_ANDROID
    private IEnumerator WaitForLocationPermission()
    {
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
        }

        Debug.Log("Location permission OK");
        StartCoroutine(EnableLocationServices());
        yield return null;
    }
#endif

    public void StartGPS()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("Location permission OK");
            StartCoroutine(EnableLocationServices());
        }
        else
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            StartCoroutine(WaitForLocationPermission());
        }
    }

    public void StopGPS()
    {
        Input.location.Stop();
        NotificationManager.Instance.GenerateNotification("Geolocation tracking stopped");
    }

    private IEnumerator EnableLocationServices()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location services not enabled");
            yield break;
        }

        // Start service before querying location
        Input.location.Start(0.001f, 0.001f);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            NotificationManager.Instance.GenerateNotification("Location services timed out");
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            Debug.Log("tracking location");
        }
    }

    void UpdateLocation()
    {
        if (gpsOn)
        {
            m_Latitude = Input.location.lastData.latitude;
            m_Longitude = Input.location.lastData.longitude;
            m_Altitude = Input.location.lastData.altitude;
            m_Haccuracy = Input.location.lastData.horizontalAccuracy;
            m_Vaccuracy = Input.location.lastData.verticalAccuracy;
        }
    }
}
