using System.Collections.Generic;
using UnityEngine;

// Makes the script run in both Play and Edit Mode
[ExecuteInEditMode]
public class RandomiseDragInChildren : MonoBehaviour
{
    // Public toggle in the Inspector
    public bool assignDragInEditor = false;

    void Update()
    {
        // Check if the bool is toggled in the Inspector
        if (assignDragInEditor)
        {
            AssignRandomDragToChildren();

            // Reset the bool to false to avoid repeatedly assigning random drag values
            assignDragInEditor = false;
        }
    }

    void AssignRandomDragToChildren()
    {
        // Get all the child GameObjects of the current GameObject
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        int childCount = children.Count; // Total number of child objects

        if (childCount == 0)
        {
            Debug.LogWarning("This GameObject has no children to assign drag values to.");
            return;
        }

        // Generate a list of unique random integer drag values
        int[] randomDragValues = GenerateUniqueRandomIntegers(childCount);

        // Assign the random drag values to each child's Rigidbody (if it has one)
        for (int i = 0; i < children.Count; i++)
        {
            Rigidbody childRb = children[i].GetComponent<Rigidbody>();
            if (childRb != null)
            {
                childRb.drag = randomDragValues[i];
                Debug.Log($"Assigned drag {randomDragValues[i]} to {children[i].name}");
            }
            else
            {
                Debug.LogWarning($"Child {children[i].name} does not have a Rigidbody. Skipping.");
            }
        }
    }

    int[] GenerateUniqueRandomIntegers(int count)
    {
        // Create a list of unique random integers in the range [0, count)
        List<int> uniqueNumbers = new List<int>();

        while (uniqueNumbers.Count < count)
        {
            int randomDrag = Random.Range(0, count); // Generates integers between 0 (inclusive) and count (exclusive)
            if (!uniqueNumbers.Contains(randomDrag))
            {
                uniqueNumbers.Add(randomDrag);
            }
        }

        return uniqueNumbers.ToArray();
    }
}