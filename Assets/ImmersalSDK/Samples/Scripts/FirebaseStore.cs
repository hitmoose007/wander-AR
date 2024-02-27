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

public class FirebaseStore : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;

    FirebaseFirestore db;
    FirebaseStorage firebase_storage;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        firebase_storage = FirebaseStorage.DefaultInstance;

        Query allMapsQuery = db.Collection("maps");

        allMapsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching collection documents: " + task.Exception);
                return;
            }

            QuerySnapshot allMapsQuerySnapshot = task.Result;

            foreach(DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));

                Maps map = documentSnapshot.ConvertTo<Maps>();
                GameObject item = Instantiate(listItemPrefab, listItemHolder);
                item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = map.mapName.ToString();

                Dictionary<string, object> documentData = documentSnapshot.ToDictionary();
                
                //add error checking
                // Extracting the text
                if (documentData.ContainsKey("mapThumbnail") == false)
                {
                    Debug.LogWarning("Map thumbnail not found");
                    continue;
                }
                string image_ref_path = documentData["mapThumbnail"] as string;
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

                firebase_storage.GetReference(image_ref_path).GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
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

                // Dictionary<string, object> map = documentSnapshot.ToDictionary();
                // foreach (KeyValuePair<string, object> pair in map)
                // {
                //     Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                // }
                // // Newline to separate entries
                // Debug.Log(" ");
            }   
            Debug.Log("Successfully loaded Firestore Image documents.");
        });
    }
    // private void FetchAndDownloadImageContent()
    // {
    //     CollectionReference imageCollectionRef = db.Collection("image_content");

    //     //Debug.Log("imageCollectionRef:  lol" + imageCollectionRef);

    //     // Fetch all documents from the collection
    //     imageCollectionRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("Error fetching collection documents: " + task.Exception);
    //             return;
    //         }

    //         Debug.Log("task.Result:  lol for images" + task.Result);
    //         QuerySnapshot snapshot = task.Result;

    //         foreach (DocumentSnapshot document in snapshot.Documents)
    //         {
    //             if (document.Exists)
    //             {
    //                 Dictionary<string, object> documentData = document.ToDictionary();

    //                 //add error checking
    //                 // Extracting the text

    //                 if (documentData.ContainsKey("image_ref") == false)
    //                 {
    //                     Debug.LogWarning("image_ref not found");
    //                     continue;
    //                 }
    //                 string image_ref_path = documentData["image_ref"] as string;

    //                 //fetch image from storage
    //                 //create a function to fetch image from storage
    //                 //create lambda experssion
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

    //                 firebase_storage.GetReference(image_ref_path).GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
    //                 {
    //                     if (!task.IsFaulted && !task.IsCanceled)
    //                     {
    //                         string downloadUrl = task.Result.ToString();
    //                         // Proceed to download the image and convert it into a Texture2D
    //                         StartCoroutine(DownloadImage(downloadUrl, OnTextureLoaded));
    //                     }
    //                     else
    //                     {
    //                         Debug.LogError("Failed to get download URL.");
    //                     }
    //                 });
    //             }
    //         }
    //         Debug.Log("Successfully loaded Firestore Image documents.");
    //     });
    // }

    private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onTextureLoaded, GameObject obj)
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
}
