using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // For coroutines

public class DroneSimulationUI : MonoBehaviour
{
    public Flock flock; // Reference to the Flock system
    public Transform basePosition; // Base position for drones to return to
    public GameObject targetBoxPrefab; // Prefab for the target box

    // UI Elements
    public InputField droneIdInput;
    public InputField batteryLevelInput;
    public Text resultText;
    public Text simulatedTimeText;
    public Button searchByIdButton;
    public Button searchByBatteryButton;
    public Button selfDestructButton;
    public Button returnToBaseButton;
    public InputField distanceId1Input;
    public InputField distanceId2Input;
    public Button calculateDistanceButton;

    private List<Drone> drones; // List of drones fetched from the Flock system
    private List<GameObject> activeTargetBoxes = new List<GameObject>(); // List to store active target boxes

    private void Start()
    {
        // Fetch the list of drones from the Flock system
        drones = flock.agents;

        // Attach listeners to buttons
        searchByIdButton.onClick.AddListener(SearchById);
        searchByBatteryButton.onClick.AddListener(SearchByBatteryLevel);
        selfDestructButton.onClick.AddListener(SelfDestruct);
        returnToBaseButton.onClick.AddListener(ReturnToBase);
        calculateDistanceButton.onClick.AddListener(CalculateDistance);
    }

    private void AddTargetBoxToDrone(Drone drone)
    {
        // Instantiate a target box at the drone's position
        GameObject targetBox = Instantiate(targetBoxPrefab, drone.transform.position, Quaternion.identity);
        
        // Scale the target box to a smaller size (adjust the scale factor as needed)
        targetBox.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);  // Adjust this scale to suit your needs
        
        // Attach the target box to the drone
        targetBox.transform.SetParent(drone.transform);

        // Keep track of active boxes
        activeTargetBoxes.Add(targetBox);

        // Start a coroutine to destroy the target box after 10 seconds
        StartCoroutine(DestroyTargetBoxAfterDelay(targetBox, 10f));
    }

    private void RemoveAllTargetBoxes()
    {
        // Destroy all active target boxes
        foreach (var targetBox in activeTargetBoxes)
        {
            Destroy(targetBox);
        }
        activeTargetBoxes.Clear();
    }

    // Coroutine to destroy target box after a delay
    private IEnumerator DestroyTargetBoxAfterDelay(GameObject targetBox, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        Destroy(targetBox); // Destroy the target box
        activeTargetBoxes.Remove(targetBox); // Remove from the active list
    }

    public void SearchById()
    {
        int id;
        if (!int.TryParse(droneIdInput.text, out id))
        {
            resultText.text = "Invalid ID!";
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        Drone drone = drones.Find(d => d.id == id); // Search for the drone in the flock's agents list
        float endTime = Time.realtimeSinceStartup;

        // Remove any previous target boxes before showing new ones
        RemoveAllTargetBoxes();

        if (drone != null)
        {
            if (drone.isDestroyed)
            {
                resultText.text = $"Drone ID {id} has already been destroyed.";
            }
            else
            {
                resultText.text = $"Drone ID {id} found at position \n{drone.transform.position}";
                AddTargetBoxToDrone(drone); // Add a target box on the found drone
            }
        }
        else
        {
            resultText.text = $"Drone ID {id} not found!";
        }

        simulatedTimeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms";
    }

    public void SearchByBatteryLevel()
    {
        int level;
        if (!int.TryParse(batteryLevelInput.text, out level))
        {
            resultText.text = "Invalid Battery Level!";
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        Drone closestDrone = null;
        float closestDistance = float.MaxValue;

        // Remove any previous target boxes before showing new ones
        RemoveAllTargetBoxes();

        foreach (Drone drone in drones) // Exhaustive search through the flock's agents list
        {
            if (drone.isDestroyed) continue; // Skip destroyed drones

            if (drone.Temperature == level) // Assuming Temperature simulates battery level
            {
                float distance = Vector3.Distance(drone.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDrone = drone;
                }
            }
        }

        float endTime = Time.realtimeSinceStartup;

        if (closestDrone != null)
        {
            resultText.text = $"Drone with battery level {level} is at position \n{closestDrone.transform.position}";
            AddTargetBoxToDrone(closestDrone); // Add a target box on the closest drone
        }
        else
        {
            resultText.text = $"No drone found with battery level {level}!";
        }

        simulatedTimeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms";
    }

    public void SelfDestruct()
    {
        int id;
        if (!int.TryParse(droneIdInput.text, out id))
        {
            resultText.text = "Invalid ID!";
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        Drone drone = drones.Find(d => d.id == id); // Search for the drone in the flock's agents list
        float endTime = Time.realtimeSinceStartup;

        // Remove any previous target boxes before showing new ones
        RemoveAllTargetBoxes();

        if (drone != null)
        {
            if (drone.isDestroyed)
            {
                resultText.text = $"Drone ID {id} has already been destroyed.";
            }
            else
            {
                drone.gameObject.SetActive(false); // Deactivate the drone
                drone.isDestroyed = true; // Mark the drone as destroyed
                resultText.text = $"Drone ID {id} self-destructed.";
            }
        }
        else
        {
            resultText.text = $"Drone ID {id} not found!";
        }

        simulatedTimeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms";
    }

    public void ReturnToBase()
    {
        float startTime = Time.realtimeSinceStartup;

        // Remove any previous target boxes before returning to base
        RemoveAllTargetBoxes();

        foreach (Drone drone in drones)
        {
            if (!drone.isDestroyed) // Only move drones that have not been destroyed
            {
                drone.transform.position = basePosition.position; // Move each drone to base
            }
        }

        float endTime = Time.realtimeSinceStartup;

        resultText.text = "All active drones returned to base.";
        simulatedTimeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms";
    }

    public void CalculateDistance()
    {
        int id1, id2;
        if (!int.TryParse(distanceId1Input.text, out id1) || !int.TryParse(distanceId2Input.text, out id2))
        {
            resultText.text = "Invalid Drone IDs!";
            return;
        }

        float startTime = Time.realtimeSinceStartup;
        Drone drone1 = drones.Find(d => d.id == id1);
        Drone drone2 = drones.Find(d => d.id == id2);
        float endTime = Time.realtimeSinceStartup;

        // Remove any previous target boxes before showing new ones
        RemoveAllTargetBoxes();

        if (drone1 != null && drone2 != null)
        {
            if (drone1.isDestroyed || drone2.isDestroyed)
            {
                resultText.text = "One or both drones have been destroyed.";
            }
            else
            {
                float distance = Vector3.Distance(drone1.transform.position, drone2.transform.position);
                resultText.text = $"Distance between Drone {id1} and Drone {id2}: {distance} units";
                AddTargetBoxToDrone(drone1); // Add target box to drone1
                AddTargetBoxToDrone(drone2); // Add target box to drone2
            }
        }
        else
        {
            resultText.text = "One or both drones not found!";
        }

        simulatedTimeText.text = $"Simulated Time: {(endTime - startTime) * 1000f} ms";
    }
}
