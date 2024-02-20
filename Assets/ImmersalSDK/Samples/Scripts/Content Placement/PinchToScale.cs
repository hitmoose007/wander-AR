using UnityEngine;

using Firebase.Firestore;
using System.Collections.Generic;

public class ScaleObject : MonoBehaviour
{
    public float scaleSpeed = 0.025f; // Control the speed of scaling
    public GameObject prefab;
    private float originalPrefabScaleMin_x;

    private float originalPrefabScaleMin_y;
    private float originalPrefabScaleMax_x;
    private float originalPrefabScaleMax_y;

    //add firestore db
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        // Calculate 75% of the prefab's original scale as the minimum allowed scale
        originalPrefabScaleMin_x = prefab.transform.localScale.x * 0.75f;
        originalPrefabScaleMin_y = prefab.transform.localScale.y * 0.75f;
        originalPrefabScaleMax_x = prefab.transform.localScale.x * 1.8f;
        originalPrefabScaleMax_y = prefab.transform.localScale.y * 1.8f;
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
                //check if one of them hits max scale limit then stop scaling

                if (
                    newScale.x >= originalPrefabScaleMax_x || newScale.y >= originalPrefabScaleMax_y
                )
                {
                    return;
                }
                newScale.x = Mathf.Max(newScale.x, originalPrefabScaleMin_x);
                newScale.y = Mathf.Max(newScale.y, originalPrefabScaleMin_y);

                newScale.x = Mathf.Min(newScale.x, originalPrefabScaleMax_x);
                newScale.y = Mathf.Min(newScale.y, originalPrefabScaleMax_y);

                transform.localScale = newScale;

                //store scale in dictionary

                //scale is being saved in firebase in the MovableContent.cs storeContent() method

            }
        }
    }
}
