/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Immersal.REST;

namespace Immersal.AR
{
	public class SpaceContainer
	{
        public int mapCount = 0;
		public Vector3 targetPosition = Vector3.zero;
		public Quaternion targetRotation = Quaternion.identity;
		public PoseFilter filter = new PoseFilter();
	}

    public class MapOffset
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public SpaceContainer space;
    }

    public class ARSpace : MonoBehaviour
    {
        public static Dictionary<Transform, SpaceContainer> transformToSpace = new Dictionary<Transform, SpaceContainer>();
        public static Dictionary<SpaceContainer, Transform> spaceToTransform = new Dictionary<SpaceContainer, Transform>();
        public static Dictionary<int, MapOffset> mapIdToOffset = new Dictionary<int, MapOffset>();
        public static Dictionary<int, ARMap> mapIdToMap = new Dictionary<int, ARMap>();

        private Matrix4x4 m_InitialOffset = Matrix4x4.identity;

        public Matrix4x4 initialOffset
        {
            get { return m_InitialOffset; }
        }

        void Awake()
		{
			Vector3 pos = transform.position;
			Quaternion rot = transform.rotation;
			Matrix4x4 offset = Matrix4x4.TRS(pos, rot, Vector3.one);

			m_InitialOffset = offset;
		}

        public void OnDestroy()
        {
            transformToSpace.Clear();
            spaceToTransform.Clear();
            mapIdToOffset.Clear();
            mapIdToMap.Clear();
        }

        public Pose ToCloudSpace(Vector3 camPos, Quaternion camRot)
		{
			Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
			Matrix4x4 trackerToCloudSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Matrix4x4 cloudSpace = trackerToCloudSpace.inverse * trackerSpace;

			return new Pose(cloudSpace.GetColumn(3), cloudSpace.rotation);
		}

		public Pose FromCloudSpace(Vector3 camPos, Quaternion camRot)
		{
			Matrix4x4 cloudSpace = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Matrix4x4 trackerSpace = Matrix4x4.TRS(camPos, camRot, Vector3.one);
			Matrix4x4 m = trackerSpace * (cloudSpace.inverse);

			return new Pose(m.GetColumn(3), m.rotation);
		}

		public static async Task<ARMap> LoadAndInstantiateARMap(Transform root, SDKMapResult map, ARMap.RenderMode renderMode = ARMap.RenderMode.DoNotRender, Color pointCloudColor = default, bool applyAlignment = false)
		{
			GameObject go = new GameObject(string.Format("AR Map {0}-{1}", map.metadata.id, map.metadata.name));
            if (root != null)
            {
                go.transform.SetParent(root, false);
            }

            if (applyAlignment)
            {
                Matrix4x4 b = Matrix4x4.TRS(new Vector3((float)map.metadata.tx, (float)map.metadata.ty, (float)map.metadata.tz), 
                    new Quaternion((float)map.metadata.qx, (float)map.metadata.qy, (float)map.metadata.qz, (float)map.metadata.qw), 
                    new Vector3((float)map.metadata.scale, (float)map.metadata.scale, (float)map.metadata.scale)
                );
                Matrix4x4 a = ARHelper.SwitchHandedness(b);
                go.transform.localPosition = a.GetColumn(3);
                go.transform.localRotation = a.rotation;
                go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }

			ARMap arMap = go.AddComponent<ARMap>();
            arMap.mapName = map.metadata.name;
            arMap.privacy = map.metadata.privacy;

            arMap.mapAlignment.tx = map.metadata.tx;
            arMap.mapAlignment.ty = map.metadata.ty;
            arMap.mapAlignment.tz = map.metadata.tz;
            arMap.mapAlignment.qx = map.metadata.qx;
            arMap.mapAlignment.qy = map.metadata.qy;
            arMap.mapAlignment.qz = map.metadata.qz;
            arMap.mapAlignment.qw = map.metadata.qw;

            // TODO: Scale support
            arMap.mapAlignment.scale = 1.0;

            arMap.wgs84.latitude = map.metadata.latitude;
            arMap.wgs84.longitude = map.metadata.longitude;
            arMap.wgs84.altitude = map.metadata.altitude;
            
            arMap.pointColor = pointCloudColor;
            arMap.renderMode = renderMode;

            await arMap.LoadMap(map.mapData, map.metadata.id);

            return arMap;
		}

		public static async Task<ARMap> LoadAndInstantiateARMap(Transform root, SDKJob map, byte[] mapData = null, ARMap.RenderMode renderMode = ARMap.RenderMode.DoNotRender, Color pointCloudColor = default, bool applyAlignment = false)
		{
			GameObject go = new GameObject(string.Format("AR Map {0}-{1}", map.id, map.name));
            if (root != null)
            {
                go.transform.SetParent(root, false);
            }

			ARMap arMap = go.AddComponent<ARMap>();
            arMap.mapName = map.name;
            arMap.privacy = map.privacy;
            arMap.pointColor = pointCloudColor;
            arMap.renderMode = renderMode;

            JobMapMetadataGetAsync j = new JobMapMetadataGetAsync();
            j.id = map.id;
            j.token = map.privacy == (int)SDKJobPrivacy.Private ? ImmersalSDK.Instance.developerToken : "";
            j.OnResult += (SDKMapMetadataGetResult metadata) => 
            {
                if (metadata.error == "none")
                {
                    arMap.mapAlignment.tx = metadata.tx;
                    arMap.mapAlignment.ty = metadata.ty;
                    arMap.mapAlignment.tz = metadata.tz;
                    arMap.mapAlignment.qx = metadata.qx;
                    arMap.mapAlignment.qy = metadata.qy;
                    arMap.mapAlignment.qz = metadata.qz;
                    arMap.mapAlignment.qw = metadata.qw;

                    // TODO: Scale support
                    arMap.mapAlignment.scale = 1.0;

                    arMap.wgs84.latitude = metadata.latitude;
                    arMap.wgs84.longitude = metadata.longitude;
                    arMap.wgs84.altitude = metadata.altitude;

                    if (applyAlignment)
                    {
                        Matrix4x4 b = Matrix4x4.TRS(new Vector3((float)metadata.tx, (float)metadata.ty, (float)metadata.tz), 
                            new Quaternion((float)metadata.qx, (float)metadata.qy, (float)metadata.qz, (float)metadata.qw), 
                            new Vector3((float)metadata.scale, (float)metadata.scale, (float)metadata.scale)
                        );
                        Matrix4x4 a = ARHelper.SwitchHandedness(b);
                        go.transform.localPosition = a.GetColumn(3);
                        go.transform.localRotation = a.rotation;
                        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                }
            };

            await j.RunJobAsync();

            await arMap.LoadMap(mapData, map.id);

            return arMap;
		}

        public static void RegisterSpace(Transform tr, ARMap map, Vector3 offsetPosition, Quaternion offsetRotation, Vector3 offsetScale)
		{
			if (tr == null)
				return;
			
            SpaceContainer sc;

            if (!transformToSpace.ContainsKey(tr))
            {
                sc = new SpaceContainer();
                transformToSpace[tr] = sc;
            }
            else
            {
                sc = transformToSpace[tr];
            }

            spaceToTransform[sc] = tr;

            sc.mapCount++;

            MapOffset mo = new MapOffset();
            mo.position = offsetPosition;
            mo.rotation = offsetRotation;
            mo.scale = offsetScale;
            mo.space = sc;

            mapIdToOffset[map.mapId] = mo;
            mapIdToMap[map.mapId] = map;
		}

        public static void RegisterSpace(Transform tr, ARMap map)
        {
            RegisterSpace(tr, map, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void UnregisterSpace(Transform tr, int mapId)
		{
			if (tr == null)
				return;
			
			if (transformToSpace.ContainsKey(tr))
			{
				SpaceContainer sc = transformToSpace[tr];
				if (--sc.mapCount == 0)
                {
					transformToSpace.Remove(tr);
                    spaceToTransform.Remove(sc);
                }
				if (mapIdToOffset.ContainsKey(mapId))
					mapIdToOffset.Remove(mapId);
                if (mapIdToMap.ContainsKey(mapId))
                    mapIdToMap.Remove(mapId);
			}
		}

		public static void UpdateSpace(SpaceContainer space, Vector3 pos, Quaternion rot)
        {
	        if (space == null)
		        return;
	        
            if (spaceToTransform.ContainsKey(space))
            {
                Transform tr = spaceToTransform[space];
        		tr.SetPositionAndRotation(pos, rot);
            }
		}
    }
}