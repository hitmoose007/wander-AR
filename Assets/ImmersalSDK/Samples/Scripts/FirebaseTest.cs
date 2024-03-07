using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using System;

public class FirebaseTest : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI countUI;
    FirebaseFirestore db;

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        Query allMapsQuery = db.Collection("maps");

        allMapsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allMapsQuerySnapshot = task.Result;

            foreach(DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                Dictionary<string, object> map = documentSnapshot.ToDictionary();
                foreach (KeyValuePair<string, object> pair in map)
                {
                    Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                }
                // Newline to separate entries
                Debug.Log(" ");
            }   
        });

    }
}
