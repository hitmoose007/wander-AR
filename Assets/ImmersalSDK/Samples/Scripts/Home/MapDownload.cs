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
using Codice.Client.GameUI.Checkin;

public class MapDownload : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;

    FirebaseFirestore db;
    FirebaseStorage firebase_storage;

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

            //make int array
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
                            if (Int32.Parse(mapData["id"].ToString()) == 96408)
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
                                            StartCoroutine(
                                                DownloadImage(downloadUrl, OnTextureLoaded, item)
                                            );
                                        }
                                        else
                                        {
                                            Debug.LogError("Failed to get download URL.");
                                        }
                                    });

                                // Save job state to item
                                item.GetComponent<MapSelect>().jobState = job.status;

                                if (job.status == SDKJobState.Done)
                                {
                                    item.transform.GetChild(4).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Done";
                                    item.transform.GetChild(4).GetComponent<Image>().color = new Color32(61,150,56,161);
                                }
                                else if (job.status == SDKJobState.Failed)
                                {
                                    item.transform.GetChild(4).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Failed";
                                    item.transform.GetChild(4).GetComponent<Image>().color = new Color32(161,34,34,161);
                                   
                                }
                                else
                                {
                                    item.transform.GetChild(4).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Processing";
                                    item.transform.GetChild(4).GetComponent<Image>().color = new Color32(212,194,59,161);
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

    public async void FetchPublicMaps()
    {
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
                    GameObject item = Instantiate(listItemPrefab, listItemHolder);

                    // Setting name of map item to id of Firestore document
                    item.name = documentSnapshot.Id;
                    
                    // Setting status panel inactive for public maps 
                    // as they are always loaded
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
                                StartCoroutine(DownloadImage(downloadUrl, OnTextureLoaded, item));
                            }
                            else
                            {
                                Debug.LogError("Failed to get download URL.");
                            }
                        });

                    //save job state to item

                    Debug.Log("Successfully loaded Firestore Image document.");
                };
            });
        // };
        // await j.RunJobAsync();
    }

    // public async void FetchPrivateMaps()
    // {
    //     List<SDKJob> filteredJobs = new List<SDKJob>();
    //     List<int> firebaseMapsId = new List<int>();

    //     await db.Collection("map")
    //         .WhereEqualTo("private", true)
    //         .GetSnapshotAsync()
    //         .ContinueWithOnMainThread(task =>
    //         {
    //             if (task.IsFaulted)
    //             {
    //                 Debug.LogError("Error fetching collection documents: " + task.Exception);
    //                 return;
    //             }

    //             QuerySnapshot allMapsQuerySnapshot = task.Result;

    //             foreach (DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
    //             {
    //                 Dictionary<string, object> mapData = documentSnapshot.ToDictionary();
    //                 GameObject item = Instantiate(listItemPrefab, listItemHolder);

    //                 if (mapData.ContainsKey("name") == false)
    //                 {
    //                     Debug.LogWarning("Map name");
    //                     continue;
    //                 }
    //                 item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = mapData[
    //                     "name"
    //                 ].ToString();

    //                 item.GetComponent<MapSelect>().mapId = int.Parse(mapData["id"].ToString());

    //                 if (mapData.ContainsKey("thumbnail_reference") == false)
    //                 {
    //                     Debug.LogWarning("Map thumbnail not found");
    //                     continue;
    //                 }

    //                 string image_ref_path = mapData["thumbnail_reference"] as string;
    //                 Texture2D texture = null;
    //                 Action<Texture2D> OnTextureLoaded = (Texture2D newTexture) =>
    //                 {
    //                     texture = newTexture; // Assign the new texture to your original texture variable
    //                     if (texture == null)
    //                     {
    //                         Debug.Log("Couldn't load texture from " + image_ref_path);
    //                         return;
    //                     }
    //                 };

    //                 firebase_storage
    //                     .GetReference(image_ref_path)
    //                     .GetDownloadUrlAsync()
    //                     .ContinueWithOnMainThread(task =>
    //                     {
    //                         if (!task.IsFaulted && !task.IsCanceled)
    //                         {
    //                             string downloadUrl = task.Result.ToString();
    //                             // Proceed to download the image and convert it into a Texture2D
    //                             StartCoroutine(DownloadImage(downloadUrl, OnTextureLoaded, item));
    //                         }
    //                         else
    //                         {
    //                             Debug.LogError("Failed to get download URL.");
    //                         }
    //                     });

    //                 //save job state to item

    //                 Debug.Log("Successfully loaded Firestore Image documents.");
    //             }
    //             // }
    //             ;
    //         });
    //     // };
    //     // await j.RunJobAsync();
    // }

    public void DeleteMap()
    {
        Debug.Log( GetType() + "-" + this.transform.parent.name + "-OnDelete();");

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
                    RawImage rawImage = obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
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
