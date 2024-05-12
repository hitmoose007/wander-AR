using UnityEngine;
using TMPro;

namespace TargetName
{
    // Import the namespace where IsNavigationTarget is defined
    using Immersal.Samples.Navigation;

    public class ChangeTargetName : MonoBehaviour
    {
        // Reference to the target GameObject with IsNavigationTarget script
        public GameObject targetObject;

        public GameObject panel;

        // Reference to the TMP Input Field
        public TMP_InputField inputField;

        private IsNavigationTarget navigationTarget;

        private void Start()
        {
            // Check if targetObject has IsNavigationTarget script attached


            // Check if TMP Input Field is assigned
        }

        // Method to update the targetName when the TMP Input Field's value changes
        public void UpdateTargetName()
        {
            Debug.Log("hey babes");
            if (inputField.text == "")
            {
                inputField.text = "Target";
            }

            Debug.Log("this is input field text: " + inputField.text);
            // Update the targetName property of the IsNavigationTarget component
            StaticData.TargetName = inputField.text;
            panel.SetActive(false);

            
        }
    }
}
