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
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.IO;
using TMPro; // Add this line to import the TextMesh Pro namespace
using NativeGalleryNamespace;
using Firebase.Storage;

using Immersal.Samples.Util;
using UnityEngine.Assertions.Must;

namespace Immersal.Samples.Navigation
{
    public class Node
    {
        public Vector3 position;
        public List<Node> neighbours = new List<Node>();
        public string nodeName;

        public Node(Vector3 position)
        {
            this.position = position;
        }

        public Node() { }

        public Node(Vector3 position, List<Node> neighbours)
        {
            this.position = position;
            this.neighbours = neighbours;
        }

        public float Cost { get; set; }

        public Node Parent { get; set; }
    }

    [System.Serializable]
    public class WaypointData
    {
        public string uniqueId;
        public Vector3 position;

        public List<string> neighbourIds;

        public WaypointData(Waypoint waypoint)
        {
            uniqueId = waypoint.UniqueID;
            neighbourIds = new List<string>();
            foreach (Waypoint neighbour in waypoint.neighbours)
            {
                neighbourIds.Add(neighbour.UniqueID);
            }
        }
    }

    [System.Serializable]
    public class WaypointDataList
    {
        public List<WaypointData> waypoints;

        public WaypointDataList(List<WaypointData> waypoints)
        {
            this.waypoints = waypoints;
        }
    }

    public class NavigationGraphManager : MonoBehaviour
    {
        private Immersal.AR.ARSpace m_ARSpace;

        private FirebaseFirestore db;

        public class LineSegment
        {
            public Vector3 startPosition;
            public Vector3 endPosition;

            public LineSegment(Vector3 start, Vector3 end)
            {
                this.startPosition = start;
                this.endPosition = end;
            }
        }

        public class Edge
        {
            public Node start;
            public Node end;

            public Edge(Node start, Node end)
            {
                this.start = start;
                this.end = end;
            }
        }

        [SerializeField]
        private float startYOffset = -0.25f;

        [SerializeField]
        GameObject waypointPrefab;
        private List<Waypoint> m_Waypoints = new List<Waypoint>();
        private List<IsNavigationTarget> m_NavigationTargets = new List<IsNavigationTarget>();

        private Mesh m_Mesh;
        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;

        private Immersal.AR.ARSpace m_ArSpace = null;

