/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;
using Firebase.Firestore;
using System.Data.Common;
using System;

namespace Immersal.Samples.ContentPlacement
{
    public abstract class MovableContent : MonoBehaviour
    {
        [SerializeField]
        private float m_ClickHoldTime = 0.1f;

        [HideInInspector]
        public string m_contentId = null;
        private float m_timeHold = 0f;
        protected FirebaseFirestore db;

        private bool m_EditingContent = false;

        private Transform m_CameraTransform;
        private float m_MovePlaneDistance;

        //add enum for two types of content


        //constructor with id passed in
        // public MovableContent(string id)
        // {
        //     m_contentId = id;
        // }

        // public MovableContent() { }

        private void Start()
        {
            db = FirebaseFirestore.DefaultInstance;
            m_CameraTransform = Camera.main.transform;
            //if its not loaded from database wont have any id already
            if (m_contentId == null || m_contentId == "")
            {
                StoreContent();
                Debug.Log("in here lol");
            }
            Debug.Log("not there lol");
            if (db == null)
            {
                Debug.LogError("Firestore not found");
            }
        }

        private void Update()
        {
            if (m_EditingContent)
            {
                Vector3 projection = Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_MovePlaneDistance)
                );
                transform.position = projection;
            }
        }

        public abstract void StoreContent();

        // if (this.contentType == MovableContent.ContentType.Image)
        // {
        //     return;

        public void RemoveContent()
        {
            // if (this.contentType == MovableContent.ContentType.Image)
            // {
            //     return;
            // }
            if (ContentStorageManager.Instance.contentList.Contains(this))
            {
                ContentStorageManager.Instance.contentList.Remove(this);
            }
            ContentStorageManager.Instance.SaveContents();
            Destroy(gameObject);
        }

        private void OnMouseDrag()
        {
            m_timeHold += Time.deltaTime;

            if (m_timeHold >= m_ClickHoldTime && !m_EditingContent)
            {
                m_MovePlaneDistance =
                    Vector3.Dot(
                        transform.position - m_CameraTransform.position,
                        m_CameraTransform.forward
                    ) / m_CameraTransform.forward.sqrMagnitude;
                m_EditingContent = true;
            }
        }

        private void OnMouseUp()
        {
            // if (this.contentType != MovableContent.ContentType.Image)
            // {
            StoreContent();
            m_timeHold = 0f;
            m_EditingContent = false;
            return;
            // }
            //     m_EditingContent = false;
        }
    }
}
