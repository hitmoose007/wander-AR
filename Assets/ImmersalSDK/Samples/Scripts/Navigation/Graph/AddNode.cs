using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Immersal.AR;
using TMPro;

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
            Waypoint,
            Target
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

        bool isPopUpActive = false;
        public GameObject popUpPanel;

        void Start()
        {
            button = GetComponent<Button>();
            mainCamera = Camera.main;
            arspace = FindObjectOfType<ARSpace>();

            if (isPopUpActive == false)
            {
                popUpPanel.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (
                Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode
                && m_NodeToAdd == NodeToAdd.Waypoint
            )
            {
                isPressed = true;
                randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;

            if (
                Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode
                && arspace != null
                && m_NodeToAdd == NodeToAdd.Waypoint
            )
            {
                GameObject finalNodeInstance;

                switch (m_NodeToAdd)
                {
                    case NodeToAdd.Waypoint:
                        finalNodeInstance = Instantiate(waypointObject, arspace.transform);
                        break;
                    case NodeToAdd.Target:
                        finalNodeInstance = Instantiate(targetObject, arspace.transform);
                        // EnablePopUp();
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

        public void CallMeMaybe()
        {
            isPressed = false;

            if (
                Immersal.Samples.Navigation.NavigationManager.Instance.inEditMode && arspace != null
            )
            {
                GameObject finalNodeInstance;

            Debug.Log("this is the call me maybe" + StaticData.TargetName);
                switch (m_NodeToAdd)
                {
                    case NodeToAdd.Waypoint:
                        finalNodeInstance = Instantiate(waypointObject, arspace.transform);
                        break;
                    case NodeToAdd.Target:
                        finalNodeInstance = Instantiate(targetObject, arspace.transform);
                        // EnablePopUp();
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

        public void EnablePopUp()
        {
            if (isPopUpActive == false)
            {
                isPopUpActive = true;
                popUpPanel.SetActive(true);
            }
            else
            {
                isPopUpActive = false;
                popUpPanel.SetActive(false);
            }
        }
    }
}
