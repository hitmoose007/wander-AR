using System.Collections;
using System.Collections.Generic;
using Immersal.Samples.ContentPlacement;
using UnityEngine;
using Firebase;
using Firebase.Firestore;

public class MovableImageContent : MovableContent
{
    public string imageRef = null;

    // Start is called before the first frame update
    //implement the abstract method



    public override void StoreContent()
    {
        // if (!ContentStorageManager.Instance.contentList.Contains(this))
        // {
        //     ContentStorageManager.Instance.contentList.Add(this);
        // }

        // Get the text from the TextMeshPro component

        // Serialize the position to a format suitable for Firestore
        Vector3 position = this.transform.position;
        Dictionary<string, object> positionData = new Dictionary<string, object>
        {
            { "x", position.x },
            { "y", position.y },
            { "z", position.z }
        };
        Dictionary<string, object> rotationData = new Dictionary<string, object>
        {
            { "x", transform.rotation.x },
            { "y", transform.rotation.y },
            { "z", transform.rotation.z },
            { "w", transform.rotation.w }
        };
        //store rotation data

        // Prepare the document data
        Dictionary<string, object> documentData = new Dictionary<string, object>
        {
            { "image_ref", imageRef },
            { "position", positionData },
            { "rotation", rotationData }
        };

        // Generate a unique document ID or use a specific identifier
        // For demonstration, let's use a unique ID for each content
        if (m_contentId == null || m_contentId == "")
        {
            m_contentId = System.Guid.NewGuid().ToString();
        }
        // Add or update the document in the "text_content" collection
        DocumentReference docRef = db.Collection("image_content").Document(m_contentId);
        docRef
            .SetAsync(documentData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    // Debug.Log("Content stored successfully!");
                }
                else
                {
                    Debug.LogError("Failed to store content: " + task.Exception.ToString());
                }
            });
    }
}
