using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.Networking;
using UnityEngine.UI;
using Immersal.REST;

public class UserInfo : MonoBehaviour
{
    public bool isActive = false;
    public GameObject userPanel;

    FirebaseFirestore db;

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        userPanel.transform.localScale = new Vector3(0,0,0);
        isActive = false;
    }

    public async void FetchUserInfo()
    {
        if (isActive == false)
        {
            isActive = true;
            userPanel.transform.localScale = new Vector3(1,1,1);

            GameObject username = userPanel.transform.GetChild(0).transform.Find("Username Panel").gameObject.transform.Find("Username Input").gameObject;
            GameObject password = userPanel.transform.GetChild(0).transform.Find("Password Panel").gameObject.transform.Find("Password Input").gameObject;
                
            // Set email from static data
            string email = StaticData.userEmail;

            await db.Collection("user")
            .WhereEqualTo("email", email)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error fetching collection documents: " + task.Exception);
                    return;
                }

                QuerySnapshot allUsersQuerySnapshot = task.Result;

                foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
                {
                    if (documentSnapshot.Exists)
                    {
                        Dictionary<string, object> userData = documentSnapshot.ToDictionary();
                        
                        // Retrieve username from Firebase
                        if (userData.ContainsKey("name") == true)
                        {
                            username.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = userData["name"].ToString();   
                        }
                        else
                        {
                            Debug.LogError("No username found for user:" + email);
                        }
                        
                        // Retrieve password from Firebase
                        if (userData.ContainsKey("password") == true)
                        {
                            password.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = userData["password"].ToString();
                        }
                        else
                        {
                            Debug.LogError("No password found for user: " + email);
                        }

                    }
                }
            });
        }
        else
        {
            userPanel.transform.localScale = new Vector3(0,0,0);
       
            isActive = false;
        }
    }

}
