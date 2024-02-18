using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    public float scaleSpeed = 0.05f; // Control the speed of scaling
    public GameObject prefab;
    private Vector3 originalPrefabScale;

    void Start()
    {
        originalPrefabScale = prefab.transform.localScale * 0.75f;
    }

    void Update()
    {
        Debug.Log("Touch count: " + Input.touchCount);

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
                Debug.Log("Pinch detected on object");

                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                // Apply scalingmn
                Vector3 newScale =
                    transform.localScale
                    - new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff)
                        * scaleSpeed
                        * Time.deltaTime;
                newScale = new Vector3(
                    Mathf.Max(newScale.x, originalPrefabScale.x),
                    Mathf.Max(newScale.y, originalPrefabScale.y),
                    Mathf.Max(newScale.z, originalPrefabScale.z)
                ); // Prevent negative scale
                transform.localScale = newScale;
            }
        }
    }
}
