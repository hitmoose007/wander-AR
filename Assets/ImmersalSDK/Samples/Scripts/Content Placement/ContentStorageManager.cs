/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.IO;
using TMPro;
using NativeGalleryNamespace;
using Firebase;
using Firebase.Firestore;
using Firebase.Storage;

using Immersal.Samples.Util;
using Firebase.Extensions;
using System.Collections;
using TMPro; // Add this line to import the TextMesh Pro namespace


namespace Immersal.Samples.ContentPlacement
{
    public class ContentStorageManager : MonoBehaviour
    {
        [HideInInspector]
        public List<MovableContent> contentList = new List<MovableContent>();

        [SerializeField]
        private GameObject m_TextContentPrefab = null;

        [SerializeField]
        private GameObject quadPrefab;

        // private DatabaseReference db_reference;

        [SerializeField]
        private Immersal.AR.ARSpace m_ARSpace;

        private FirebaseFirestore db;

        private FirebaseStorage firebase_storage;

        [SerializeField]
        private string m_Filename = "contentttt.json";
        private Savefile m_Savefile;
        private List<Vector3> m_Positions = new List<Vector3>();
        private List<string> m_Texts = new List<string>(); // Create list for text

        // create list for font style, color and font
        private List<Color> m_TextColors = new List<Color>();
        // private List<TMP_FontStyle> m_FontStyles = new List<TMP_FontStyle>();
        private List<TMP_FontAsset> m_Fonts = new List<TMP_FontAsset>();


        [System.Serializable]
        public struct Savefile
        {
            public List<Vector3> positions;
            public List<string> texts;
            public List<Color> textColors; // List for text color
            // public List<TMP_FontStyle> fontStyles; // List for font style
            public List<TMP_FontAsset> fonts; // List for font

        }

