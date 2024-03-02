using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeScreen : MonoBehaviour
{
    public GameObject panelA;
    public GameObject panelB;
    public bool isEnabled = true;

    //Start is called before the first frame update
    void Start()
    {
        panelA.SetActive(true);
        panelB.SetActive(false);
    }

    // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void AddButtonClicked()
    {
        panelA.SetActive(false);
        panelB.SetActive(true);
    }

    public void BackButtonClicked()
    {
        panelB.SetActive(false);
        panelA.SetActive(true);
    }

}