        #region singleton pattern
        private static NavigationGraphManager instance = null;
        public static NavigationGraphManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null && !Application.isPlaying)
                {
                    instance = FindObjectOfType<NavigationGraphManager>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError(
                        "No NavigationGraphManager instance found. Ensure one exists in the scene."
                    );
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one NavigationGraphManager in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }
            if (m_ARSpace == null)
            {
                m_ARSpace = GameObject.FindObjectOfType<Immersal.AR.ARSpace>();
            }
        }
        #endregion

        void Start()
        {
            db = FirebaseFirestore.DefaultInstance;
            InitializeMeshRenderer();

            m_ArSpace = FindObjectOfType<Immersal.AR.ARSpace>();

            LoadWaypoints();
        }

        void Update()
        {
            if (Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode)
            {
                m_MeshRenderer.enabled = true;
                DrawConnections(0.075f, m_Mesh);
            }
            else
            {
                m_MeshRenderer.enabled = false;
            }
        }

        private void InitializeMeshRenderer()
        {
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

        private void DrawConnections(float LineSegmentWidth, Mesh mesh)
        {
            List<LineSegment> lineSegments = new List<LineSegment>();

            foreach (Waypoint wp in m_Waypoints)
            {
                foreach (Waypoint neighbour in wp.neighbours)
                {
                    if (neighbour != null)
                    {
                        LineSegment ls = new LineSegment(wp.position, neighbour.position);
                        lineSegments.Add(ls);
                    }
                }
            }

            if (lineSegments.Count < 1)
            {
                mesh.Clear();
                return;
            }

            int segmentCount = lineSegments.Count;
            int offset = 0;

            Vector3[] vertices = new Vector3[segmentCount * 4];
            Vector2[] uvs = new Vector2[segmentCount * 4];
            int[] triangleIndices = new int[segmentCount * 6];

            foreach (LineSegment ls in lineSegments)
            {
                {
                    Vector3 startPosition = transform.worldToLocalMatrix.MultiplyPoint(
                        ls.startPosition
                    );
                    Vector3 endPosition = transform.worldToLocalMatrix.MultiplyPoint(
                        ls.endPosition
                    );

                    Vector3 billboardUp = Camera.main.transform.forward;
                    //billboardUp = new Vector3(billboardUp.x, 0f, billboardUp.z);

                    Quaternion edgeOrientation = Quaternion.LookRotation(
                        endPosition - startPosition,
                        billboardUp
                    );

                    Matrix4x4 startTransform = Matrix4x4.TRS(
                        startPosition,
                        edgeOrientation,
                        Vector3.one
                    );
                    Matrix4x4 endTransform = Matrix4x4.TRS(
                        endPosition,
                        edgeOrientation,
                        Vector3.one
                    );

                    Vector3[] shape = new Vector3[]
                    {
                        new Vector3(-0.5f, 0f, 0f),
                        new Vector3(0.5f, 0f, 0f)
                    };
                    float[] shapeU = new float[] { 0f, 1f };

                    int verticesOffset = offset * 4;
                    int triangleIndicesOffset = offset * 6;

                    vertices[verticesOffset + 0] = startTransform.MultiplyPoint(
                        shape[0] * LineSegmentWidth
                    );
                    vertices[verticesOffset + 1] = startTransform.MultiplyPoint(
                        shape[1] * LineSegmentWidth
                    );
                    vertices[verticesOffset + 2] = endTransform.MultiplyPoint(
                        shape[1] * LineSegmentWidth
                    );
                    vertices[verticesOffset + 3] = endTransform.MultiplyPoint(
                        shape[0] * LineSegmentWidth
                    );

                    uvs[verticesOffset + 0] = new Vector2(0f, 1f);
                    uvs[verticesOffset + 1] = new Vector2(0f, 0f);
                    uvs[verticesOffset + 2] = new Vector2(1f, 0f);
                    uvs[verticesOffset + 3] = new Vector2(1f, 1f);

                    triangleIndices[triangleIndicesOffset + 0] = verticesOffset + 0;
                    triangleIndices[triangleIndicesOffset + 1] = verticesOffset + 1;
                    triangleIndices[triangleIndicesOffset + 2] = verticesOffset + 2;
                    triangleIndices[triangleIndicesOffset + 3] = verticesOffset + 0;
                    triangleIndices[triangleIndicesOffset + 4] = verticesOffset + 2;
                    triangleIndices[triangleIndicesOffset + 5] = verticesOffset + 3;

                    offset++;
                }

                mesh.Clear();
                mesh.vertices = vertices;
                mesh.uv = uvs;
                mesh.triangles = triangleIndices;
            }
        }

        public void AddWaypoint(Waypoint wp)
        {
            // Check if the waypoint is not null and not already in the list
            if (wp != null && !m_Waypoints.Contains(wp))
            {
                m_Waypoints.Add(wp);

                Dictionary<string, object> positionData = new Dictionary<string, object>
                {
                    { "x", wp.transform.position.x },
                    { "y", wp.transform.position.y },
                    { "z", wp.transform.position.z }
                };

                Dictionary<string, object> documentData = new Dictionary<string, object>
                {
                    { "position", positionData },
                    { "mapID", StaticData.MapIdContentPlacement }
                };

                wp.UniqueID = System.Guid.NewGuid().ToString();
                // Add or update the document in the "waypoint_object" collection
                DocumentReference docRef = db.Collection("waypoint_object").Document(wp.UniqueID);
                docRef
                    .SetAsync(documentData)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            Debug.Log("Content stored successfully!");
                        }
                        else
                        {
                            Debug.LogError("Failed to store content: " + task.Exception.ToString());
                        }
                    });

                SaveWaypoints(); // Save waypoints after adding
            }
            // Debug.Log("AddWaypoint: " + (wp != null ? wp.name : "null"));
        }

        public void RemoveWaypoint(Waypoint wp)
        {
            // Check if the waypoint is in the list and not null
            if (wp != null && m_Waypoints.Contains(wp))
            {
                m_Waypoints.Remove(wp);
                SaveWaypoints(); // Save waypoints after removing
            }
        }

        public void DeleteAllWaypoints()
        {
            // Create a temporary list to avoid modifying the list while iterating
            List<Waypoint> waypointsToRemove = new List<Waypoint>(m_Waypoints);

            foreach (Waypoint wp in waypointsToRemove)
            {
                if (wp != null)
                {
                    Destroy(wp.gameObject); // Destroy the waypoint GameObject
                    // Note: RemoveWaypoint(wp) call is not needed here as m_Waypoints is cleared at the end
                }
            }

            m_Waypoints.Clear(); // Clear the list after all waypoints are destroyed
            SaveWaypoints(); // Save waypoints after clearing the list
        }

        public void AddTarget(IsNavigationTarget target)
        {
            if (!m_NavigationTargets.Contains(target))
            {
                m_NavigationTargets.Add(target);
            }
        }

        public void RemoveTarget(IsNavigationTarget target)
        {
            if (m_NavigationTargets.Contains(target))
            {
                m_NavigationTargets.Remove(target);
            }
        }

        public void DeleteAllNavigationTargets()
        {
            foreach (IsNavigationTarget target in m_NavigationTargets)
            {
                Destroy(target.gameObject);
            }

            m_NavigationTargets.Clear();
        }

        public void SaveWaypoints()
        {
            List<WaypointData> waypointDataList = new List<WaypointData>();
            foreach (Waypoint wp in m_Waypoints)
            {
                waypointDataList.Add(new WaypointData(wp));
            }

            // if (waypointDataList.Count < 1)
            // {
            //     Debug.LogWarning("NAVIGATION GRAPH MANAGER: No waypoints to save");
            //     return;
            // }
            WaypointDataList container = new WaypointDataList(waypointDataList);
            string json = JsonUtility.ToJson(container);

            // Debug.Log("Serialized waypoints JSON: " + json);
            // System.IO.File.WriteAllText(Application.persistentDataPath + "/waypoints.json", json);
        }

        public void LoadWaypoints()
        {
            Query waypointCollectionQuery = db.Collection("waypoint_object")
                .WhereEqualTo("mapID", StaticData.MapIdContentPlacement);

            // if empty, return
            if (waypointCollectionQuery == null)
            {
                Debug.LogWarning("Waypoint object collection not found");
                return;
            }

            waypointCollectionQuery
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {

                    Debug.Log("hello2");
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Error fetching collection documents: " + task.Exception);
                        return;
                    }
                    QuerySnapshot snapshot = task.Result;

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.Exists)
                        {
                            Dictionary<string, object> data = document.ToDictionary();

                            Dictionary<string, object> positionData =
                                data["position"] as Dictionary<string, object>;
                            Vector3 position = new Vector3(
                                System.Convert.ToSingle(positionData["x"]),
                                System.Convert.ToSingle(positionData["y"]),
                                System.Convert.ToSingle(positionData["z"])
                            );

                            // int mapID = Int32.Parse(data["mapID"].ToString());

                            // Debug.Log(" position: " + position + " mapID: " + mapID);

                            // if (mapID == StaticData.MapIdContentPlacement)
                            // {
                            GameObject wpObject = Instantiate(
                                waypointPrefab,
                                position,
                                Quaternion.identity,
                                m_ARSpace.transform // Set m_ARSpace.transform as the parent transform
                            );

                            //set wp inactive
                            wpObject.SetActive(false);
                            Waypoint wp = wpObject.GetComponent<Waypoint>();

                            wp.UniqueID = document.Id;

                            m_Waypoints.Add(wp);
                            // }
                            // else
                            // {
                            //     Debug.Log("Map ID does not match for waypoint: " + document.Id);
                            // }
                        }
                    }

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        try
                        {
                            if (document.Exists)
                            {
                                Dictionary<string, object> data = document.ToDictionary();

                                // int mapID = Int32.Parse(data["mapID"].ToString());

                                // if (data.ContainsKey("neighbours") && mapID == StaticData.MapIdContentPlacement)
                                // {
                                Waypoint wp = m_Waypoints.Find(x => x.UniqueID == document.Id);

                                List<object> neighboursListObj = data["neighbours"] as List<object>;
                                List<string> neighbourIds = neighboursListObj
                                    .Where(obj => obj != null) // Ensure the object is not null
                                    .Select(obj => obj.ToString()) // Convert each object to string
                                    .ToList();

                                neighbourIds.ForEach(neighbourId => Debug.Log(neighbourId));

                                foreach (string neighbourId in neighbourIds)
                                {
                                    wp.neighbours.Add(
                                        m_Waypoints.Find(x => x.UniqueID == neighbourId)
                                    );
                                }
                                // }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                });

            // if (System.IO.File.Exists(path))
            // {
            //     string json = System.IO.File.ReadAllText(path);
            //     WaypointDataList container = JsonUtility.FromJson<WaypointDataList>(json);
            //     foreach (WaypointData waypointData in container.waypoints)
            //     {
            //         GameObject wpObject = Instantiate(
            //             waypointPrefab,
            //             waypointData.position,
            //             Quaternion.identity,
            //             m_ARSpace.transform // Set m_ARSpace.transform as the parent transform
            //         );
            //         //log out the nieghbors
            //         foreach (string neighbor in waypointData.neighbourIds)
            //         {
            //             Debug.Log("neighbor: " + neighbor);
            //         }

            //         Waypoint wp = wpObject.GetComponent<Waypoint>();
            //         wp.UniqueID = waypointData.uniqueId;
            //         //add neighbor ids to list


            //         m_Waypoints.Add(wp);
            //         // Add wp to m_Waypoints list and reconstruct neighbours...
            //     }

            //     foreach (WaypointData waypointData in container.waypoints)
            //     {
            //         foreach (Waypoint waypointObject in m_Waypoints)
            //         {
            //             if (waypointData.uniqueId == waypointObject.UniqueID)
            //             {
            //                 Debug.Log("inside the real mom ");
            //                 foreach (string neighborId in waypointData.neighbourIds)
            //                 {
            //                     Debug.Log("inside deeply the real mom ");
            //                     waypointObject.neighbours.Add(
            //                         m_Waypoints.Find(x => x.UniqueID == neighborId)
            //                     );
            //                 }
            //             }
            //         }
            //     }

            //     // Reconstruct the neighbors for each waypoint...
            // }
        }

        public List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition)
        {
            List<Vector3> pathPositions = new List<Vector3>();

            if (m_Waypoints.Count < 1)
            {
                return pathPositions;
            }

            //
            // Convert a list of Waypoints to a dictionary of Nodes and neighbours and build the graph.
            //

            Dictionary<Waypoint, Node> link = new Dictionary<Waypoint, Node>();
            Dictionary<Node, List<Node>> graph = new Dictionary<Node, List<Node>>();

            foreach (Waypoint wp in m_Waypoints)
            {
                Node n = new Node(wp.position);
                n.nodeName = wp.name;
                link[wp] = n;
            }

            foreach (Waypoint wp in link.Keys)
            {
                List<Node> nodes = new List<Node>();

                foreach (Waypoint neighbour in wp.neighbours)
                {
                    nodes.Add(link[neighbour]);
                }

                graph[link[wp]] = nodes;
            }

            //
            // Add in and out Nodes for Main Camera and Navigation Target (start position and end position)
            //

            Node startNode = new Node(startPosition + new Vector3(0f, startYOffset, 0f));
            startNode.nodeName = "GRAPH - START NODE";
            Node inNode = new Node();
            inNode.nodeName = "GRAPH - IN NODE";
            EdgeToNearestPointInGraph(ref startNode, ref inNode, ref graph);

            Node endNode = new Node(endPosition);
            endNode.nodeName = "GRAPH - END NODE";

            // GetClosestNode(ref startNode, ref graph);

            // GetClosestNode(ref endNode, ref graph);
            Node outNode = new Node();
            outNode.nodeName = "GRAPH - OUT NODE";
            EdgeToNearestPointInGraph(ref endNode, ref outNode, ref graph);

            // string json = JsonUtility.ToJson(graph);
            // Debug.Log("Graph JSON: " + json);
            // Debug.Log("helloooo");
            // Debug.Log("Graph Contents:");
            // string graphLog = "Graph Contents:\n";
            // foreach (KeyValuePair<Node, List<Node>> pair in graph)
            // {
            //     graphLog += $"Key: {pair.Key.nodeName}\nNeighbors: ";
            //     foreach (Node neighbor in pair.Value)
            //     {
            //         graphLog += $"{neighbor.nodeName}, ";
            //     }
            //     graphLog = graphLog.TrimEnd(',', ' '); // Remove trailing comma and space
            //     graphLog += "\n";
            // }
            // Debug.Log(graphLog);

            pathPositions = GetPathPositions(startNode, endNode, graph);

            //log pathpositions

            if (pathPositions.Count < 2)
            {
                Debug.LogWarning("NAVIGATION GRAPH MANAGER: Path could not be found");
                return pathPositions;
            }

            return pathPositions;
        }

        private void GetClosestNode(ref Node searchNode, ref Dictionary<Node, List<Node>> graph)
        {
            float min = Mathf.Infinity;
            Node nearestNode = null;

            foreach (Node node in graph.Keys)
            {
                float dist = (node.position - searchNode.position).magnitude;

                if (dist < min)
                {
                    min = dist;
                    nearestNode = node;
                }
            }

            searchNode.neighbours.Add(nearestNode);
            graph[nearestNode].Add(searchNode);
        }

        private void EdgeToNearestPointInGraph(
            ref Node searchNode,
            ref Node insertNode,
            ref Dictionary<Node, List<Node>> graph
        )
        {
            Vector3 nodePos = Vector3.zero;
            float min = Mathf.Infinity;

            List<Edge> edges = new List<Edge>();
            foreach (KeyValuePair<Node, List<Node>> k in graph)
            {
                foreach (Node neighbour in k.Value)
                {
                    Edge edge = new Edge(k.Key, neighbour);
                    edges.Add(edge);
                }
            }

            Edge nearestEdge = null;
            foreach (Edge edge in edges)
            {
                Vector3 pos;
                Vector3 p = searchNode.position;
                Vector3 a = edge.start.position;
                Vector3 b = edge.end.position;

                Vector3 ap = new Vector3(p.x - a.x, p.y - a.y, p.z - a.z);
                Vector3 ab = new Vector3(b.x - a.x, b.y - a.y, b.z - a.z);

                float d2 = Vector3.SqrMagnitude(ab);
                float t =
                    (
                        (p.x - a.x) * (b.x - a.x)
                        + (p.y - a.y) * (b.y - a.y)
                        + (p.z - a.z) * (b.z - a.z)
                    ) / d2;

                if (t < 0f || t > 1f)
                {
                    if (Vector3.SqrMagnitude(a - p) < Vector3.SqrMagnitude(b - p))
                    {
                        pos = a;
                    }
                    else
                    {
                        pos = b;
                    }
                }
                else
                {
                    pos = a + Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab) * ab;
                }

                float currentDistance = Vector3.SqrMagnitude(pos - p);
                if (currentDistance < min)
                {
                    min = currentDistance;
                    nodePos = pos;
                    nearestEdge = edge;
                }
            }

            insertNode.position = nodePos;

            insertNode.neighbours.Add(nearestEdge.start);
            insertNode.neighbours.Add(nearestEdge.end);

            graph[nearestEdge.start].Add(insertNode);
            graph[nearestEdge.end].Add(insertNode);

            graph[nearestEdge.start].Remove(nearestEdge.end);
            graph[nearestEdge.end].Remove(nearestEdge.start);

            searchNode.neighbours.Add(insertNode);
            insertNode.neighbours.Add(searchNode);

            graph[insertNode] = insertNode.neighbours;
            graph[searchNode] = searchNode.neighbours;
        }

        public List<Vector3> GetPathPositions(
            Node startNode,
            Node endNode,
            Dictionary<Node, List<Node>> graph
        )
        {
            List<Vector3> pathPositions = new List<Vector3>();
            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node currNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].Cost < currNode.Cost)
                    {
                        currNode = openList[i];
                    }
                }

                openList.Remove(currNode);
                closedList.Add(currNode);

                if (currNode == endNode)
                {
                    pathPositions = NodesToPathPositions(startNode, endNode);
                    return pathPositions;
                }

                foreach (Node n in graph[currNode])
                {
                    if (!closedList.Contains(n))
                    {
                        float totalCost = currNode.Cost + n.Cost;
                        if (totalCost < n.Cost || !openList.Contains(n))
                        {
                            n.Cost = totalCost;
                            n.Parent = currNode;

                            if (!openList.Contains(n))
                            {
                                openList.Add(n);
                            }
                        }
                    }
                }
            }

            return pathPositions;
        }

        private List<Vector3> NodesToPathPositions(Node startNode, Node endNode)
        {
            List<Vector3> pathPositions = new List<Vector3>();
            Node currNode = endNode;

            while (currNode != startNode)
            {
                pathPositions.Add(currNode.position);
                currNode = currNode.Parent;
            }

            pathPositions.Add(startNode.position);
            pathPositions.Reverse();

            return pathPositions;
        }
    }
}
