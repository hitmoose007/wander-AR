/*===============================================================================
Copyright (C) 2023 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using UnityEngine;

namespace Immersal.Samples.ContentPlacement
{
    public class MovableContent : MonoBehaviour
    {
        [SerializeField]
        private float m_ClickHoldTime = 0.1f;

        public enum ContentType
        {
            Image,
            Text
        };

        [SerializeField]
        public ContentType contentType;
        private float m_timeHold = 0f;

        private bool m_EditingContent = false;

        private Transform m_CameraTransform;
        private float m_MovePlaneDistance;

        //add enum for two types of content

        private void Start()
        {
            m_CameraTransform = Camera.main.transform;
            if (this.contentType == ContentType.Image)
            {
                return;
            }
            StoreContent();
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

        private void StoreContent()
        {
            // if (this.contentType == MovableContent.ContentType.Image)
            // {
            //     return;
            // }
            if (!ContentStorageManager.Instance.contentList.Contains(this))
            {
                ContentStorageManager.Instance.contentList.Add(this);
            }
            
            ContentStorageManager.Instance.SaveContents();
        }

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
