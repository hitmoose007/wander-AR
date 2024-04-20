/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections.Generic;
using System.Collections;
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
using Immersal.Samples.Navigation;


using System.Linq;
using UnityEngine.Assertions.Must;

namespace Immersal.Samples.ContentPlacement
{
    public class ContentStorageManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_TextContentPrefab = null;

        public GameObject NavPrefab;

        [SerializeField]
        private GameObject quadPrefab;

        // private DatabaseReference db_reference;

        [SerializeField]
        private Immersal.AR.ARSpace m_ARSpace;

        private FirebaseFirestore db;

        private FirebaseStorage firebase_storage;

        [SerializeField]
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

            // Add TextMeshPro component if not already present
            TextMeshPro textMeshPro = go.GetComponent<TextMeshPro>();
            if (textMeshPro == null)
            {
                textMeshPro = go.AddComponent<TextMeshPro>();
            }

            // Set the text
            TextMeshPro textPrefabTMPro = m_TextContentPrefab.GetComponent<TextMeshPro>();
            Debug.Log("Adding content: " + textPrefabTMPro.text);
            if (textPrefabTMPro != null)
            {
                textMeshPro.text = textPrefabTMPro.text;

                // Set the font
                if (textPrefabTMPro.font != null)
                {
                    textMeshPro.font = textPrefabTMPro.font;
                }
                else
                {
                    Debug.LogWarning("Font not found in the prefab.");
                }

                // Set the color
                textMeshPro.color = textPrefabTMPro.color;
            }
            else
            {
                Debug.LogError("TextMeshPro component not found in the prefab.");
            }
        }

        public void LoadContents()
        {
            //fetch both functions in parrallel
            FetchAndDownloadImageContent();
            FetchAndInstantiateTextContent();
            FetchAndInstantiateNavPoint();

            //wait 10 seconds before setting all game objects to active
            StartCoroutine(WaitAndSetActive());
            // Reference to your Firestore collection
            //for text content
        }

        IEnumerator WaitAndSetActive()
        {
            yield return new WaitForSeconds(10);
            foreach (Transform child in m_ARSpace.transform)
            {
                child.gameObject.SetActive(true);
            }
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
                        // Assuming each document has a 'position' map and a 'text' field
                        if (document.Exists)
                        {
                            Dictionary<string, object> documentData = document.ToDictionary();
                            foreach (string key in documentData.Keys)
                            {
                                Debug.Log("Key: " + key);
                            }

                            // Debug.Log("documentData: " + documentData.ContainsKey("position") + documentData.ContainsKey("text") + documentData.ContainsKey("text_color") + documentData.ContainsKey("font"));

                            // Extracting the position map
                            if (!documentData.ContainsKey("position"))
                            {
                                Debug.LogWarning("Position not found");
                                continue;
                            }
                            Dictionary<string, object> positionMap =
                                documentData["position"] as Dictionary<string, object>;
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
                            Dictionary<string, object> rotationMap =
                                documentData["rotation"] as Dictionary<string, object>;
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

                            Dictionary<string, object> scaleMap =
                                documentData["scale"] as Dictionary<string, object>;
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

                            // Extracting the text color
                            Color textColor;
                            if (!documentData.ContainsKey("color"))
                            {
                                Debug.LogWarning("Text color not found");
                                textColor = new Color(1f, 1f, 1f, 1f);
                            }
                            else
                            {
                                Dictionary<string, object> textColorMap =
                                    documentData["color"] as Dictionary<string, object>;

                                textColor = new Color(
                                    Convert.ToSingle(textColorMap["r"]),
                                    Convert.ToSingle(textColorMap["g"]),
                                    Convert.ToSingle(textColorMap["b"]),
                                    Convert.ToSingle(textColorMap["a"])
                                );
                                Debug.Log("Text color: " + textColor);
                            }

                            // extracting font
                            string fontName;
                            if (!documentData.ContainsKey("font"))
                            {
                                Debug.LogWarning("Font not found");
                                fontName = "Anton SDF";
                            }
                            else
                            {
                                fontName = documentData["font"] as string;
                                Debug.Log("Font name: " + fontName);
                            }

                            TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>(
                                "Fonts & Materials/" + fontName
                            );
                            Debug.Log("Font asset: " + fontAsset);
                            // extracting the font style
                            string fontStyle;
                            if (!documentData.ContainsKey("fontStyle"))
                            {
                                Debug.LogWarning("Font style not found");
                                fontStyle = "Normal";
                            }
                            else
                            {
                                fontStyle = documentData["fontStyle"] as string;
                                Debug.Log("Font style: " + fontStyle);
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
                            Debug.Log(
                                "TextMeshPro component found in the prefab:"
                                    + textComponent.text
                                    + " "
                                    + textComponent.font
                                    + " "
                                    + textComponent.color
                            );

                            if (textComponent != null)
                            {
                                textComponent.text = text;
                                if (fontAsset != null)
                                {
                                    textComponent.font = fontAsset; // Set font
                                }
                                else
                                {
                                    Debug.LogWarning("Font asset is null");
                                }

                                textComponent.color = textColor; // Set text color
                                Debug.Log("Fontstyle: " + fontStyle);
                                switch (fontStyle)
                                {
                                    case "Normal":
                                        textComponent.fontStyle = FontStyles.Normal;
                                        break;
                                    case "Bold":
                                        textComponent.fontStyle = FontStyles.Bold;
                                        break;
                                    case "Italic":
                                        textComponent.fontStyle = FontStyles.Italic;
                                        break;
                                    case "BoldItalic":
                                        textComponent.fontStyle =
                                            FontStyles.Bold | FontStyles.Italic;
                                        break;
                                    case "Underline":
                                        textComponent.fontStyle = FontStyles.Underline;
                                        break;
                                    case "Strikethrough":
                                        textComponent.fontStyle = FontStyles.Strikethrough;
                                        break;
                                    case "UpperCase":
                                        textComponent.fontStyle = FontStyles.UpperCase;
                                        break;
                                    default:
                                        textComponent.fontStyle = FontStyles.Normal;
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("TextMeshPro component is null");
                            }
                            go.SetActive(false);
                        }
                    }
                });
        }

        private void FetchAndInstantiateNavPoint()
        {
            CollectionReference navPointCollectionRef = db.Collection("navigation_targets");

            // if empty, return
            if (navPointCollectionRef == null)
            {
                Debug.LogWarning("Navigation target collection not found");
                return;
            }

            // Fetch all documents from the collection
            navPointCollectionRef
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
                            Dictionary<string, object> positionMap =
                                documentData["position"] as Dictionary<string, object>;
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
                            Dictionary<string, object> rotationMap =
                                documentData["rotation"] as Dictionary<string, object>;
                            Quaternion rot = new Quaternion(
                                Convert.ToSingle(rotationMap["x"]),
                                Convert.ToSingle(rotationMap["y"]),
                                Convert.ToSingle(rotationMap["z"]),
                                Convert.ToSingle(rotationMap["w"])
                            );

                            if (!documentData.ContainsKey("targetName"))
                            {
                                Debug.LogWarning("Target name not found");
                                continue;
                            }
                            Dictionary<string, object> targetNameMap =
                                documentData["targetName"] as Dictionary<string, object>;

                            // Instantiating the content prefab and setting its properties

                            GameObject go = Instantiate(
                                NavPrefab,
                                pos,
                                rot,
                                m_ARSpace.transform
                            );

                            go.GetComponent<IsNavigationTarget>().targetName = targetNameMap["targetName"] as string;
                            go.GetComponent<MovableContent>().m_contentId = document.Id;
                            go.SetActive(false);

                            // Add id of document to the game object




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

                            // Debug.Log(
                            //     "documentData:  key value pairs"
                            //         + documentData.ContainsKey("image_ref")
                            // );
                            if (
                                documentData.ContainsKey("image_ref") == false
                                || documentData["image_ref"] == null
                            )
                            {
                                Debug.LogWarning("image_ref not found");
                                continue;
                            }
                            string image_ref_path = documentData["image_ref"] as string;

                            GameObject quadInstance = Instantiate(
                                quadPrefab,
                                pos, // Use the target position here
                                rot, // Use the target rotation here
                                m_ARSpace.transform // This sets the parent
                            );

                            quadInstance.transform.localScale = scale;

                            quadInstance.GetComponent<MovableImageContent>().m_contentId =
                                document.Id;
                            quadInstance.GetComponent<MovableImageContent>().imageRef =
                                image_ref_path;
                            quadInstance.SetActive(false);
                            //deactivate
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

                                //disable game object

                                // quadInstance.transform.localScale = scale;
                                // quadInstance.transform.localScale = new Vector3(
                                //     0.2f,
                                //     texture.height / (float)texture.width * 0.2f,
                                //     1f
                                // );

                                // Debug.Log(image_ref_path);

                                ApplyTextureToSecondChild(quadInstance, texture);
                                // Instantiating the content prefab and setting its properties
                                // GameObject go = Instantiate(m_TextContentPrefab, m_ARSpace.transform);
                                //add id of document to the game object
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

        //function to set all game objects to active
    }
}
