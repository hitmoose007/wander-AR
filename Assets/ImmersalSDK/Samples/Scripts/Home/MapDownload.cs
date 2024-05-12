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

    public async void RequestLocationPermissionAndFetchPublicMapsAsync()
    {
        // Check if the user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location service is not enabled by user.");
            return;
        }

        // Request user permission for location
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            // Wait until the user grants or denies permission
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }

        // Check if the user granted permission
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.LogError("User denied location permission.");
            return;
        }

        // Start location service
        Input.location.Start();

        // Wait until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            await System.Threading.Tasks.Task.Delay(1000);
            maxWait--;
        }

        // Check if the location service initialized successfully
        if (maxWait < 1)
        {
            Debug.LogError("Timed out waiting for location service to initialize.");
            return;
        }

        // Check if the location service started successfully
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location.");
            return;
        }

        // Retrieve the device's current location
        LocationInfo locationInfo = Input.location.lastData;
        Debug.Log("Location: " + locationInfo.latitude + ", " + locationInfo.longitude);

        // Fetch public maps after getting the current location
        FetchPublicMaps();
    }

    public async void FetchPublicMaps()
    {
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

                        double maxLatitude = currentLatitude + latDelta;
                        double minLatitude = currentLatitude- latDelta;

                        double maxLongitude = currentLongitude+ lngDelta;
                        double minLongitude = currentLongitude- lngDelta;

                        // Check if the map's latitude and longitude are within the delta range
                        if (
                            mapLatitude >= minLatitude
                            && mapLatitude <= maxLatitude
                            && mapLongitude >= minLongitude
                            && mapLongitude <= maxLongitude
                        )
                        {
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
                            item.GetComponent<MapSelect>().jobState = SDKJobState.Done;

                            Debug.Log("Successfully loaded Firestore Image document.");
                        }
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

    // public async void Jobs()
    // {
    //     JobListJobsAsync j = new JobListJobsAsync();

    //     // if (mapperSettings.listOnlyNearbyMaps)
    //     // {
    //     //     j.useGPS = true;
    //     //     j.latitude = m_Latitude;
    //     //     j.longitude = m_Longitude;
    //     //     j.radius = DefaultRadius;
    //     // }

    //     // foreach (int id in ARSpace.mapIdToMap.Keys)
    //     // {
    //     //     activeMaps.Add(id);
    //     // }

    //     j.OnResult += (SDKJobsResult result) =>
    //     {
    //         List<SDKJob> jobList = new List<SDKJob>();
    //         foreach (SDKJob job in result.jobs)
    //         {
    //             if (job.type != (int)SDKJobType.Alignment)
    //             {
    //                 jobList.Add(job);
    //             }
    //         }

    //         //make int array
    //         List<SDKJob> filteredJobs = new List<SDKJob>();
    //         List<int> firebaseMapsId = new List<int>();
    //         //use firestore document fetch dont check dependency
    //         db = FirebaseFirestore.DefaultInstance;
    //         db.Collection("map")
    //             .WhereEqualTo("email", StaticData.userEmail)
    //             .GetSnapshotAsync()
    //             .ContinueWithOnMainThread(task =>
    //             {
    //                 QuerySnapshot snapshot = task.Result;
    //                 foreach (DocumentSnapshot document in snapshot.Documents)
    //                 {
    //                     Dictionary<string, object> map = document.ToDictionary();
    //                     Debug.Log(map["id"]);
    //                     if (map["email"].ToString() == PlayerPrefs.GetString("email"))
    //                     {
    //                         firebaseMapsId.Add(int.Parse(map["id"].ToString()));
    //                     }
    //                     foreach (SDKJob job in jobList)
    //                     {
    //                         foreach (int firebaseMapId in firebaseMapsId)
    //                         {
    //                             if (job.id == firebaseMapId)
    //                             {
    //                                 filteredJobs.Add(job);
    //                             }
    //                         }
    //                     }

    //                     // this.visualizeManager.SetMapListData(filteredJobs.ToArray(), activeMaps);
    //                 }
    //             });

    //         //only extract the job list data with matching ids with
    //     };

    //     await j.RunJobAsync();
    // }
}
