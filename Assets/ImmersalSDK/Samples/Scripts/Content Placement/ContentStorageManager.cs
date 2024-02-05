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
using System.IO;
using TMPro;
using NativeGalleryNamespace;

namespace Immersal.Samples.ContentPlacement
{
    public class ContentStorageManager : MonoBehaviour
    {
        [HideInInspector]
        public List<MovableContent> contentList = new List<MovableContent>();

        [SerializeField]
        private GameObject m_ContentPrefab = null;

        [SerializeField]
        private Immersal.AR.ARSpace m_ARSpace;

        [SerializeField]
        private string m_Filename = "content.json";
        private Savefile m_Savefile;
        private List<Vector3> m_Positions = new List<Vector3>();
        private List<string> m_Texts = new List<string>(); // Create list for text

        [SerializeField]
        private GameObject quadPrefab;

        [System.Serializable]
        public struct Savefile
        {
            public List<Vector3> positions;
            public List<string> texts;
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
        }

        private void Start()
        {
            contentList.Clear();
            LoadContents();
        }

        public void AddContent()
        {
            Transform cameraTransform = Camera.main.transform;
            GameObject go = Instantiate(
                m_ContentPrefab,
                cameraTransform.position + cameraTransform.forward,
                Quaternion.identity,
                m_ARSpace.transform
            );
            // go.AddComponent<TextMeshPro>();
            go.GetComponent<TextMeshPro>().text = m_ContentPrefab.GetComponent<TextMeshPro>().text;
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
            m_Texts.Clear(); // Clear text list
            // List<string> texts = new List<string>(); // Create list for text

            foreach (MovableContent content in contentList)
            {
                m_Positions.Add(content.transform.localPosition);

                //convert prefab to text mesh pro and save text
                // Debug.Log("content.text: " + content);
                m_Texts.Add(content.GetComponent<TextMeshPro>().text); // Access text from MovableContent (adjust based on your implementation)
            }

            m_Savefile.positions = m_Positions;
            m_Savefile.texts = m_Texts; // Assign text to struct

            string jsonstring = JsonUtility.ToJson(m_Savefile, true);
            string dataPath = Path.Combine(Application.persistentDataPath, m_Filename);

            // Debug.LogFormat("Trying to save file: {0}", dataPath);
            Debug.LogFormat("Saving contents: {0}", jsonstring);
            File.WriteAllText(dataPath, jsonstring);
        }

        public void LoadContents()
        {
            string dataPath = Path.Combine(Application.persistentDataPath, m_Filename);
            Debug.LogFormat("Trying to load file: {0}", dataPath);

            try
            {
                Savefile loadFile = JsonUtility.FromJson<Savefile>(File.ReadAllText(dataPath));
                int index = 0;
                foreach (Vector3 pos in loadFile.positions)
                {
                    GameObject go = Instantiate(m_ContentPrefab, m_ARSpace.transform);
                    go.transform.localPosition = pos;

                    go.GetComponent<TextMeshPro>().text = loadFile.texts[index];

                    index++;
                }

                Debug.Log("Successfully loaded file!");
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarningFormat(
                    "{0}\n.json file for content storage not found. Created a new file!",
                    e.Message
                );
                File.WriteAllText(dataPath, "");
            }
            catch (NullReferenceException err)
            {
                Debug.LogWarningFormat(
                    "{0}\n.json file for content storage not found. Created a new file!",
                    err.Message
                );
                File.WriteAllText(dataPath, "");
            }
        }

        public void ChangePrefab(GameObject newPrefab)
        {
            m_ContentPrefab = newPrefab;
        }

        public void LoadImageFromGallery()
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
                (path) =>
                {
                    Debug.Log("Image path: " + path);
                    if (path != null)
                    {
                        // Create Texture from selected image
                        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false, false);
                        if (texture == null)
                        {
                            Debug.Log("Couldn't load texture from " + path);
                            return;
                        }

                        // Create a quad and position it in front of the camera
                        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        quad.transform.position =
                            Camera.main.transform.position + Camera.main.transform.forward * 1f;
                        quad.transform.forward = Camera.main.transform.forward;

                        // Adjust the scale of the quad based on the texture's aspect ratio
                        quad.transform.localScale = new Vector3(
                            0.2f,
                            texture.height / (float)texture.width * 0.2f,
                            1f
                        );

                        quad.transform.parent = m_ARSpace.transform;

                        // Apply the texture to the quad
                        Material material = quad.GetComponent<Renderer>().material;
                        // Optionally, set the quad to be destroyed after a certain time
                        // Apply the texture to the quad using a widely compatible shader
                        material.shader = Shader.Find("Unlit/Texture");
                        material.mainTexture = texture;
                        // Remove or adjust this line if you want the quad to persist
                        // Destroy(quad, 5f);
                    }
                },
                "Select a PNG image",
                "image/png"
            );
        }
    }
}
