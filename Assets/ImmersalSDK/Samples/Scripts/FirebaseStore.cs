// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase.Firestore;
// using Firebase.Storage;
// using Firebase.Extensions;

// public class FirebaseStore : MonoBehaviour
// {
//     FirebaseStorage firebase_storage;
//     void Start()
//     {
//         firebase_storage.GetReference(image_ref_path).GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
//         {
//             if (!task.IsFaulted && !task.IsCanceled)
//             {
//                 string downloadUrl = task.Result.ToString();
//                 // Proceed to download the image and convert it into a Texture2D
//                 StartCoroutine(DownloadImage(downloadUrl, OnTextureLoaded));
//             }
//             else
//             {
//                 Debug.LogError("Failed to get download URL.");
//             }
//         });
//     }

//     private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onTextureLoaded)
//     {
//         using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
//         {
//             yield return www.SendWebRequest();
//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Failed to download image: " + www.error);
//             }
//             else
//             {
//                 // Create a Texture
//                 Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(www);
//                 onTextureLoaded?.Invoke(downloadedTexture); // Invoke the callback
//                 // Here you can use the texture, for example, apply it to a GameObject to display the image
//                 // Example: yourGameObject.GetComponent<Renderer>().material.mainTexture = texture;
//             }
//         }
//     }
// }

