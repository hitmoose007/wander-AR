using UnityEngine;

public class DistanceBasedRendering : MonoBehaviour
{
    public Camera cameraTransform;
    public float renderDistance = 4f;
    private Renderer objectRenderer; // Reference to the Renderer component

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main;
        }

        // Try to get the Renderer component attached to this GameObject
        objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (objectRenderer != null)
        {
            float distance = Vector3.Distance(
                cameraTransform.transform.position,
                transform.position
            );

            // Enable or disable the Renderer based on the distance
            objectRenderer.enabled = (distance <= renderDistance);
        }
    }
}
