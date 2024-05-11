using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using Immersal.Samples.ContentPlacement;

public class NavigationTargetContent : MovableContent
{
    public override void StoreContent()
    {
        // Serialize the position to a format suitable for Firestore
        Vector3 position = transform.position;
        Dictionary<string, object> positionData = new Dictionary<string, object>
        {
            { "x", position.x },
            { "y", position.y },
            { "z", position.z }
        };

        // Serialize the rotation to a format suitable for Firestore
        Quaternion rotation = transform.rotation;
        Dictionary<string, object> rotationData = new Dictionary<string, object>
        {
            { "x", rotation.x },
            { "y", rotation.y },
            { "z", rotation.z },
            { "w", rotation.w }
        };

        // Prepare the document data
        Dictionary<string, object> documentData = new Dictionary<string, object>
        {
            { "position", positionData },
            { "rotation", rotationData },
            {"mapID", StaticData.MapIdContentPlacement}
        };

        // Generate a unique document ID or use a specific identifier
        if (m_contentId == null || m_contentId == "")
        {
            m_contentId = System.Guid.NewGuid().ToString();
        }

        // Add or update the document in the "navigation_targets" collection
        DocumentReference docRef = db.Collection("navigation_targets").Document(m_contentId);
        docRef
            .SetAsync(documentData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("Navigation target stored successfully!");
                }
                else
                {
                    Debug.LogError("Failed to store navigation target: " + task.Exception.ToString());
                }
            });
    }
}
