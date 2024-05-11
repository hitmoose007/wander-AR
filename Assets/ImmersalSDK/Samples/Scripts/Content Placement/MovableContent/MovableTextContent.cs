using Immersal.Samples.ContentPlacement;
using UnityEngine;
using Firebase.Firestore;
using TMPro;
using System.Collections.Generic;

public class MovableTextContent : MovableContent
{
    public override void StoreContent()
    {
        // if (!ContentStorageManager.Instance.contentList.Contains(this))
        // {
        //     ContentStorageManager.Instance.contentList.Add(this);
        // }

        // Get the text from the TextMeshPro component
        string textContent = this.GetComponent<TextMeshPro>().text;
        // get the color from the TextMeshPro component
        Color textColor = this.GetComponent<TextMeshPro>().color;
        // get the font from the TextMeshPro component
        string textFont = this.GetComponent<TextMeshPro>().font.name;
        // get font style from the TextMeshPro component
        string fontStyle = this.GetComponent<TextMeshPro>().fontStyle.ToString();
        Debug.Log("Font Style: " + fontStyle);
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

        //scale
        Dictionary<string, object> scaleData = new Dictionary<string, object>
        {
            { "x", transform.localScale.x },
            { "y", transform.localScale.y },
            { "z", transform.localScale.z }
        };
        // color
        Dictionary<string, object> colorData = new Dictionary<string, object>
        {
            { "r", textColor.r },
            { "g", textColor.g },
            { "b", textColor.b },
            { "a", textColor.a }
        };
        // font
        // Dictionary<string, object> fontData = new Dictionary<string, object>
        // {
        //     { "font", textFont }
        // };

        // Prepare the document data
        Dictionary<string, object> documentData = new Dictionary<string, object>
        {
            { "text", textContent },
            { "rotation", rotationData },
            { "position", positionData },
            { "scale", scaleData },
            { "color", colorData },
            { "font", textFont },
            { "fontStyle", fontStyle }, 
            {"mapID", StaticData.MapIdContentPlacement}

        };

        // Generate a unique document ID or use a specific identifier
        // For demonstration, let's use a unique ID for each content
        if (m_contentId == null || m_contentId == "")
        {
            m_contentId = System.Guid.NewGuid().ToString();
        }
        // Add or update the document in the "text_content" collection
        DocumentReference docRef = db.Collection("text_content").Document(m_contentId);
        docRef
            .SetAsync(documentData)
            .ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("Content stored successfully!");
                }
                else
                {
                    Debug.LogError("Failed to store content: " + task.Exception.ToString());
                }
            });
    }
}
