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

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        buttonText = button.GetComponentInChildren<Text>(); // Get the Text component inside the button
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
}
