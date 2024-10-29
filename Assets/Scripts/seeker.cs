using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    private enum State { Idle, Moving, Turning, Avoiding }
    private State _currentState;

    public Transform target;
    public float speed = 12f;
    public LayerMask unwalkableLayer;
    public LayerMask otherPathLayer;
    public float nodeRadius = 0.5f;
    public float turningSpeed = 5f;

    private Node _currentNode;
    private Node _targetNode;
    private Grid _grid;

    void Start()
    {
        _currentState = State.Moving;
        _grid = FindObjectOfType<Grid>();
        _currentNode = _grid.NodeFromWorldPoint(transform.position);
        _targetNode = _grid.NodeFromWorldPoint(target.position);
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (true)
        {
            switch (_currentState)
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
        if (Vector3.Distance(transform.position, _currentNode.worldPosition) < 0.1f)
        {
            if (_currentNode == _targetNode)
            {
                _currentState = State.Idle;
                return;
            }

            List<Node> neighbours = GetNeighbours(_currentNode);
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
                _currentNode = bestNode;
            }
            else
            {
                _currentState = State.Turning;
                return;
            }
        }
        
        Vector3 direction = (_currentNode.worldPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);

        transform.position = Vector3.MoveTowards(transform.position, _currentNode.worldPosition, speed * Time.deltaTime);
        
        if (Physics.CheckSphere(transform.position, nodeRadius, otherPathLayer))
        {
            _currentState = State.Avoiding;
        }
    }

    void Turn()
    {
        List<Node> neighbours = GetNeighbours(_currentNode);
        foreach (Node neighbour in neighbours)
        {
            if (neighbour.walkable)
            {
                _currentNode = neighbour;
                _currentState = State.Moving;
                return;
            }
        }

        transform.Rotate(Vector3.up, Random.Range(45, 135));
    }

    void Avoid()
    {
        transform.Rotate(Vector3.up, 180);
        _currentState = State.Moving;
    }

    List<Node> GetNeighbours(Node node)
    {
        return _grid.GetNeighbours(node);
    }
}
