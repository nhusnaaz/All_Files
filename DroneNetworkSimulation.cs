using System.Collections.Generic;
using UnityEngine;

public class DroneNetworkCommunication
{
    private Dictionary<int, List<int>> network; // Graph adjacency list

    public DroneNetworkCommunication()
    {
        network = new Dictionary<int, List<int>>();
    }

    // Adds a drone to the network
    public void AddDrone(int droneId)
    {
        if (!network.ContainsKey(droneId))
        {
            network[droneId] = new List<int>();
        }
    }

    // Connects two drones in the network
    public void ConnectDrones(int drone1Id, int drone2Id)
    {
        if (!network.ContainsKey(drone1Id) || !network.ContainsKey(drone2Id))
        {
            Debug.LogError("One or both drones are not present in the network.");
            return;
        }

        // Add bidirectional links
        if (!network[drone1Id].Contains(drone2Id))
        {
            network[drone1Id].Add(drone2Id);
        }
        if (!network[drone2Id].Contains(drone1Id))
        {
            network[drone2Id].Add(drone1Id);
        }
    }

    // Routes a message between two drones using BFS
    public List<int> RouteMessage(int sourceId, int targetId)
    {
        if (!network.ContainsKey(sourceId) || !network.ContainsKey(targetId))
        {
            
        }

        Queue<int> queue = new Queue<int>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>(); // To reconstruct the path
        queue.Enqueue(sourceId);
        cameFrom[sourceId] = -1;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == targetId)
            {
                return ReconstructPath(cameFrom, targetId);
            }

            foreach (int neighbor in network[current])
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        Debug.LogError($"No route found between {sourceId} and {targetId}.");
        return null;
    }

    // Reconstructs the path from source to target
    private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int targetId)
    {
        List<int> path = new List<int>();
        int current = targetId;

        while (current != -1)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse(); // Reverse the path to get the correct order
        return path;
    }

    // Debugging helper to print the entire network
    public void PrintNetwork()
    {
        foreach (var drone in network)
        {
            Debug.Log($"Drone {drone.Key}: Connected to -> {string.Join(", ", drone.Value)}");
        }
    }
}
