using UnityEngine;
using System.Collections.Generic;

public class MCTSCar : MonoBehaviour
{
    public Transform goal; // Target goal transform
    public Grid grid; // Reference to the Grid script
    public float speed = 5f; // Movement speed
    public int maxIterations = 1000; // Number of MCTS iterations
    public int maxDepth = 20; // Max simulation depth

    private List<Node> path; // Computed path
    private int currentPathIndex = 0; // Current index in the path

    void Start()
    {
        if (goal == null || grid == null)
        {
            Debug.LogError("Goal or Grid not assigned to MCTSCar!");
            return;
        }

        path = FindPath(transform.position, goal.position);

        if (path == null || path.Count == 0)
        {
            Debug.LogError("No valid path found for MCTSCar!");
        }
    }

    void Update()
    {
        if (path != null && currentPathIndex < path.Count)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        Node currentNode = path[currentPathIndex];
        Vector3 targetPosition = currentNode.worldPosition;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    private List<Node> FindPath(Vector3 startPosition, Vector3 goalPosition)
    {
        Node startNode = grid.NodeFromWorldPoint(startPosition);
        Node goalNode = grid.NodeFromWorldPoint(goalPosition);

        if (!startNode.walkable || !goalNode.walkable)
        {
            Debug.LogWarning("Start or Goal node is unwalkable!");
            return null;
        }

        Node bestNode = PerformMCTS(startNode, goalNode);
        return ReconstructPath(bestNode);
    }

    private Node PerformMCTS(Node startNode, Node goalNode)
    {
        MCTSNode root = new MCTSNode(startNode);

        for (int i = 0; i < maxIterations; i++)
        {
            MCTSNode selectedNode = Select(root);
            Node expandedNode = Expand(selectedNode, goalNode);
            float reward = Simulate(expandedNode, goalNode);
            Backpropagate(selectedNode, reward);
        }

        return root.GetBestChild()?.Node;
    }

    private MCTSNode Select(MCTSNode node)
    {
        while (node.Children.Count > 0 && node.Depth < maxDepth)
        {
            node = node.GetBestChild();
        }
        return node;
    }

    private Node Expand(MCTSNode node, Node goalNode)
    {
        List<Node> neighbors = grid.GetNeighbours(node.Node);
        foreach (Node neighbor in neighbors)
        {
            if (neighbor.walkable && !node.ContainsChild(neighbor))
            {
                MCTSNode childNode = new MCTSNode(neighbor, node);
                node.AddChild(childNode);
                return neighbor;
            }
        }
        return node.Node; // No expansion possible
    }

    private float Simulate(Node node, Node goalNode)
    {
        float score = 0;
        Node current = node;

        for (int depth = 0; depth < maxDepth && current != goalNode; depth++)
        {
            List<Node> neighbors = grid.GetNeighbours(current);
            neighbors.RemoveAll(n => !n.walkable);

            if (neighbors.Count == 0)
                break;

            current = neighbors[Random.Range(0, neighbors.Count)];
            score -= Vector3.Distance(current.worldPosition, goalNode.worldPosition);
        }

        if (current == goalNode)
        {
            score += 100; // Bonus for reaching the goal
        }
        return score;
    }

    private void Backpropagate(MCTSNode node, float reward)
    {
        while (node != null)
        {
            node.VisitCount++;
            node.TotalReward += reward;
            node = node.Parent;
        }
    }

    private List<Node> ReconstructPath(Node node)
    {
        List<Node> path = new List<Node>();
        Node current = node;

        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private class MCTSNode
    {
        public Node Node { get; private set; }
        public MCTSNode Parent { get; private set; }
        public List<MCTSNode> Children { get; private set; }
        public int VisitCount { get; set; }
        public float TotalReward { get; set; }
        public int Depth => Parent == null ? 0 : Parent.Depth + 1;

        public MCTSNode(Node node, MCTSNode parent = null)
        {
            Node = node;
            Parent = parent;
            Children = new List<MCTSNode>();
        }

        public void AddChild(MCTSNode child)
        {
            Children.Add(child);
        }

        public bool ContainsChild(Node node)
        {
            return Children.Exists(c => c.Node == node);
        }

        public MCTSNode GetBestChild()
        {
            float bestValue = float.MinValue;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in Children)
            {
                float ucbValue = child.TotalReward / child.VisitCount + Mathf.Sqrt(2 * Mathf.Log(VisitCount) / child.VisitCount);
                if (ucbValue > bestValue)
                {
                    bestValue = ucbValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }
    }
}
