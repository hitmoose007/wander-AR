using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBottomPanel : MonoBehaviour
{
    public void TaskOnClick()
    {
        if (name == "UploadImage Button" || name == "AddWaypoint Button")
        {
            transform.parent.transform.GetChild(0).gameObject.SetActive(true);
            transform.parent.transform.GetChild(1).gameObject.SetActive(false);
            transform.parent.transform.GetChild(2).gameObject.SetActive(false);
        }

        else if (name == "AddText Button" || name == "AddTarget Button")
        {
            transform.parent.transform.GetChild(0).gameObject.SetActive(false);
            transform.parent.transform.GetChild(1).gameObject.SetActive(true);
            transform.parent.transform.GetChild(2).gameObject.SetActive(false);
        }

        else if (name == "EditText Button" || name == "Settings Button")
        {
            transform.parent.transform.GetChild(0).gameObject.SetActive(false);
            transform.parent.transform.GetChild(1).gameObject.SetActive(false);
            transform.parent.transform.GetChild(2).gameObject.SetActive(true);
        }
    }
}
