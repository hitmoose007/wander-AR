using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using Immersal.Samples.ContentPlacement;
using Immersal.Samples.Navigation;


public class NavigationTargetContent : MovableContent
{
    // name of the navigation target
    public override void StoreContent()
    {
        string targetName = this.GetComponent<IsNavigationTarget>().targetName;

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

        Dictionary<string, object> targetNameData = new Dictionary<string, object>
        {
            { "targetName", targetName }
        };
        // Prepare the document data
        Dictionary<string, object> documentData = new Dictionary<string, object>
        {
            { "position", positionData },
            { "rotation", rotationData },
            { "targetName", targetNameData },
            {"mapID", StaticData.MapIdContentPlacement}

        };

        // target name

        // Generate a unique document ID or use a specific identifier
        if (m_contentId == null || m_contentId == "")
        {
            m_contentId = System.Guid.NewGuid().ToString();
        }


        // Add or update the document in the "navigation_targets" collection
        // print the document data
        Debug.Log("Document data: " + documentData);
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
