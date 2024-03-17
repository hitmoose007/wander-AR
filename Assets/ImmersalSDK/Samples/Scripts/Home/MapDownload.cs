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

    FirebaseFirestore db;
    FirebaseStorage firebase_storage;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        firebase_storage = FirebaseStorage.DefaultInstance;

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

            //make int array
            List<SDKJob> filteredJobs = new List<SDKJob>();
            List<int> firebaseMapsId = new List<int>();

            Query allMapsQuery = db.Collection("map");
            allMapsQuery
                .WhereEqualTo("email", StaticData.userEmail)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(async task =>
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
                        foreach (SDKJob job in jobList)
                        {
                            if (job.id == Int32.Parse(mapData["id"].ToString()))
                            {
                                // Maps map = documentSnapshot.ConvertTo<Maps>();
                                GameObject item = Instantiate(listItemPrefab, listItemHolder);

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

                                await firebase_storage
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
                                //add status of job here
                                // filteredJobs.Add(job);




                                if (bool.Parse(mapData["copied"].ToString()) == false)
                                {
                                    if (job.status == SDKJobState.Done)
                                    {
                                        Debug.Log("teri tutti");

                                        JobCopyMapAsync copyJob = new JobCopyMapAsync();
                                        if (StaticData.MainAccountDeveloperToken == null)
                                        {
                                            Debug.Log("main account token is null");
                                        }

                                        Debug.Log(
                                            "main account token: "
                                                + StaticData.MainAccountDeveloperToken
                                        );
                                        Debug.Log("developer token: " + StaticData.developerToken);
                                        copyJob.id = job.id;
                                        copyJob.login = StaticData.MainAccountDeveloperToken; //where you send to the main account
                                        copyJob.token = StaticData.developerToken; //from where you send

                                        copyJob.OnResult += (SDKCopyMapResult copyResult) =>
                                        {
                                            Debug.LogFormat("Map {0} copied successfully.", job.id);

                                            DocumentReference mapRef = db.Collection("map")
                                                .Document(documentSnapshot.Id);
                                            Dictionary<string, object> updates = new Dictionary<
                                                string,
                                                object
                                            >
                                            {
                                                { "copied", true }
                                            };
                                            mapRef
                                                .UpdateAsync(updates)
                                                .ContinueWithOnMainThread(task =>
                                                {
                                                    if (task.IsFaulted)
                                                    {
                                                        Debug.LogError(
                                                            "Error updating document: "
                                                                + task.Exception
                                                        );
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("Map copied: " + mapRef.Id);
                                                    }
                                                });

                                            Debug.Log("Map copied: " + mapRef.Id);
                                        };
                                        await copyJob.RunJobAsync();
                                        //wait for 1 second
                                    }
                                }
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

            //make int array
            List<SDKJob> filteredJobs = new List<SDKJob>();
            List<int> firebaseMapsId = new List<int>();
            //use firestore document fetch dont check dependency
            db = FirebaseFirestore.DefaultInstance;
            db.Collection("map")
                .WhereEqualTo("email", StaticData.userEmail)
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
