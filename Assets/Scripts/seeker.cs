using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    private enum State { Idle, Moving, Turning, Avoiding }
    private State currentState;

    public Transform target;
    public float speed = 20f;
    public LayerMask unwalkableLayer;
    public LayerMask otherPathLayer;
    public float nodeRadius = 0.5f;
    public float turningSpeed = 5f;

    private Node currentNode;
    private Node targetNode;
    private Grid grid;

    void Start()
    {
        currentState = State.Moving;
        grid = FindObjectOfType<Grid>();
        currentNode = grid.NodeFromWorldPoint(transform.position);
        targetNode = grid.NodeFromWorldPoint(target.position);
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Moving:
                    Move();
                    break;

                case State.Turning:
                    Turn();
                    break;

                case State.Avoiding:
                    Avoid();
                    break;
            }
            yield return null;
        }
    }

    void Move()
    {
        if (Vector3.Distance(transform.position, currentNode.worldPosition) < 0.1f)
        {
            if (currentNode == targetNode)
            {
                currentState = State.Idle;
                return;
            }

            List<Node> neighbours = GetNeighbours(currentNode);
            Node bestNode = null;
            float shortestDistance = Mathf.Infinity;

            foreach (Node neighbour in neighbours)
            {
                if (neighbour.walkable && !Physics.CheckSphere(neighbour.worldPosition, nodeRadius, otherPathLayer))
                {
                    float distance = Vector3.Distance(neighbour.worldPosition, target.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestNode = neighbour;
                    }
                }
            }

            if (bestNode != null)
            {
                currentNode = bestNode;
            }
            else
            {
                currentState = State.Turning;
                return;
            }
        }

        // Smoothly rotate towards the target node
        Vector3 direction = (currentNode.worldPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);

        transform.position = Vector3.MoveTowards(transform.position, currentNode.worldPosition, speed * Time.deltaTime);

        // Check for collision with A* path
        if (Physics.CheckSphere(transform.position, nodeRadius, otherPathLayer))
        {
            currentState = State.Avoiding;
        }
    }

    void Turn()
    {
        List<Node> neighbours = GetNeighbours(currentNode);
        foreach (Node neighbour in neighbours)
        {
            if (neighbour.walkable)
            {
                currentNode = neighbour;
                currentState = State.Moving;
                return;
            }
        }

        // Randomly turn if blocked
        transform.Rotate(Vector3.up, Random.Range(45, 135));
    }

    void Avoid()
    {
        transform.Rotate(Vector3.up, 180);
        currentState = State.Moving;
    }

    List<Node> GetNeighbours(Node node)
    {
        return grid.GetNeighbours(node);
    }
}
