using UnityEngine;

using Firebase.Firestore;
using System.Collections.Generic;

public class ScaleObject : MonoBehaviour
{
    public float scaleSpeed = 0.025f; // Control the speed of scaling
    public GameObject prefab;
    private Vector3 originalPrefabScaleMin;
    private Vector3 originalPrefabScaleMax;

    //add firestore db
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        // Calculate 75% of the prefab's original scale as the minimum allowed scale
        originalPrefabScaleMin = prefab.transform.localScale * 0.75f;
        originalPrefabScaleMax = prefab.transform.localScale * 1.4f;
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            // Convert touch positions into rays
            Ray ray1 = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            Ray ray2 = Camera.main.ScreenPointToRay(Input.GetTouch(1).position);

            RaycastHit hit;

            // Check if both rays hit the collider of this GameObject
            if (
                Physics.Raycast(ray1, out hit)
                && hit.transform == transform
                && Physics.Raycast(ray2, out hit)
                && hit.transform == transform
            )
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

                // Apply scaling
                Vector3 scaleChange =
                    new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, 0)
                    * scaleSpeed
                    * Time.deltaTime;
                Vector3 newScale = transform.localScale + scaleChange;

                // Maintain the original Z scale of the object
                newScale.z = transform.localScale.z;

                // Apply the minimum scale limit based on the original prefab scale
                newScale.x = Mathf.Max(newScale.x, originalPrefabScaleMin.x);
                newScale.y = Mathf.Max(newScale.y, originalPrefabScaleMin.y);

                newScale.x = Mathf.Min(newScale.x, originalPrefabScaleMax.x);
                newScale.y = Mathf.Min(newScale.y, originalPrefabScaleMax.y);

                transform.localScale = newScale;

                //store scale in dictionary

                Dictionary<string, object> scaleDictionary = new Dictionary<string, object>
                {
                    { "scale_x", newScale.x },
                    { "scale_y", newScale.y }
                };

                string content_id = GetComponent<MovableTextContent>().m_contentId;
                if (content_id != null && content_id != "")
                {
                    db.Collection("text_content")
                        .Document(content_id)
                        .SetAsync(scaleDictionary)
                        .ContinueWith(task =>
                        {
                            if (task.IsCompleted && !task.IsFaulted)
                            {
                                Debug.Log("Scale stored successfully!");
                            }
                            else
                            {
                                Debug.LogError(
                                    "Failed to store scale: " + task.Exception.ToString()
                                );
                            }
                        });
                }
            }
        }
    }
}
