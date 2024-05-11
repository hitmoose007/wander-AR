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

        // Reference to the TMP Input Field
        public TMP_InputField inputField;

        private IsNavigationTarget navigationTarget;

        private void Start()
        {
            // Check if targetObject has IsNavigationTarget script attached
            if (targetObject != null)
            {
                // Get the IsNavigationTarget component from targetObject
                navigationTarget = targetObject.GetComponent<IsNavigationTarget>();
                if (navigationTarget == null)
                {
                    Debug.LogError("The targetObject does not have the IsNavigationTarget script attached.");
                }
            }
            else
            {
                Debug.LogError("Target object is not assigned.");
            }

            // Check if TMP Input Field is assigned
            if (inputField == null)
            {
                Debug.LogError("TMP Input Field is not assigned.");
            }
            else
            {
                // Add listener to the TMP Input Field's onValueChanged event
                inputField.onValueChanged.AddListener(UpdateTargetName);
            }
        }

        // Method to update the targetName when the TMP Input Field's value changes
        private void UpdateTargetName(string newName)
        {
            if (navigationTarget != null)
            {
                // Update the targetName property of the IsNavigationTarget component
                navigationTarget.targetName = newName;
            }
        }
    }
}
