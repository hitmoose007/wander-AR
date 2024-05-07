using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    public bool isActive = false;
    public GameObject userInfoPrefab;
    public Transform userInfoHolder;
    GameObject user;

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
    }

    public void UserButtonClicked()
    {
        if (isActive == false)
        {
            user = Instantiate(userInfoPrefab, userInfoHolder);
            isActive = true;

            // Set username and password text here
            GameObject username = user.transform.GetChild(0).transform.Find("Username Panel").gameObject.transform.Find("Username Input").gameObject;
            username.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Amina Wasif";

            GameObject password = user.transform.GetChild(0).transform.Find("Password Panel").gameObject.transform.Find("Password Input").gameObject;
            password.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "helloworld";
        }
        else
        {
            Destroy(user);        
            isActive = false;
        }
    }
}
