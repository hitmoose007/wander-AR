/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using Immersal.REST;

namespace Immersal.AR
{
    [CustomEditor(typeof(ARMap))]
    public class ARMapEditor : Editor
    {
        private ImmersalSDK m_Sdk = null;

        private static float pointSizeSliderValue = 0.33f;
        private static bool renderAs3dPointsToggle = true;

        private TextAsset currentMapFile = null;
        private TextAsset prevMapFile = null;

        private void OnEnable()
        {
            pointSizeSliderValue = EditorPrefs.GetFloat("pointSizeSliderValue", pointSizeSliderValue);
            renderAs3dPointsToggle = EditorPrefs.GetBool("pointSizeSliderValue", renderAs3dPointsToggle);
        }

        public override void OnInspectorGUI()
        {
            ARMap obj = (ARMap)target;

            // reload map file without using OnValidate() on ARMap.cs
            currentMapFile = obj.mapFile;
            if (currentMapFile != prevMapFile || prevMapFile == null)
            {
                if (currentMapFile != null)
                {
                    if (AssetDatabase.GetAssetPath(currentMapFile).EndsWith(".bytes"))
                    {
                        EditorCoroutineUtility.StartCoroutine(DoLoadMap(), this);
                        prevMapFile = currentMapFile;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                    else
                    {
                        Debug.LogFormat("{0} is not a valid map file", AssetDatabase.GetAssetPath(currentMapFile));
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            pointSizeSliderValue = EditorGUILayout.Slider("Point Size", pointSizeSliderValue, 0f, 1f);
            renderAs3dPointsToggle = EditorGUILayout.Toggle("Render as 3D Points", renderAs3dPointsToggle);
            if (EditorGUI.EndChangeCheck())
            {
                ARMap.pointSize = pointSizeSliderValue;
                ARMap.renderAs3dPoints = renderAs3dPointsToggle;

                EditorPrefs.SetFloat("pointSizeSliderValue", pointSizeSliderValue);
                EditorPrefs.SetBool("pointSizeSliderValue", renderAs3dPointsToggle);

                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("Alignment metadata stored in right-handed coordinate system. Captured (default) alignment is in ECEF coordinates", MessageType.Info);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Load Alignment", "Loads alignment from map metadata. Coordinate system is unknown (ECEF or Unity's)")))
            {
                EditorCoroutineUtility.StartCoroutine(MapAlignmentLoad(), this);
            }

            if (GUILayout.Button(new GUIContent("Save Alignment", "Saves current (local transform) alignment to map metadata")))
            {
                EditorCoroutineUtility.StartCoroutine(MapAlignmentSave(), this);
            }

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.6f);

            if (GUILayout.Button(new GUIContent("Reset Alignment", "Fetches the original captured alignment metadata in ECEF coordinates")))
            {
                EditorCoroutineUtility.StartCoroutine(MapAlignmentReset(), this);
            }

            GUI.backgroundColor = oldColor;

            GUILayout.EndHorizontal();
        }

        private IEnumerator DoLoadMap()
        {
            ARMap obj = (ARMap)target;
            Task task = obj.LoadMap();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private IEnumerator MapAlignmentLoad()
        {
            //
            // Loads map metadata, updates AR Map metadata info, extracts the alignment, converts it to Unity's coordinate system and sets the map transform
            //

            ARMap obj = (ARMap)target;
            m_Sdk = ImmersalSDK.Instance;

            // Load map metadata from Immersal Cloud Service
            SDKMapMetadataGetRequest r = new SDKMapMetadataGetRequest();
            r.token = m_Sdk.developerToken;
            r.id = obj.mapId;

            string jsonString = JsonUtility.ToJson(r);
            UnityWebRequest request = UnityWebRequest.Put(string.Format(ImmersalHttp.URL_FORMAT, ImmersalSDK.Instance.localizationServer, SDKMapMetadataGetRequest.endpoint), jsonString);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError(string.Format("Failed to load alignment metadata for map id {0}\n{1}", obj.mapId, request.error));
            }
            else
            {
                SDKMapMetadataGetResult result = JsonUtility.FromJson<SDKMapMetadataGetResult>(request.downloadHandler.text);
                if (result.error == "none")
                {
                    // Save metadata file on disk, overwrite existing file
                    string destinationFolder = Path.Combine("Assets", "Map Data");
                    string jsonFilePath = Path.Combine(destinationFolder, string.Format("{0}-{1}-metadata.json", result.id, result.name));
                    WriteJson(jsonFilePath, request.downloadHandler.text);

                    Vector3 posMetadata = new Vector3((float)result.tx, (float)result.ty, (float)result.tz);
                    Quaternion rotMetadata = new Quaternion((float)result.qx, (float)result.qy, (float)result.qz, (float)result.qw);
                    float scaleMetadata = (float)result.scale; // Only uniform scale metadata is supported

                    // Update metadata information on AR Map
                    obj.mapAlignment.tx = posMetadata.x;
                    obj.mapAlignment.ty = posMetadata.y;
                    obj.mapAlignment.tz = posMetadata.z;
                    obj.mapAlignment.qx = rotMetadata.x;
                    obj.mapAlignment.qy = rotMetadata.y;
                    obj.mapAlignment.qz = rotMetadata.z;
                    obj.mapAlignment.qw = rotMetadata.w;
                    obj.mapAlignment.scale = scaleMetadata;

                    obj.wgs84.latitude = result.latitude;
                    obj.wgs84.longitude = result.longitude;
                    obj.wgs84.altitude = result.altitude;

                    obj.privacy = result.privacy;

                    // IMPORTANT
                    // Switch coordinate system handedness back from Immersal Cloud Service's default right-handed system to Unity's left-handed system
                    Matrix4x4 b = Matrix4x4.TRS(posMetadata, rotMetadata, new Vector3(scaleMetadata, scaleMetadata, scaleMetadata));
                    Matrix4x4 a = ARHelper.SwitchHandedness(b);
                    Vector3 pos = a.GetColumn(3);
                    Quaternion rot = a.rotation;
                    Vector3 scl = new Vector3(scaleMetadata, scaleMetadata, scaleMetadata); // Only uniform scale metadata is supported

                    // Set AR Map local transform from the converted metadata
                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;
                    obj.transform.localScale = scl;
                }
            }
        }

        private IEnumerator MapAlignmentSave()
        {
            //
            // Updates map metadata to the Cloud Service and reloads to keep local files in sync
            //

            ARMap obj = (ARMap)target;
            m_Sdk = ImmersalSDK.Instance;

            Vector3 pos = obj.transform.localPosition;
            Quaternion rot = obj.transform.localRotation;
            float scl = (obj.transform.localScale.x + obj.transform.localScale.y + obj.transform.localScale.z) / 3f; // Only uniform scale metadata is supported

            // IMPORTANT
            // Switching coordinate system handedness from Unity's left-handed system to Immersal Cloud Service's default right-handed system
            Matrix4x4 b = Matrix4x4.TRS(pos, rot, obj.transform.localScale);
            Matrix4x4 a = ARHelper.SwitchHandedness(b);
            pos = a.GetColumn(3);
            rot = a.rotation;

            // Update map alignment metadata to Immersal Cloud Service
            SDKMapAlignmentSetRequest r = new SDKMapAlignmentSetRequest();
            r.token = m_Sdk.developerToken;
            r.id = obj.mapId;
            r.tx = pos.x;
            r.ty = pos.y;
            r.tz = pos.z;
            r.qx = rot.x;
            r.qy = rot.y;
            r.qz = rot.z;
            r.qw = rot.w;
            r.scale = scl;

            string jsonString = JsonUtility.ToJson(r);
            UnityWebRequest request = UnityWebRequest.Put(string.Format(ImmersalHttp.URL_FORMAT, ImmersalSDK.Instance.localizationServer, SDKMapAlignmentSetRequest.endpoint), jsonString);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError(string.Format("Failed to save alignment for map id {0}\n{1}", obj.mapId, request.error));
            }
            else
            {
                SDKMapAlignmentSetResult result = JsonUtility.FromJson<SDKMapAlignmentSetResult>(request.downloadHandler.text);
                if (result.error == "none")
                {
                    // Reload the metadata from Immersal Cloud Service to keep local files in sync
                    EditorCoroutineUtility.StartCoroutine(MapAlignmentLoad(), this);
                }
            }
        }

        private IEnumerator MapAlignmentReset()
        {
            //
            // Reset map alignment to the original captured data and reload metadata from the Immersal Cloud Service to keep local files in sync
            //

            ARMap obj = (ARMap)target;
            m_Sdk = ImmersalSDK.Instance;

            // Reset alignment on Immersal Cloud Service
            SDKMapAlignmentResetRequest r = new SDKMapAlignmentResetRequest();
            r.token = m_Sdk.developerToken;
            r.id = obj.mapId;

            string jsonString = JsonUtility.ToJson(r);
            UnityWebRequest request = UnityWebRequest.Put(string.Format(ImmersalHttp.URL_FORMAT, ImmersalSDK.Instance.localizationServer, SDKMapAlignmentResetRequest.endpoint), jsonString);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError(string.Format("Failed to reset alignment for map id {0}\n{1}", obj.mapId, request.error));
            }
            else
            {
                SDKMapAlignmentResetResult result = JsonUtility.FromJson<SDKMapAlignmentResetResult>(request.downloadHandler.text);
                if (result.error == "none")
                {
                    // Reload the metadata from Immersal Cloud Service to keep local files in sync
                    EditorCoroutineUtility.StartCoroutine(MapAlignmentLoad(), this);
                }
            }
        }

        private void WriteJson(string jsonFilepath, string data)
        {
            File.WriteAllText(jsonFilepath, data);
            AssetDatabase.ImportAsset(jsonFilepath);
        }
    }
}
#endif