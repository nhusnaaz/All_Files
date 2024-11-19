using System;
using System.Collections.Generic;
using UnityEngine;

public class DroneBTCommunication
{
    public class Node
    {
        public Drone Drone { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(Drone drone)
        {
            Drone = drone;
            Left = null;
            Right = null;
        }
    }

    private Node root;

    public DroneBTCommunication()
    {
        root = null;
    }

    // Insert a drone into the tree using the specified key attribute (e.g., id, Temperature)
    public void Insert(Drone drone, Func<Drone, int> keySelector)
    {
        root = InsertNode(root, drone, keySelector);
    }

    private Node InsertNode(Node current, Drone drone, Func<Drone, int> keySelector)
    {
        if (current == null)
        {
            return new Node(drone);
        }

        int key = keySelector(drone);
        int currentKey = keySelector(current.Drone);

        if (key < currentKey)
        {
            current.Left = InsertNode(current.Left, drone, keySelector);
        }
        else
        {
            current.Right = InsertNode(current.Right, drone, keySelector);
        }

        return current;
    }

    // Search for a drone in the tree using the specified key
    public Drone Search(int key, Func<Drone, int> keySelector)
    {
        return SearchNode(root, key, keySelector);
    }

    private Drone SearchNode(Node current, int key, Func<Drone, int> keySelector)
    {
        if (current == null)
        {
            return null; // Drone not found
        }

        int currentKey = keySelector(current.Drone);

        if (key == currentKey)
        {
            return current.Drone;
        }
        else if (key < currentKey)
        {
            return SearchNode(current.Left, key, keySelector);
        }
        else
        {
            return SearchNode(current.Right, key, keySelector);
        }
    }

    // Route a message through the tree
    public List<Drone> RouteMessage(int targetKey, Func<Drone, int> keySelector)
    {
        List<Drone> route = new List<Drone>();
        RouteMessageRecursive(root, targetKey, keySelector, route);
        return route;
    }

    private bool RouteMessageRecursive(Node current, int targetKey, Func<Drone, int> keySelector, List<Drone> route)
    {
        if (current == null)
        {
            return false; // Target not found
        }

        route.Add(current.Drone);

        int currentKey = keySelector(current.Drone);

        if (targetKey == currentKey)
        {
            return true; // Target found
        }
        else if (targetKey < currentKey)
        {
            return RouteMessageRecursive(current.Left, targetKey, keySelector, route);
        }
        else
        {
            return RouteMessageRecursive(current.Right, targetKey, keySelector, route);
        }
    }
}