using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    public GameObject userInfoPrefab;
    public Transform userInfoHolder;

    public void UserButtonClicked()
    {
        GameObject user = Instantiate(userInfoPrefab, userInfoHolder);

        user.gameObject.transform.Find("");
    }
}
