using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DroneManager : MonoBehaviour
{
    public TMP_InputField droneIdInput; // Reference to the input field
    public TextMeshProUGUI resultText;   // Reference to the result text
    public TextMeshProUGUI timeText;     // Reference to the simulated time text
    public Flock flock; 

    public GameObject targetBoxPrefab; // Prefab for the target box (assign in the inspector)
    private List<Drone> drones = new List<Drone>(); // List to hold drone objects
    private List<GameObject> activeTargetBoxes = new List<GameObject>(); // List to track active target boxes

    void Start()
    {
        if (flock != null)
        {
            drones = flock.agents; // Use the list of agents from the Flock script
        }
    }

    // Method to handle the "Search Drone" button click
    public void OnSearchDrone()
    {
        int droneId;
        if (int.TryParse(droneIdInput.text, out droneId))
        {
            float startTime = Time.realtimeSinceStartup; // Record the start time

            Drone drone = drones.Find(d => d.id == droneId); // Find the drone by ID
            if (drone != null)
            {
                ClearActiveTargetBoxes(); // Clear any previously active target boxes

                if (drone.isDestroyed)
                {
                    resultText.text = $"Drone {droneId} has already been destroyed.";
                }
                else
                {
                    resultText.text = $"Drone Position: {drone.transform.position}";
                    AddTargetBoxToDrone(drone); // Add target box to drone
                }
            }
            else
            {
                resultText.text = "Drone not found.";
            }

            float endTime = Time.realtimeSinceStartup; // Record the end time
            timeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms"; // Calculate the time difference in ms
        }
    }

    // Method to add a target box to the selected drone
    private void AddTargetBoxToDrone(Drone drone)
    {
        GameObject targetBox = Instantiate(targetBoxPrefab, drone.transform.position, Quaternion.identity);
        targetBox.transform.SetParent(drone.transform); // Set the box as a child of the drone
        targetBox.transform.localScale = new Vector3(1f, 1f, 1f); // Adjust scale (you can modify this)
        
        activeTargetBoxes.Add(targetBox); // Keep track of the target box

        // Start the coroutine to destroy the target box after 10 seconds
        StartCoroutine(DestroyTargetBoxAfterDelay(targetBox, 3f));
    }

    // Coroutine to destroy target box after a delay
    private IEnumerator DestroyTargetBoxAfterDelay(GameObject targetBox, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        Destroy(targetBox); // Destroy the target box
        activeTargetBoxes.Remove(targetBox); // Remove from active list
    }

    // Method to clear all active target boxes (to avoid cluttering the scene)
    private void ClearActiveTargetBoxes()
    {
        foreach (GameObject targetBox in activeTargetBoxes)
        {
            Destroy(targetBox); // Destroy the box
        }
        activeTargetBoxes.Clear(); // Clear the list of active boxes
    }

    public void OnSelfDestructDrone()
    {
        int droneId;
        if (int.TryParse(droneIdInput.text, out droneId))
        {
            float startTime = Time.realtimeSinceStartup; // Record the start time

            Drone drone = drones.Find(d => d.id == droneId); // Find the drone by ID
            if (drone != null)
            {
                ClearActiveTargetBoxes(); // Remove any target boxes before self-destructing

                if (drone.isDestroyed)
                {
                    resultText.text = $"Drone {droneId} has already been destroyed.";
                }
                else
                {
                    drone.SelfDestruct(); // Call the self-destruct method
                    resultText.text = $"Drone {droneId} destroyed.";
                }
            }
            else
            {
                resultText.text = "Drone not found.";
            }

            float endTime = Time.realtimeSinceStartup; // Record the end time
            timeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms"; // Calculate the time difference in ms
        }
    }
}
