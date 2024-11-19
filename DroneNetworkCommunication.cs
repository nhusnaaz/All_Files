using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DroneUIManager : MonoBehaviour
{
    public DroneNetworkCommunication droneNetwork; // Reference to DroneNetworkCommunication
    public LineRenderer lineRenderer; // LineRenderer for path visualization
    public Material lineMaterial; // Material for the line
    public GameObject highlightBoxPrefab; // Prefab for the highlight box

    // UI Elements
    public InputField droneIdInput;
    public InputField sourceDroneInput;
    public InputField targetDroneInput;
    public InputField newDriveFactorInput; // New input field for drive factor
    public Text resultText;

    // Buttons
    public Button searchButton;
    public Button pathButton;
    public Button displayDriveFactorButton; // Button to display drive factor
    public Button changeDriveFactorButton; // Button to change drive factor
    public Button distanceButton;

    private Drone sourceDrone; // Source drone for the path
    private Drone targetDrone; // Target drone for the path
    private bool isPathActive = false; // Flag to track if a path is active
    private List<GameObject> highlightedBoxes = new List<GameObject>();

    void Start()
    {
        // Initialize drone network if not assigned
        if (droneNetwork == null)
        {
            droneNetwork = new DroneNetworkCommunication();
            InitializeNetwork(); // Populate the network
        }

        // Link buttons to methods
        searchButton.onClick.AddListener(SearchDrone);
        pathButton.onClick.AddListener(FindShortestPath);
        displayDriveFactorButton.onClick.AddListener(DisplayDroneDriveFactor); // Updated to display drive factor
        changeDriveFactorButton.onClick.AddListener(ChangeDroneDriveFactor); // Updated to change drive factor
        distanceButton.onClick.AddListener(CalculateDistance);

        // Ensure LineRenderer is ready
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not assigned.");
            GameObject lineObject = new GameObject("PathVisualizer");
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Default material
        }
    }

    void Update()
    {
        // Update the line's positions if a path is active
        if (isPathActive && sourceDrone != null && targetDrone != null)
        {
            lineRenderer.SetPosition(0, sourceDrone.transform.position);
            lineRenderer.SetPosition(1, targetDrone.transform.position);
        }
    }

    // Initialize the drone network with some sample data
    private void InitializeNetwork()
    {
        // Example drones
        droneNetwork.AddDrone(1);
        droneNetwork.AddDrone(2);
        droneNetwork.AddDrone(3);
        droneNetwork.AddDrone(4);

        // Example connections
        droneNetwork.ConnectDrones(1, 2);
        droneNetwork.ConnectDrones(2, 3);
        droneNetwork.ConnectDrones(3, 4);

        Debug.Log("Drone network initialized with sample data.");
    }

    // Search for a drone by ID and display its position
    public void SearchDrone()
    {
        ClearPathVisualization(); // Clear any previous visualizations

        int droneId;
        if (int.TryParse(droneIdInput.text, out droneId))
        {
            ClearPathVisualization();
            Drone drone = GetDroneById(droneId);

            if (drone != null)
            {
                HighlightDroneWithBox(drone); // Highlight the searched drone
                resultText.text = $"Drone {droneId} found. Position: {drone.transform.position}";
                StartCoroutine(ClearVisualizationAfterDelay(10f)); // Clear after 3 seconds
            }
            else
            {
                resultText.text = $"Drone {droneId} not found.";
            }
        }
        else
        {
            resultText.text = "Invalid drone ID.";
        }
    }

    // Find the shortest path between two drones and highlight it
    public void FindShortestPath()
    {
        ClearPathVisualization(); // Clear previous path and highlights

        int sourceId, targetId;
        if (int.TryParse(sourceDroneInput.text, out sourceId) && int.TryParse(targetDroneInput.text, out targetId))
        {
            // Ensure both drones exist in the network
            sourceDrone = GetDroneById(sourceId);
            targetDrone = GetDroneById(targetId);

            if (sourceDrone != null && targetDrone != null)
            {
                ClearPathVisualization();
                // Highlight both drones with boxes
                HighlightDroneWithBox(sourceDrone);
                HighlightDroneWithBox(targetDrone);

                // Draw a direct line between the two drones
                DrawPathBetweenDrones();

                resultText.text = $"Path from Drone {sourceId} to Drone {targetId} drawn.";
                isPathActive = true; // Activate the path

                // Start the coroutine to clear the path and boxes after 3 seconds
                StartCoroutine(ClearVisualizationAfterDelay(10f));
            }
            else
            {
                resultText.text = "One or both drones not found.";
                ClearPathVisualization();
            }
        }
        else
        {
            resultText.text = "Invalid source or target drone ID.";
            ClearPathVisualization();
        }
    }

    // Display a drone's current drive factor
    public void DisplayDroneDriveFactor()
    {
        int droneId;
        if (int.TryParse(droneIdInput.text, out droneId))
        {
            ClearPathVisualization();
            Drone drone = GetDroneById(droneId);

            if (drone != null)
            {
                HighlightDroneWithBox(drone);
                resultText.text = $"Drone {droneId}'s speed: {drone.driveFactor}"; // Display drive factor
                StartCoroutine(ClearVisualizationAfterDelay(10f)); // Clear after 3 seconds
            }
            else
            {
                resultText.text = $"Drone {droneId} not found.";
            }
        }
        else
        {
            resultText.text = "Invalid drone ID.";
        }
    }

    // Change a drone's drive factor
    public void ChangeDroneDriveFactor()
    {
        int droneId;
        float newDriveFactor;

        if (int.TryParse(droneIdInput.text, out droneId) && 
            float.TryParse(newDriveFactorInput.text, out newDriveFactor))
        {
            ClearPathVisualization();
            Drone drone = GetDroneById(droneId);

            if (drone != null)
            {
                HighlightDroneWithBox(drone);
                drone.driveFactor = newDriveFactor; // Update the drive factor
                Debug.Log($"Drone {droneId}'s speed is changed to {newDriveFactor}."); // Debug log
                resultText.text = $"Drone {droneId}'s speed is changed to {newDriveFactor}.";
                StartCoroutine(ClearVisualizationAfterDelay(10f)); // Clear after 3 seconds
            }
            else
            {
                resultText.text = $"Drone {droneId} not found.";
            }
        }
        else
        {
            resultText.text = "Invalid drone ID or drive factor.";
        }
    }

    // Calculate the distance between two drones
    public void CalculateDistance()
    {
        int droneId1, droneId2;
        if (int.TryParse(sourceDroneInput.text, out droneId1) && int.TryParse(targetDroneInput.text, out droneId2))
        {
            ClearPathVisualization();
            Drone drone1 = GetDroneById(droneId1);
            Drone drone2 = GetDroneById(droneId2);

            if (drone1 != null && drone2 != null)
            {
                HighlightDroneWithBox(drone1);
                HighlightDroneWithBox(drone2);
                float distance = Vector3.Distance(drone1.transform.position, drone2.transform.position);
                resultText.text = $"Distance between Drone {droneId1} and Drone {droneId2}: {distance} units.";
                StartCoroutine(ClearVisualizationAfterDelay(10f)); // Clear after 3 seconds
            }
            else
            {
                resultText.text = "One or both drones not found.";
            }
        }
        else
        {
            resultText.text = "Invalid drone IDs.";
        }
    }

    // Highlight a drone with a box
    private void HighlightDroneWithBox(Drone drone)
    {
        GameObject highlightBox = Instantiate(highlightBoxPrefab, drone.transform.position, Quaternion.identity, drone.transform);
        highlightedBoxes.Add(highlightBox);
    }

    // Draw a path between two drones
    private void DrawPathBetweenDrones()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not assigned.");
            return;
        }

        // Set the positions of the LineRenderer
        lineRenderer.positionCount = 2; // Only two points: source and target
        lineRenderer.SetPosition(0, sourceDrone.transform.position); // Source drone position
        lineRenderer.SetPosition(1, targetDrone.transform.position); // Target drone position
    }

    // Coroutine to clear path and highlighted boxes after a delay
    private IEnumerator ClearVisualizationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
                // Clear path visualization and highlighted boxes
        ClearPathVisualization();
    }

    // Clear path visualization
    private void ClearPathVisualization()
    {
        isPathActive = false; // Deactivate the path
        sourceDrone = null;
        targetDrone = null;

        // Remove all highlight boxes
        foreach (GameObject box in highlightedBoxes)
        {
            Destroy(box);
        }
        highlightedBoxes.Clear();

        // Clear the line
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    // Helper method to find a drone by ID
    private Drone GetDroneById(int id)
    {
        foreach (Drone drone in FindObjectsOfType<Drone>())
        {
            if (drone.id == id)
            {
                return drone;
            }
        }
        return null;
    }
}
