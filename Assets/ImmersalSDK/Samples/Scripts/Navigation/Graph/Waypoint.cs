/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Firebase;
using Firebase.Firestore;

namespace Immersal.Samples.Navigation
{
    public class Waypoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public List<Waypoint> neighbours = new List<Waypoint>();

        public Vector3 position
        {
            get { return m_collider.bounds.center; }
            set { }
        }
        public string UniqueID;
        private FirebaseFirestore db;

        [SerializeField]
        private float m_ClickHoldTime = 1f;
        private float m_timeHeld = 0f;

        private Collider m_collider = null;
        private bool isPressed = false;
        private bool isEditing = false;

        private Camera m_mainCamera = null;
        private float m_DragPlaneDistance = 0f;

        private MeshFilter m_MeshFilter = null;
        private MeshRenderer m_MeshRenderer = null;
        private Mesh m_Mesh = null;


        void Start()
        {
            UniqueID = System.Guid.NewGuid().ToString();
            db = FirebaseFirestore.DefaultInstance;
            m_mainCamera = Camera.main;


            InitializeNode();
            NavigationGraphManager.Instance.AddWaypoint(this);
        }

        private void InitializeNode()
        {
            m_collider = GetComponent<Collider>();
            if (m_collider == null)
                m_collider = gameObject.AddComponent<SphereCollider>();

            if (m_Mesh == null)
                m_Mesh = new Mesh();

            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            if (m_MeshFilter == null)
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();

            if (m_MeshRenderer == null)
                m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

            m_MeshFilter.mesh = m_Mesh;
        }

        void Update()
        {
            if (isPressed)
            {
                Vector3 projection = m_mainCamera.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_DragPlaneDistance)
                );
                DrawPreviewConnection(position, projection, 0.1f, m_Mesh);

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Waypoint wp = hit.collider.GetComponent<Waypoint>();
                        if (wp == this)
                        {
                            m_timeHeld += Time.deltaTime;
                        }
                        else
                        {
                            m_timeHeld = 0f;
                        }
                    }
                }

                if (m_timeHeld >= m_ClickHoldTime && !isEditing)
                {
                    isEditing = true;
                }

                if (isEditing)
                {
                    transform.position = projection;
                }
            }
        }

        private void DrawPreviewConnection(
            Vector3 startPosition,
            Vector3 endPosition,
            float lineWidth,
            Mesh mesh
        )
        {
            Vector3 camPos = m_mainCamera.transform.position;
            float length = (endPosition - startPosition).magnitude;

            List<Vector3> points = new List<Vector3>();
            points.Add(startPosition);
            points.Add(endPosition);

            Vector3[] x = new Vector3[2];
            Vector3[] z = new Vector3[2];
            Vector3[] y = new Vector3[2];
            Quaternion[] q = new Quaternion[2];
            Matrix4x4[] m = new Matrix4x4[2];

            z[0] = (endPosition - startPosition).normalized;
            z[1] = (startPosition - endPosition).normalized;

            y[0] = (camPos - startPosition).normalized;
            y[1] = (camPos - endPosition).normalized;

            x[0] = Vector3.Cross(y[0], z[0]);
            x[1] = Vector3.Cross(y[1], -z[1]);

            //Debug.DrawRay(startPosition, x[0] * 0.4f, Color.red);
            //Debug.DrawRay(endPosition, x[0] * 0.4f, Color.red);
            //Debug.DrawRay(startPosition, y[0] * 0.4f, Color.green);
            //Debug.DrawRay(endPosition, y[0] * 0.4f, Color.green);
            //Debug.DrawRay(startPosition, z[0] * 0.4f, Color.blue);
            //Debug.DrawRay(endPosition, z[0] * 0.4f, Color.blue);

            Vector3[] vtx = new Vector3[4];
            int[] idx = new int[6];
            Vector2[] uv = new Vector2[4];

            vtx[0] = transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(startPosition.x, startPosition.y, startPosition.z)
                    - (x[0] * lineWidth * 0.5f)
            );
            vtx[1] = transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(endPosition.x, endPosition.y, endPosition.z) - (x[1] * lineWidth * 0.5f)
            );
            vtx[2] = transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(endPosition.x, endPosition.y, endPosition.z) + (x[1] * lineWidth * 0.5f)
            );
            vtx[3] = transform.worldToLocalMatrix.MultiplyPoint(
                new Vector3(startPosition.x, startPosition.y, startPosition.z)
                    + (x[0] * lineWidth * 0.5f)
            );

            idx[0] = 0;
            idx[1] = 1;
            idx[2] = 3;
            idx[3] = 1;
            idx[4] = 2;
            idx[5] = 3;

            uv[0] = new Vector2(0f, 0f);
            uv[1] = new Vector2(1f, 0f);
            uv[2] = new Vector2(1f, 1f);
            uv[3] = new Vector2(0f, 1f);

            mesh.Clear();
            mesh.vertices = vtx;
            mesh.triangles = idx;
            mesh.uv = uv;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode)
            {
                isPressed = true;
                m_DragPlaneDistance =
                    Vector3.Dot(
                        transform.position - m_mainCamera.transform.position,
                        m_mainCamera.transform.forward
                    ) / m_mainCamera.transform.forward.sqrMagnitude;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isEditing)
            {
                db.Collection("waypoint_object")
                    .Document(UniqueID)
                    .SetAsync(
                        new Dictionary<string, object>
                        {
                            {
                                "position",
                                new Dictionary<string, object>
                                {
                                    { "x", transform.position.x },
                                    { "y", transform.position.y },
                                    { "z", transform.position.z }
                                }
                            },
                            {
                                "mapID",
                                StaticData.MapIdContentPlacement
                            }
                        }
                    );

                NavigationGraphManager.Instance.SaveWaypoints();
            }
            isPressed = false;
            isEditing = false;
            m_timeHeld = 0f;

            m_Mesh.Clear();

            // Add neigbours whichever hits collider and vice versa
            if (Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Waypoint wp = hit.collider.GetComponent<Waypoint>();
                        if (wp != null && wp != this)
                        {
                            if (!neighbours.Contains(wp))
                            {
                                neighbours.Add(wp);
                                var update = FieldValue.ArrayUnion(wp.UniqueID);
                                db.Collection("waypoint_object")
                                    .Document(UniqueID)
                                    .UpdateAsync("neighbours", update);

                                NavigationGraphManager.Instance.SaveWaypoints();
                            }
                            if (!wp.neighbours.Contains(this))
                            {
                                var update = FieldValue.ArrayUnion(this.UniqueID);
                                db.Collection("waypoint_object")
                                    .Document(wp.UniqueID)
                                    .UpdateAsync("neighbours", update);
                                wp.neighbours.Add(this);
                                NavigationGraphManager.Instance.SaveWaypoints();
                            }
                        }
                    }
                }
            }
        }
    }
}
