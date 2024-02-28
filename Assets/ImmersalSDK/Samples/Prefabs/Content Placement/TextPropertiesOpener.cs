using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPropertiesOpener : MonoBehaviour
{
    public GameObject Panel;
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

        if (Panel != null)
        {
            bool isActive = Panel.activeSelf;
            Panel.SetActive(!isActive);

            // Change the button text based on the panel state
            if (!isActive)
            {
                // Panel is opening, change text to "X"
                if (buttonText != null)
                {
                    Debug.Log("Change text to X");
                    buttonText.text = "X";
                }
            }
            else
            {
                // Panel is closing, change text to "T"
                if (buttonText != null)
                {
                    Debug.Log("Change text to T");
                    buttonText.text = "T";
                }
            }
        }
    }
}
