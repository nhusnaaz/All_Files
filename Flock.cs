using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Drone agentPrefab;
    public List<Drone> agents = new List<Drone>();
    public FlockBehavior behavior;

    [Range(10, 500)]
    public int startingCount = 50;

    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 8f;

    [Range(1f, 100f)]
    public float maxSpeed = 1.5f;

    [Range(1f, 10f)]
    public float neighborRadius = 2f;

    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 1.5f;

    public float innerRadius = 5f;
    public float outerRadius = 10f;
    public float destructRadius = 15f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    // Binary Tree Communication Systems
    private DroneBTCommunication partition1BT;
    private DroneBTCommunication partition2BT;

    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        partition1BT = new DroneBTCommunication();
        partition2BT = new DroneBTCommunication();

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
            );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this, i); // Pass the Flock reference and a unique ID
            agents.Add(newAgent);
        }

        PartitionDrones(); // Initial partitioning
    }

    void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        UnityEngine.Debug.Log("FPS: " + fps);

        // Update each drone's color and check for hiding (self-destruction) based on distance
        UpdateDroneColors();

        foreach (Drone agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent);
            Vector2 move = behavior.CalculateMove(agent, context, this);
            move *= agent.driveFactor;

            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }

            agent.Move(move);
        }
    }

    void UpdateDroneColors()
    {
        foreach (Drone agent in agents)
        {
            float distance = Vector2.Distance(agent.transform.position, transform.position);

            if (distance <= innerRadius)
            {
                if (!agent.isDestroyed)
                {
                    agent.SetColor(Color.blue); // Within inner radius, set color to blue
                }
            }
            else if (distance <= outerRadius)
            {
                if (!agent.isDestroyed)
                {
                    agent.SetColor(Color.red); // Outside inner radius but within outer radius, set color to red
                }
            }
            else if (distance <= destructRadius)
            {
                if (!agent.isDestroyed)
                {
                    agent.SetFlashing(true); // Start flashing within destruct radius
                }
            }
            else
            {
                agent.Hide(); // Beyond destruct radius, self-destruct the drone
            }
        }
    }


    void PartitionDrones()
    {
        if (agents.Count == 0) return;

        agents.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
        int medianIndex = agents.Count / 2;

        for (int i = 0; i < agents.Count; i++)
        {
            float startTime = Time.time; // Start time

            if (i < medianIndex)
            {
                agents[i].GetComponent<SpriteRenderer>().color = Color.red;
                partition1BT.Insert(agents[i], drone => drone.id); // Insert into Partition 1's Binary Tree
            }
            else
            {
                agents[i].GetComponent<SpriteRenderer>().color = Color.blue;
                partition2BT.Insert(agents[i], drone => drone.id); // Insert into Partition 2's Binary Tree
            }

            // Measure elapsed time
            float elapsedMilliseconds = (Time.time - startTime) * 1000f;
            UnityEngine.Debug.Log($"{agents[i].name} Partitioning Time (ms): {elapsedMilliseconds}");
        }
    }


    public void RouteMessage(int targetId)
    {
        List<Drone> route = partition1BT.RouteMessage(targetId, drone => drone.id);

        if (route == null || route.Count == 0)
        {
            route = partition2BT.RouteMessage(targetId, drone => drone.id);
        }

        if (route != null && route.Count > 0)
        {
            Debug.Log($"Message routed through {route.Count} drones to reach target {targetId}.");
            foreach (Drone drone in route)
            {
                Debug.Log($"Drone ID: {drone.id} handled the message.");
            }
        }
        else
        {
            Debug.Log($"Target {targetId} not found.");
        }
    }

    List<Transform> GetNearbyObjects(Drone agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius);

        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }

        return context;
    }
}