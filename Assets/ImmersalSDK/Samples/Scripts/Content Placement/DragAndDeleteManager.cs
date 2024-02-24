using UnityEngine;
using UnityEngine.EventSystems; // Required for UI interactions
using System.Collections.Generic; // For List<>
using Immersal.Samples.ContentPlacement; // Assuming your movable content component is within this namespace

public class DragAndDeleteManager : MonoBehaviour
{
    public RectTransform deleteButtonRectTransform; // Assign this in the Unity Editor
    private GameObject selectedObject = null;
    private bool isHoveringDeleteButton = false; // Track if hovering over the delete button

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        MovableContent movableContent =
                            hit.collider.gameObject.GetComponent<MovableContent>();
                        if (movableContent != null)
                        {
                            selectedObject = hit.collider.gameObject;
                            Debug.Log("Selected: " + selectedObject.name);
                        }
                    }
                    break;
                // Intentionally fall through to Moved phase to check for hovering immediately
                case TouchPhase.Moved:
                    // Check if hovering over the delete button as the touch moves
                    isHoveringDeleteButton = IsOverDeleteButton(touch.position);
                    // Adjust delete button's scale based on hovering state
                    deleteButtonRectTransform.localScale = isHoveringDeleteButton
                        ? Vector3.one * 1.2f
                        : Vector3.one;
                    break;

                case TouchPhase.Ended:
                    if (selectedObject != null && isHoveringDeleteButton)
                    {
                        selectedObject.GetComponent<MovableContent>().RemoveContent();
                        selectedObject = null; // Reset selection
                    }
                    // Reset delete button size when touch ends
                    deleteButtonRectTransform.localScale = Vector3.one;
                    isHoveringDeleteButton = false;
                    break;
            }
        }
    }

    bool IsOverDeleteButton(Vector2 screenPosition)
    {
        //check if its movable content or else dont enlarge and return
        if (selectedObject == null)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == deleteButtonRectTransform.gameObject)
            {
                return true; // Touch is over the delete button
            }
        }
        return false; // Touch is not over the delete button
    }
}
