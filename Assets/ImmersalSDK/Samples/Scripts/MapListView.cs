using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System;

public class MapListView : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listItemHolder;

    //public int numOfListItems;
    //List<string> mapNames = new List<string>();
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        Query allMapsQuery = db.Collection("map");

        allMapsQuery
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                QuerySnapshot allMapsQuerySnapshot = task.Result;

                foreach (DocumentSnapshot documentSnapshot in allMapsQuerySnapshot.Documents)
                {
                    Debug.Log(
                        String.Format("Document data for {0} document:", documentSnapshot.Id)
                    );

                    Dictionary<string, object> mapData = documentSnapshot.ToDictionary();
                    GameObject item = Instantiate(listItemPrefab, listItemHolder);

                    if (mapData.ContainsKey("name") == false)
                    {
                        Debug.LogWarning("Map name");
                        continue;
                    }
                    //check if key exists
                    item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = mapData[
                        "name"
                    ].ToString();

                    // Dictionary<string, object> map = documentSnapshot.ToDictionary();
                    // foreach (KeyValuePair<string, object> pair in map)
                    // {
                    //     Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                    // }
                    // // Newline to separate entries
                    // Debug.Log(" ");
                }
            });
    }
}
