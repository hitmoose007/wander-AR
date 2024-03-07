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
            }

            if (db == null)
            {
                Debug.LogError("Firestore not found");
            }
        }

        private void Update()
        {
            if (m_EditingContent)
            {
                if (Input.touchCount > 1)
                {
                    return;
                }
                Vector3 projection = Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_MovePlaneDistance)
                );
                transform.position = projection;
            }
        }

        public abstract void StoreContent();

        public void RemoveContent()
        {
            if (this is MovableImageContent)
                db.Collection("image_content").Document(m_contentId).DeleteAsync();
            else if (this is MovableTextContent)
                db.Collection("text_content").Document(m_contentId).DeleteAsync();

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

            // Change rotation to face the user (camera)
            if (m_EditingContent)
            {
                // Calculate the direction from the object to the camera
                Vector3 directionToCamera = transform.position - m_CameraTransform.position;

                // Create a new rotation that looks in the direction of the camera but stays upright.
                // Assuming that the camera's up direction is the global up direction (Vector3.up).
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

                // Apply the rotation to the object. You can also use Quaternion.Slerp for a smoother transition:
                // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                transform.rotation = targetRotation;
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