        public static ContentStorageManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null && !Application.isPlaying)
                {
                    instance = UnityEngine.Object.FindObjectOfType<ContentStorageManager>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError(
                        "No ContentStorageManager instance found. Ensure one exists in the scene."
                    );
                }
                return instance;
            }
        }

        private static ContentStorageManager instance = null;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one ContentStorageManager object in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }

            if (m_ARSpace == null)
            {
                m_ARSpace = GameObject.FindObjectOfType<Immersal.AR.ARSpace>();
            }

            db = FirebaseFirestore.DefaultInstance;

            firebase_storage = FirebaseStorage.DefaultInstance;
            ///check firebase intialized
        }

        private void Start()
        {
            string directoryPath = Application.persistentDataPath;

            // if (Directory.Exists(directoryPath))
            // {
            //     // Delete all files in the directory
            //     foreach (var file in Directory.GetFiles(directoryPath))
            //     {
            //         File.Delete(file);
            //     }

            //     Debug.Log("All persistent data deleted.");
            // }
            // else
            // {
            //     Debug.LogWarning("Directory does not exist: " + directoryPath);
            // }
            // contentList.Clear();

            LoadContents();
        }

        public void AddContent()
        {
            Transform cameraTransform = Camera.main.transform;
            GameObject go = Instantiate(
                m_TextContentPrefab,
                cameraTransform.position + cameraTransform.forward,
                cameraTransform.rotation,
                m_ARSpace.transform
            );
            // go.AddComponent<TextMeshPro>();
            go.GetComponent<TextMeshPro>().text = m_TextContentPrefab
                .GetComponent<TextMeshPro>()
                .text;
            //get the text and save it in a new variable
            //  go.GetComponent<TextMeshPro>().text;
        }

        public void DeleteAllContent()
        {
            List<MovableContent> copy = new List<MovableContent>();

            foreach (MovableContent content in contentList)
            {
                copy.Add(content);
            }

            foreach (MovableContent content in copy)
            {
                content.RemoveContent();
            }
        }

        public void SaveContents()
        {
            m_Positions.Clear();
            m_Texts.Clear();
            List<Color> textColors = new List<Color>(); // Initialize list for text color
            // List<TMP_FontStyle> fontStyles = new List<TMP_FontStyle>(); // Initialize list for font style
            List<TMP_FontAsset> fonts = new List<TMP_FontAsset>(); // Initialize list for font

            foreach (MovableContent content in contentList)
            {
                m_Positions.Add(content.transform.localPosition);
                m_Texts.Add(content.GetComponent<TextMeshPro>().text);

                // Access TextMeshPro component
                TextMeshPro tmp = content.GetComponent<TextMeshPro>();

                // Save text color
                textColors.Add(tmp.color);

                // Save font style
                // fontStyles.Add(tmp.fontStyle);

                // Save font
                fonts.Add(tmp.font);
            }

            m_Savefile.positions = m_Positions;
            m_Savefile.texts = m_Texts;
            m_Savefile.textColors = textColors; // Assign text colors
            // m_Savefile.fontStyles = fontStyles; // Assign font styles
            m_Savefile.fonts = fonts; // Assign fonts

            string jsonstring = JsonUtility.ToJson(m_Savefile, true);
            string dataPath = Path.Combine(Application.persistentDataPath, m_Filename);

            Debug.LogFormat("Saving contents: {0}", jsonstring);
            File.WriteAllText(dataPath, jsonstring);
        }

        public void LoadContents()
        {
            //fetch both functions in parrallel
            Debug.Log("going babes");
            FetchAndDownloadImageContent();
            FetchAndInstantiateTextContent();
            Debug.Log("coming babes");
            // Reference to your Firestore collection
            //for text content
        }

        private void FetchAndInstantiateTextContent()
        {
            CollectionReference textCollectionRef = db.Collection("text_content");

            // Fetch all documents from the collection
            textCollectionRef
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Error fetching collection documents: " + task.Exception);
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Debug.Log("Starting text");
                        // Assuming each document has a 'position' map and a 'text' field
                        if (document.Exists)
                        {
                            Dictionary<string, object> documentData = document.ToDictionary();

                            // Extracting the position map
                            if (!documentData.ContainsKey("position"))
                            {
                                Debug.LogWarning("Position not found");
                                continue;
                            }
                            Dictionary<string, object> positionMap = documentData["position"] as Dictionary<string, object>;
                            Vector3 pos = new Vector3(
                                Convert.ToSingle(positionMap["x"]),
                                Convert.ToSingle(positionMap["y"]),
                                Convert.ToSingle(positionMap["z"])
                            );

                            if (!documentData.ContainsKey("rotation"))
                            {
                                Debug.LogWarning("Rotation not found");
                                continue;
                            }
                            Dictionary<string, object> rotationMap = documentData["rotation"] as Dictionary<string, object>;
                            Quaternion rot = new Quaternion(
                                Convert.ToSingle(rotationMap["x"]),
                                Convert.ToSingle(rotationMap["y"]),
                                Convert.ToSingle(rotationMap["z"]),
                                Convert.ToSingle(rotationMap["w"])
                            );

                            if (!documentData.ContainsKey("scale"))
                            {
                                Debug.LogWarning("Scale not found");
                                continue;
                            }
                            Dictionary<string, object> scaleMap = documentData["scale"] as Dictionary<string, object>;
                            Vector3 scale = new Vector3(
                                Convert.ToSingle(scaleMap["x"]),
                                Convert.ToSingle(scaleMap["y"]),
                                Convert.ToSingle(scaleMap["z"])
                            );

                            // Extracting the text
                            if (!documentData.ContainsKey("text"))
                            {
                                Debug.LogWarning("Text not found");
                                continue;
                            }
                            string text = documentData["text"] as string;

                            // Additional properties
                            Color textColor = Color.white;
                            if (documentData.ContainsKey("textColor"))
                            {
                                string textColorString = documentData["textColor"] as string;
                                ColorUtility.TryParseHtmlString(textColorString, out textColor);
                            }

                            FontStyles fontStyle = FontStyles.Normal;
                            if (documentData.ContainsKey("fontStyle"))
                            {
                                string fontStyleString = documentData["fontStyle"] as string;
                                Enum.TryParse(fontStyleString, out fontStyle);
                            }

                            TMP_FontAsset fontAsset = null;
                            if (documentData.ContainsKey("font"))
                            {
                                string fontName = documentData["font"] as string;
                                fontAsset = Resources.Load<TMP_FontAsset>("Fonts/" + fontName);
                            }

                            // Instantiating the content prefab and setting its properties
                            GameObject go = Instantiate(
                                m_TextContentPrefab,
                                pos,
                                rot,
                                m_ARSpace.transform
                            );
                            go.transform.localScale = scale;

                            // Add id of document to the game object
                            go.GetComponent<MovableContent>().m_contentId = document.Id;
                            TextMeshPro textComponent = go.GetComponent<TextMeshPro>();
                            if (textComponent != null)
                            {
                                textComponent.text = text;
                                textComponent.color = textColor; // Set text color
                                textComponent.fontStyle = fontStyle; // Set font style
                                if (fontAsset != null)
                                {
                                    textComponent.font = fontAsset; // Set font
                                }
                            }
                        }
                    }
                });
        }


        private void FetchAndDownloadImageContent()
        {
            CollectionReference imageCollectionRef = db.Collection("image_content");

            Debug.Log("imageCollectionRef:  lol" + imageCollectionRef);
            // Fetch all documents from the collection
            imageCollectionRef
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Error fetching collection documents: " + task.Exception);
                        return;
                    }

                    Debug.Log("task.Result:  lol for images" + task.Result);
                    QuerySnapshot snapshot = task.Result;

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Debug.Log("heyyy babes");
                        // Assuming each document has a 'position' map and a 'text' field
                        if (document.Exists)
                        {
                            Dictionary<string, object> documentData = document.ToDictionary();

                            // Extracting the position map
                            //add error checking

                            if (documentData.ContainsKey("position") == false)
                            {
                                Debug.LogWarning("position not found");
                                continue;
                            }
                            Dictionary<string, object> positionMap =
                                documentData["position"] as Dictionary<string, object>;
                            Vector3 pos = new Vector3(
                                Convert.ToSingle(positionMap["x"]),
                                Convert.ToSingle(positionMap["y"]),
                                Convert.ToSingle(positionMap["z"])
                            );

                            if (documentData.ContainsKey("rotation") == false)
                            {
                                Debug.LogWarning("rotation not found");
                                continue;
                            }
                            Dictionary<string, object> rotationMap =
                                documentData["rotation"] as Dictionary<string, object>;
                            Quaternion rot = new Quaternion(
                                Convert.ToSingle(rotationMap["x"]),
                                Convert.ToSingle(rotationMap["y"]),
                                Convert.ToSingle(rotationMap["z"]),
                                Convert.ToSingle(rotationMap["w"])
                            );

                            if (documentData.ContainsKey("scale") == false)
                            {
                                Debug.LogWarning("scale not found");
                                continue;
                            }
                            Dictionary<string, object> scaleMap =
                                documentData["scale"] as Dictionary<string, object>;
                            Vector3 scale = new Vector3(
                                Convert.ToSingle(scaleMap["x"]),
                                Convert.ToSingle(scaleMap["y"]),
                                Convert.ToSingle(scaleMap["z"])
                            );
                            // Extracting the text

                            if (documentData.ContainsKey("image_ref") == false)
                            {
                                Debug.LogWarning("image_ref not found");
                                continue;
                            }
                            string image_ref_path = documentData["image_ref"] as string;

                            //fetch image from storage
                            //create a function to fetch image from storage
                            //create lambda experssion
                            Texture2D texture = null;
                            Action<Texture2D> OnTextureLoaded = (Texture2D newTexture) =>
                            {
                                texture = newTexture; // Assign the new texture to your original texture variable
                                if (texture == null)
                                {
                                    Debug.Log("Couldn't load texture from " + image_ref_path);
                                    return;
                                }

                                GameObject quadInstance = Instantiate(
                                    quadPrefab,
                                    pos, // Use the target position here
                                    rot, // Use the target rotation here
                                    m_ARSpace.transform // This sets the parent
                                );

                                // quadInstance.transform.localScale = scale;
                                // quadInstance.transform.localScale = new Vector3(
                                //     0.2f,
                                //     texture.height / (float)texture.width * 0.2f,
                                //     1f
                                // );

                                quadInstance.transform.localScale = scale;
                                // Debug.Log(image_ref_path);

                                ApplyTextureToSecondChild(quadInstance, texture);
                                // Instantiating the content prefab and setting its properties
                                // GameObject go = Instantiate(m_TextContentPrefab, m_ARSpace.transform);
                                //add id of document to the game object
                                quadInstance.GetComponent<MovableImageContent>().m_contentId =
                                    document.Id;
                                quadInstance.GetComponent<MovableImageContent>().imageRef =
                                    image_ref_path;
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
                                        StartCoroutine(DownloadImage(downloadUrl, OnTextureLoaded));
                                    }
                                    else
                                    {
                                        Debug.LogError("Failed to get download URL.");
                                    }
                                });

                            // TextMeshPro textComponent = go.GetComponent<TextMeshPro>();
                            // if (textComponent != null)
                            // {
                            //     textComponent.text = text;
                            // }
                        }
                    }

                    Debug.Log("Successfully loaded Firestore Image documents.");
                });
        }

        private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onTextureLoaded)
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
                }
            }
        }

        public void ChangePrefab(GameObject newPrefab)
        {
            m_TextContentPrefab = newPrefab;
        }

        public void LoadImageFromGallery()
        {
            //instantiate before to remember location
            Transform cameraTransform = Camera.main.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 position = cameraTransform.position + cameraTransform.forward;
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
                (path) =>
                {
                    Debug.Log("Image path: " + path);
                    if (path != null)
                    {
                        // Create Texture from selected image
                        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false, false);
                        byte[] imageBytes = texture.EncodeToJPG(); // or EncodeToPNG() based on your preference

                        string image_path = "images/" + Guid.NewGuid().ToString() + ".jpg";
                        ;
                        StorageReference imageRef = firebase_storage.GetReference(image_path);

                        imageRef
                            .PutBytesAsync(imageBytes)
                            .ContinueWithOnMainThread(
                                (task) =>
                                {
                                    if (task.IsFaulted || task.IsCanceled)
                                    {
                                        Debug.LogError(task.Exception.ToString());
                                        // Handle the error
                                    }
                                    else
                                    {
                                        // Image uploaded successfully
                                        Debug.Log("Image uploaded: " + image_path);
                                    }
                                }
                            );
                        if (texture == null)
                        {
                            Debug.Log("Couldn't load texture from " + path);
                            return;
                        }

                        // Instantiate the quad prefab and position it
                        GameObject quadInstance = Instantiate(
                            quadPrefab,
                            position,
                            Quaternion.identity,
                            m_ARSpace.transform
                        );
                        quadInstance.transform.forward = forward;

                        // Adjust the scale of the quad based on the texture's aspect ratio
                        quadInstance.transform.localScale = new Vector3(
                            quadInstance.transform.localScale.x,
                            quadInstance.transform.localScale.y
                                * texture.height
                                / (float)texture.width,
                            quadInstance.transform.localScale.z
                        );

                        quadInstance.transform.parent = m_ARSpace.transform;

                        //store image path so that content in firestore is able to access it
                        quadInstance.GetComponent<MovableImageContent>().imageRef = image_path;
                        // Apply the texture to the second child of the quad
                        ApplyTextureToSecondChild(quadInstance, texture);
                    }
                },
                "Select a PNG image",
                "image/png"
            );
        }

        private void ApplyTextureToSecondChild(GameObject quad, Texture2D texture)
        {
            // Check if the quad has at least two children
            if (quad.transform.childCount >= 2)
            {
                Transform secondChild = quad.transform.GetChild(1);
                Renderer childRenderer = secondChild.GetComponent<Renderer>();

                if (childRenderer != null)
                {
                    // Apply the texture to the second child
                    childRenderer.material.shader = Shader.Find("Unlit/Texture");
                    childRenderer.material.mainTexture = texture;
                }
                else
                {
                    Debug.LogWarning("Renderer not found on the second child of the quad prefab");
                }
            }
            else
            {
                Debug.LogWarning("The quad prefab does not have enough children");
            }
        }
    }
}
