using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro components
#if UNITY_EDITOR
using UnityEditor; // Required for SetDirty and saving editor changes
#endif

[ExecuteInEditMode] // Allows the script to run in the Unity Editor
public class PeriodTableGuesserSetup : MonoBehaviour
{
    // Dictionary to store the periodic table symbols and their names
    private Dictionary<string, string> elementNames;

    // Public bool toggled in the Inspector to run the script
    [Header("Run the script by toggling this on")]
    public bool runScript = false; // Flag to run the logic

    // Initialize the dictionary in OnEnable (works in edit mode)
    private void OnEnable()
    {
        InitializeDictionary();
    }

    // Initialize the dictionary with element data
    private void InitializeDictionary()
    {
        elementNames = new Dictionary<string, string>
        {
            { "H", "Hydrogen" },
            { "C", "Carbon" },
            { "N", "Nitrogen" },
            { "O", "Oxygen" },
            { "S", "Sulfur" },
            { "Mg", "Magnesium" },
            { "Na", "Sodium" },
            { "Cl", "Chlorine" },
            { "Ca", "Calcium" },
            { "Cu", "Copper" },
            { "Fe", "Iron" },
            { "He", "Helium" }
        };
    }

    // Update is called every frame in the Unity Editor
    private void Update()
    {
        // Check if the user toggled the bool
        if (runScript)
        {
            // Ensure dictionary is initialized
            if (elementNames == null || elementNames.Count == 0)
            {
                Debug.LogWarning("Element dictionary is empty or not properly initialized.");
                return;
            }

            // Run the script logic
            AssignTextToChildrenRecursive(this.transform);

            // Reset the bool to prevent running repeatedly
            runScript = false;

            Debug.Log("Periodic table data assigned recursively to children.");
        }
    }

    // Recursive function to assign title and object text to children
    private void AssignTextToChildrenRecursive(Transform parent)
    {
        int index = 0;

        // Recursive traversal of all child GameObjects
        foreach (Transform child in parent)
        {
            if (index >= elementNames.Count) return; // Stop processing if all elements are handled

            // Get the key (symbol) and value (name) for the current element index
            string key = GetKeyAtIndex(index);
            string value = elementNames[key];

            // Look for "TitleText" component recursively and assign the *name*
            Transform titleTextChild = FindChildByNameRecursive(child, "TitleText");
            if (titleTextChild != null)
            {
                // Assign the value (element name) to the TMP_Text component
                TMP_Text tmpTextComponent = titleTextChild.GetComponent<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.text = value;

                    // Make changes dirty so they can be saved
                    #if UNITY_EDITOR
                    EditorUtility.SetDirty(tmpTextComponent);
                    #endif
                }
            }

            // Look for "ObjectText" component recursively and assign the *symbol*
            Transform objectTextChild = FindChildByNameRecursive(child, "ObjectText");
            if (objectTextChild != null)
            {
                // Assign the key (element symbol) to the TMP_Text component
                TMP_Text tmpTextComponent = objectTextChild.GetComponent<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.text = key;

                    // Make changes dirty so they can be saved
                    #if UNITY_EDITOR
                    EditorUtility.SetDirty(tmpTextComponent);
                    #endif
                }
            }

            index++; // Increment the index to move to the next dictionary entry

            // Process the current child's children recursively
            AssignTextToChildrenRecursive(child);
        }
    }

    // Recursive utility function: find a child object by name
    private Transform FindChildByNameRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // Recursive call to search in lower levels
            Transform found = FindChildByNameRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    // Utility to get the key from the dictionary by index
    private string GetKeyAtIndex(int index)
    {
        int i = 0;
        foreach (var key in elementNames.Keys)
        {
            if (i == index) return key;
            i++;
        }
        return null; // Invalid index handling, should not happen
    }
}