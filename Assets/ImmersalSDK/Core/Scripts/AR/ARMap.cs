/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace Immersal.AR
{
    [System.Serializable]
    public class MapLocalizedEvent : UnityEvent<int>
    {
    }

    [ExecuteAlways]
    public class ARMap : MonoBehaviour
    {
        public static readonly Color[] pointCloudColors = new Color[]	{	new Color(0.22f,    1f,     0.46f), 
																            new Color(0.96f,    0.14f,  0.14f),
																            new Color(0.16f,    0.69f,  0.95f),
																            new Color(0.93f,    0.84f,  0.12f),
																            new Color(0.57f,    0.93f,  0.12f),
																            new Color(1f,       0.38f,  0.78f),
																            new Color(0.4f,     0f,     0.9f),
																            new Color(0.89f,    0.4f,   0f)
															            };

        public enum RenderMode { DoNotRender, EditorOnly, EditorAndRuntime }

        public static Dictionary<int, ARMap> mapHandleToMap = new Dictionary<int, ARMap>();
		public static bool pointCloudVisible = true;

        public RenderMode renderMode = RenderMode.EditorOnly;
        public TextAsset mapFile;
        
        [SerializeField]
        private Color m_PointColor = new Color(0.57f, 0.93f, 0.12f);

        [Space(10)]
        [Header("Map Metadata")]

        [SerializeField][ReadOnly]
        private int m_MapId = -1;
        [SerializeField][ReadOnly]
        private string m_MapName = null;
        [ReadOnly]
        public int privacy;
        [ReadOnly]
        public MapAlignment mapAlignment;
        [ReadOnly]
        public WGS84 wgs84;

        [Space(10)]
        [Header("Events")]

        public MapLocalizedEvent OnFirstLocalization = null;
        protected ARSpace m_ARSpace = null;
        private bool m_LocalizedOnce = false;

        public Color pointColor
        {
            get { return m_PointColor; }
            set { m_PointColor = value; }
        }

        public static float pointSize = 0.33f;
        // public static bool isRenderable = true;
        public static bool renderAs3dPoints = true;

        [System.Serializable]
        public struct MapAlignment
        {
            public double tx;
            public double ty;
            public double tz;
            public double qx;
            public double qy;
            public double qz;
            public double qw;
            public double scale;
        }

        [System.Serializable]
        public struct WGS84
        {
            public double latitude;
            public double longitude;
            public double altitude;
        }

        private Shader m_Shader;
        private Material m_Material;
        private Mesh m_Mesh;
        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;

        public Transform root { get; protected set; }
        public int mapHandle { get; private set; } = -1;

        public int mapId
        {
            get => m_MapId;
            private set => m_MapId = value;
        }

        public string mapName
        {
            get => m_MapName;
            set => m_MapName = value;
        }

        public static int MapHandleToId(int handle)
        {
            if (mapHandleToMap.ContainsKey(handle))
            {
                return mapHandleToMap[handle].mapId;
            }
            return -1;
        }

        public static int MapIdToHandle(int id)
        {
            if (ARSpace.mapIdToMap.ContainsKey(id))
            {
                return ARSpace.mapIdToMap[id].mapHandle;
            }
            return -1;
        }

        public double[] MapToEcefGet()
        {
            double[] q = new double[] {this.mapAlignment.qw, this.mapAlignment.qx, this.mapAlignment.qy, this.mapAlignment.qz};
            double[] m = new double[9];
            ARHelper.DoubleQuaternionToDoubleMatrix3x3(out m, q);

            double[] mapToEcef = new double[] {this.mapAlignment.tx, this.mapAlignment.ty, this.mapAlignment.tz, m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], this.mapAlignment.scale};

            return mapToEcef;
        }

        public virtual void FreeMap(bool destroy = false)
        {
            if (mapHandle >= 0)
            {
                Immersal.Core.FreeMap(mapHandle);

                if (mapHandleToMap.ContainsKey(mapHandle))
                {
                    mapHandleToMap.Remove(mapHandle);
                }
            }

            mapHandle = -1;
            ClearMesh();
            Reset();

            if (this.mapId > 0)
            {
                ARSpace.UnregisterSpace(root, this.mapId);
                this.mapId = -1;
            }

            if (destroy)
            {
                GameObject.Destroy(gameObject);
            }
        }

        public virtual void Reset()
        {
            m_LocalizedOnce = false;
        }

        public virtual async Task<int> LoadMap(byte[] mapBytes = null, int mapId = -1)
        {
            if (mapBytes == null)
            {
                mapBytes = (mapFile != null) ? mapFile.bytes : null;
            }

            if (mapBytes != null)
            {
                Task<int> t = Task.Run(() =>
                {
                    return Immersal.Core.LoadMap(mapBytes);
                });

                await t;

                mapHandle = t.Result;

                if (this == null)
                {
                    FreeMap();
                    return -1;
                }
            }

            if (mapId > 0)
            {
                this.mapId = mapId;
            }
            else
            {
                ParseMapIdAndName();
            }

            if (mapHandle >= 0)
            {
                int pointCloudSize = Immersal.Core.GetPointCloudSize(mapHandle);
                Vector3[] points = new Vector3[pointCloudSize];
                Immersal.Core.GetPointCloud(mapHandle, points);
                for (int i = 0; i < pointCloudSize; i++)
                {
                    points[i] = ARHelper.SwitchHandedness(points[i]);
                }
                mapHandleToMap[mapHandle] = this;
                InitializeMesh(points);
            }

            //Debug.LogFormat("LoadMap() maphandle: {0}, mapID: {1}, name: {2}", mapHandle, this.mapId, mapName);

            if (this.mapId > 0 && m_ARSpace != null)
            {
                root = m_ARSpace.transform;
                ARSpace.RegisterSpace(root, this, transform.localPosition, transform.localRotation, transform.localScale);
            }

            return mapHandle;
        }

        private void InitializeMesh(Vector3[] pointPositions)
        {
            if (this == null) return;

            if (m_Shader == null)
            {
                m_Shader = Shader.Find("Immersal/Point Cloud");
            }

            if (m_Material == null)
            {
                m_Material = new Material(m_Shader);
                m_Material.hideFlags = HideFlags.DontSave;
            }

            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_Mesh.indexFormat = IndexFormat.UInt32;
            }

            int numPoints = pointPositions.Length;

            int[] indices = new int[numPoints];
            Vector3[] pts = new Vector3[numPoints];
            Color32[] col = new Color32[numPoints];

            for (int i = 0; i < numPoints; ++i)
            {
                indices[i] = i;
                pts[i] = pointPositions[i];
            }

            m_Mesh.Clear();
            m_Mesh.vertices = pts;
            m_Mesh.colors32 = col;
            m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
            m_Mesh.RecalculateBounds();

            if (m_MeshFilter == null)
            {
                m_MeshFilter = gameObject.GetComponent<MeshFilter>();
                if (m_MeshFilter == null)
                {
                    m_MeshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }

            if (m_MeshRenderer == null)
            {
                m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (m_MeshRenderer == null)
                {
                    m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }

            m_MeshFilter.mesh = m_Mesh;
            m_MeshRenderer.material = m_Material;

            m_MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_MeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            m_MeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        private void InitializeMesh()
        {
            InitializeMesh(new Vector3[0]);
        }

        private void ClearMesh()
        {
            if (m_Mesh != null)
            {
                m_Mesh.Clear();
            }
        }

        public void NotifySuccessfulLocalization(int mapId)
        {
            if (m_LocalizedOnce)
                return;
            
            OnFirstLocalization?.Invoke(mapId);
            m_LocalizedOnce = true;
        }

        private void Awake()
        {
            m_ARSpace = gameObject.GetComponentInParent<ARSpace>();
            if (!m_ARSpace)
            {
                GameObject go = new GameObject("AR Space");
                m_ARSpace = go.AddComponent<ARSpace>();
                transform.SetParent(go.transform);
            }

            ParseMapIdAndName();
            InitializeMesh();
        }

        private void ParseMapIdAndName()
        {
            int id;
            if (GetMapId(out id))
            {
                this.mapId = id;
                this.mapName = mapFile.name.Substring(id.ToString().Length + 1);
            }

            if (Application.isEditor)
            {
                if (mapFile != null)
                {
                    try
                    {
                        string destinationFolder = Path.Combine("Assets", "Map Data");
                        string jsonFilePath = Path.Combine(destinationFolder, string.Format("{0}-metadata.json", mapFile.name));

                        MetadataFile metadataFile = JsonUtility.FromJson<MetadataFile>(File.ReadAllText(jsonFilePath));
                        
                        this.mapAlignment.tx = metadataFile.tx;
                        this.mapAlignment.ty = metadataFile.ty;
                        this.mapAlignment.tz = metadataFile.tz;

                        this.mapAlignment.qx = metadataFile.qx;
                        this.mapAlignment.qy = metadataFile.qy;
                        this.mapAlignment.qz = metadataFile.qz;
                        this.mapAlignment.qw = metadataFile.qw;

                        this.mapAlignment.scale = metadataFile.scale;
                        
                        this.wgs84.latitude = metadataFile.latitude;
                        this.wgs84.longitude = metadataFile.longitude;
                        this.wgs84.altitude = metadataFile.altitude;
                        
                        this.privacy = metadataFile.privacy;
                    }
                    catch (FileNotFoundException e)
                    {
                        Debug.LogWarningFormat("{0}\nCould not find {1}-metadata.json", e.Message, mapFile.name);
                        // set default values in case metadata is not available
                        
                        this.mapAlignment.tx = 0.0;
                        this.mapAlignment.ty = 0.0;
                        this.mapAlignment.tz = 0.0;

                        this.mapAlignment.qx = 0.0;
                        this.mapAlignment.qy = 0.0;
                        this.mapAlignment.qz = 0.0;
                        this.mapAlignment.qw = 1.0;

                        this.mapAlignment.scale = 1.0;
                        
                        this.wgs84.latitude = 0.0;
                        this.wgs84.longitude = 0.0;
                        this.wgs84.altitude = 0.0;

                        this.privacy = 0;
                    }
                }
            }
        }

        [System.Serializable]
        public struct MetadataFile
        {
            public string error;
            public int id;
            public int type;
            public string created;
            public string version;
            public int user;
            public int creator;
            public string name;
            public int size;
            public string status;
            public int privacy;
            public double latitude;
            public double longitude;
            public double altitude;
            public double tx;
            public double ty;
            public double tz;
            public double qw;
            public double qx;
            public double qy;
            public double qz;
            public double scale;
            public string sha256_al;
            public string sha256_sparse;
            public string sha256_dense;
            public string sha256_tex;
        }

        private bool GetMapId(out int mapId)
        {
            if (mapFile == null)
            {
                mapId = -1;
                return false;
            }

            string mapFileName = mapFile.name;
            Regex rx = new Regex(@"^\d+");
            Match match = rx.Match(mapFileName);
            if (match.Success)
            {
                mapId = Int32.Parse(match.Value);
                return true;
            }
            else
            {
                mapId = -1;
                return false;
            }
        }

        private async void OnEnable()
        {
            if (mapFile != null)
            {
                await LoadMap();
            }
        }

        private void OnDisable()
        {
            FreeMap();
        }

        private void OnDestroy()
        {
            FreeMap();

            if (m_Material != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_Mesh);
                    Destroy(m_Material);
                }
                else
                {
                    DestroyImmediate(m_Mesh);
                    DestroyImmediate(m_Material);
                }
            }
        }

        private bool IsRenderable()
        {
            if (pointCloudVisible)
            {
                switch (renderMode)
                {
                    case RenderMode.DoNotRender:
                        return false;
                    case RenderMode.EditorOnly:
                        if (Application.isEditor)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case RenderMode.EditorAndRuntime:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }


        private void OnRenderObject()
        {
            if (IsRenderable() && m_Material != null)
            {
                m_MeshRenderer.enabled = true;

                if (renderAs3dPoints)
                {
                    m_Material.SetFloat("_PerspectiveEnabled", 1f);
                    m_Material.SetFloat("_PointSize", Mathf.Lerp(0.002f, 0.14f, Mathf.Max(0, Mathf.Pow(pointSize, 3f))));
                }
                else
                {
                    m_Material.SetFloat("_PerspectiveEnabled", 0f);
                    m_Material.SetFloat("_PointSize", Mathf.Lerp(1.5f, 40f, Mathf.Max(0, pointSize)));
                }
                m_Material.SetColor("_PointColor", m_PointColor);
            }
            else
            {
                m_MeshRenderer.enabled = false;
            }
        }
    }
}