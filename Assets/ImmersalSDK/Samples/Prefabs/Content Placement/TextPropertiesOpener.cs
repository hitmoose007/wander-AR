using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPropertiesOpener : MonoBehaviour
{
    public GameObject Panel1;
    public GameObject Panel2;
    public GameObject Panel3;
    public GameObject Panel4;

    private Button button;
    private Text buttonText;
    public GameObject buttonBackgroundPanel1;
    public GameObject buttonBackgroundPanel2;
    public GameObject buttonBackgroundPanel3;
    public GameObject buttonBackgroundPanel4;

    public Button addTextButton;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        buttonText = button.GetComponentInChildren<Text>(); // Get the Text component inside the button
        buttonBackgroundPanel1.SetActive(true);
    }

    public void OpenPanel()
    {
        Debug.Log("Open Panel");

        // Close panels 2, 3, and 4
        ClosePanel(Panel2);
        ClosePanel(Panel3);
        ClosePanel(Panel4);

        // Toggle the state of Panel1
        if (Panel1 != null)
        {
            bool isActive = Panel1.activeSelf;
            Panel1.SetActive(!isActive);
            
            // Change the button text based on the panel state
            if (buttonText != null)
            {
                buttonText.text = isActive ? "T" : "X";
            }
        }
    }

    // Close a panel if it is active
    private void ClosePanel(GameObject panel)
    {
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
        }
    }

    public void EnableButtonBackground()
    {
        if(name == "InputOpen")
        {
            buttonBackgroundPanel1.SetActive(true);
            buttonBackgroundPanel2.SetActive(false);
            buttonBackgroundPanel3.SetActive(false);
            buttonBackgroundPanel4.SetActive(false);

            addTextButton.transform.localScale = new Vector3(1,1,1);
        }

        else if (name == "FontOpen")
        {
            buttonBackgroundPanel1.SetActive(false);
            buttonBackgroundPanel2.SetActive(true);
            buttonBackgroundPanel3.SetActive(false);
            buttonBackgroundPanel4.SetActive(false);

            addTextButton.transform.localScale = new Vector3(0,0,0);
        }

        else if (name == "ColorOpen")
        {
            buttonBackgroundPanel1.SetActive(false);
            buttonBackgroundPanel2.SetActive(false);
            buttonBackgroundPanel3.SetActive(true);
            buttonBackgroundPanel4.SetActive(false);

            addTextButton.transform.localScale = new Vector3(0,0,0);
        }
        else if (name == "StyleOpen")
        {
            buttonBackgroundPanel1.SetActive(false);
            buttonBackgroundPanel2.SetActive(false);
            buttonBackgroundPanel3.SetActive(false);
            buttonBackgroundPanel4.SetActive(true);

            addTextButton.transform.localScale = new Vector3(0,0,0);
        }

    }
}

