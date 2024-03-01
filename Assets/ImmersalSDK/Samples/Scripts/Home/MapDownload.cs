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

public class MapDownload : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;

    protected List<JobAsync> m_Jobs = new List<JobAsync>();
    FirebaseFirestore db;
    FirebaseStorage firebase_storage;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        firebase_storage = FirebaseStorage.DefaultInstance;

        PlayerPrefs.SetString("email", "yourmom@gmail.com");
        DownloadMaps();
    }

    private async void DownloadMaps()
    {
        JobListJobsAsync j = new JobListJobsAsync();

        j.OnResult += (SDKJobsResult result) =>
        {
            List<SDKJob> jobList = new List<SDKJob>();
            foreach (SDKJob job in result.jobs)
            {
                if (job.type != (int)SDKJobType.Alignment)
                {
                    jobList.Add(job);
                }
            }

            PlayerPrefs.SetString("email", "mom@gmail.com");

            //make int array
            List<SDKJob> filteredJobs = new List<SDKJob>();
            List<int> firebaseMapsId = new List<int>();

            Query allMapsQuery = db.Collection("map");
            allMapsQuery
                .WhereEqualTo("email", PlayerPrefs.GetString("email"))
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
                        // Maps map = documentSnapshot.ConvertTo<Maps>();
                        GameObject item = Instantiate(listItemPrefab, listItemHolder);

                        if (mapData.ContainsKey("name") == false)
                        {
                            Debug.LogWarning("Map name");
                            continue;
                        }
                        item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = mapData[
                            "name"
                        ].ToString();

                        firebaseMapsId.Add(int.Parse(mapData["id"].ToString()));
                        foreach (SDKJob job in jobList)
                        {
                            foreach (int firebaseMapId in firebaseMapsId)
                            {
                                if (job.id == firebaseMapId)
                                {
                                    //add status of job here
                                    // filteredJobs.Add(job);

                                    Debug.Log("Job ID: " + job.status);
                                    Debug.Log("Job ID: " + job.id);
                                    Debug.Log("Job ID: " + firebaseMapId);
                                }
                            }
                        }

                        // Debug.Log("Map Name: " + map.Name);

                        //add error checking
                        // Extracting the text
                        if (mapData.ContainsKey("thumbnail_reference") == false)
                        {
                            Debug.LogWarning("Map thumbnail not found");
                            continue;
                        }

                        string image_ref_path = mapData["thumbnail_reference"] as string;
                        //string image_ref_path = map.mapThumbnail.ToString();

                        // fetch image from storage
                        // create a function to fetch image from storage
                        // create lambda expression
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
                                    StartCoroutine(
                                        DownloadImage(downloadUrl, OnTextureLoaded, item)
                                    );
                                }
                                else
                                {
                                    Debug.LogError("Failed to get download URL.");
                                }
                            });
                    }
                    Debug.Log("Successfully loaded Firestore Image documents.");
                });
        };

        await j.RunJobAsync();
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
                RawImage rawImage = obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
                rawImage.texture = downloadedTexture;
            }
        }
    }

    public async void Jobs()
    {
        JobListJobsAsync j = new JobListJobsAsync();

        // if (mapperSettings.listOnlyNearbyMaps)
        // {
        //     j.useGPS = true;
        //     j.latitude = m_Latitude;
        //     j.longitude = m_Longitude;
        //     j.radius = DefaultRadius;
        // }

        // foreach (int id in ARSpace.mapIdToMap.Keys)
        // {
        //     activeMaps.Add(id);
        // }

        j.OnResult += (SDKJobsResult result) =>
        {
            List<SDKJob> jobList = new List<SDKJob>();
            foreach (SDKJob job in result.jobs)
            {
                if (job.type != (int)SDKJobType.Alignment)
                {
                    jobList.Add(job);
                }
            }

            PlayerPrefs.SetString("email", "mom@gmail.com");

            //make int array
            List<SDKJob> filteredJobs = new List<SDKJob>();
            List<int> firebaseMapsId = new List<int>();
            //use firestore document fetch dont check dependency
            db = FirebaseFirestore.DefaultInstance;
            db.Collection("map")
                .WhereEqualTo("email", PlayerPrefs.GetString("email"))
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    QuerySnapshot snapshot = task.Result;
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Dictionary<string, object> map = document.ToDictionary();
                        Debug.Log(map["id"]);
                        if (map["email"].ToString() == PlayerPrefs.GetString("email"))
                        {
                            firebaseMapsId.Add(int.Parse(map["id"].ToString()));
                        }
                        foreach (SDKJob job in jobList)
                        {
                            foreach (int firebaseMapId in firebaseMapsId)
                            {
                                if (job.id == firebaseMapId)
                                {
                                    filteredJobs.Add(job);
                                }
                            }
                        }

                        // this.visualizeManager.SetMapListData(filteredJobs.ToArray(), activeMaps);
                    }
                });

            //only extract the job list data with matching ids with
        };

        await j.RunJobAsync();
    }
}
