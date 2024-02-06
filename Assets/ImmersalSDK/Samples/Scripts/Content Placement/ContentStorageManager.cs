﻿/*===============================================================================
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
        private GameObject m_TextContentPrefab = null;

        [SerializeField]
        private GameObject quadPrefab;

        [SerializeField]
        private Immersal.AR.ARSpace m_ARSpace;

        [SerializeField]
        private string m_Filename = "content.json";
        private Savefile m_Savefile;
        private List<Vector3> m_Positions = new List<Vector3>();
        private List<string> m_Texts = new List<string>(); // Create list for text

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
                m_TextContentPrefab,
                cameraTransform.position + cameraTransform.forward,
                Quaternion.identity,
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

        public void SaveContents(MovableContent movableContent)
        {
            
            if(movableContent.contentType == MovableContent.ContentType.Image)
            {
                return;
            }
            
            {
                contentList.Add(movableContent);
            }
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
                    GameObject go = Instantiate(m_TextContentPrefab, m_ARSpace.transform);
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
            m_TextContentPrefab = newPrefab;
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

                        // Instantiate the quad prefab and position it
                        Transform cameraTransform = Camera.main.transform;
                        GameObject quadInstance = Instantiate(
                            quadPrefab,
                            cameraTransform.position + cameraTransform.forward,
                            Quaternion.identity,
                            m_ARSpace.transform
                        );
                        quadInstance.transform.forward = Camera.main.transform.forward;

                        // Adjust the scale of the quad based on the texture's aspect ratio
                        quadInstance.transform.localScale = new Vector3(
                            0.2f,
                            texture.height / (float)texture.width * 0.2f,
                            1f
                        );

                        quadInstance.transform.parent = m_ARSpace.transform;

                        // Apply the texture to the second child of the quad
                        ApplyTextureToSecondChild(quadInstance, texture);
                    }
                },
                "Select a PNG image",
                "image/png"
            );
        }

        void ApplyTextureToSecondChild(GameObject quad, Texture2D texture)
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
