using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public GameObject ContentUI;
    public GameObject NavUI;
    public GameObject WaypointUI;

    public bool activateForEditor = false;

    public void ActivateContentUI()
    {
        ContentUI.SetActive(true);
    }

    public void ActivateNavUI()
    {
        NavUI.SetActive(true);
    }

    public void ActivateWaypointUI()
    {
        WaypointUI.SetActive(true);
    }

    //start
    public void Start()
    {
        if (activateForEditor)
        {
            ContentUI.SetActive(true);
            NavUI.SetActive(true);
            WaypointUI.SetActive(true);
        }
    }
}
