using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Immersal.AR;

namespace Immersal.Samples.Navigation
{
    [RequireComponent(typeof(Button))]
    public class AddNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private GameObject waypointObject = null;
        [SerializeField]
        private GameObject targetObject = null;
        [SerializeField]
        private Material overrideMaterial = null;

        private enum NodeToAdd
        {
            Waypoint, Target
        };
        [SerializeField]
        private NodeToAdd m_NodeToAdd = NodeToAdd.Waypoint;

        private Button button = null;
        private bool isPressed = false;

        private Camera mainCamera = null;

        private Vector3 pos = Vector3.zero;
        private Quaternion rot = Quaternion.identity;
        private Quaternion randomRotation = Quaternion.identity;

        private ARSpace arspace = null;

        void Start()
        {
            button = GetComponent<Button>();
            mainCamera = Camera.main;
            arspace = FindObjectOfType<ARSpace>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode)
            {
                isPressed = true;
                randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;

            if (Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode && arspace != null)
            {
                GameObject finalNodeInstance;

                switch (m_NodeToAdd)
                {
                    case NodeToAdd.Waypoint:
                        finalNodeInstance = Instantiate(waypointObject, arspace.transform);
                        break;
                    case NodeToAdd.Target:
                        finalNodeInstance = Instantiate(targetObject, arspace.transform);
                        break;
                    default:
                        finalNodeInstance = Instantiate(waypointObject, arspace.transform);
                        break;
                }

                pos = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
                Vector3 x = Vector3.Cross(Vector3.up, mainCamera.transform.forward);
                Vector3 z = Vector3.Cross(x, Vector3.up);
                rot = Quaternion.LookRotation(z, Vector3.up) * randomRotation;

                finalNodeInstance.transform.position = pos;
                finalNodeInstance.transform.rotation = rot;
            }
        }
    }
}
