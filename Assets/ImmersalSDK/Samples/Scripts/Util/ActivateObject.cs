using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public GameObject ContentUI;
    public GameObject NavUI;
    public GameObject WaypointUI;

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
        ContentUI.SetActive(true);
        NavUI.SetActive(true);
        WaypointUI.SetActive(true);
    }
}
