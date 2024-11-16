using UnityEngine;
using System.Collections.Generic;

namespace MCTS
{
    public class MCTSCar : MonoBehaviour
    {
        public Transform goal; // Static goal position
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f;
        public int maxIterations = 1000; // Max MCTS iterations
        public int maxSimulationDepth = 10; // Max simulation depth

        private Grid grid;
        private List<Node> path;
        private int currentPathIndex;

        void Start()
        {
            grid = FindObjectOfType<Grid>();
            if (grid == null)
            {
                Debug.LogError("Grid not found in the scene!");
                return;
            }

            if (goal == null)
            {
                Debug.LogError("Goal transform is not assigned!");
                return;
            }

            // Perform MCTS to find a path
            path = PerformMCTS(transform.position, goal.position);

            if (path != null && path.Count > 0)
            {
                Debug.Log("Path successfully calculated.");
                currentPathIndex = 0;
            }
            else
            {
                Debug.LogError("No valid path found!");
            }
        }

        void Update()
        {
            if (path == null || currentPathIndex >= path.Count)
            {
                Debug.Log("No valid path to follow or reached the goal.");
                return;
            }

            MoveAlongPath();
        }

        private List<Node> PerformMCTS(Vector3 startPosition, Vector3 goalPosition)
        {
            Node startNode = grid.NodeFromWorldPoint(startPosition);
            Node goalNode = grid.NodeFromWorldPoint(goalPosition);

            if (!startNode.walkable || !goalNode.walkable)
            {
                Debug.LogWarning("Start or Goal node is unwalkable!");
                return null;
            }

            // Initialize the root node
            MCTSNode root = new MCTSNode(startNode, null);

            for (int i = 0; i < maxIterations; i++)
            {
                // Perform MCTS steps
                MCTSNode selectedNode = Select(root);
                if (selectedNode == null) continue;

                MCTSNode expandedNode = Expand(selectedNode, goalNode);
                if (expandedNode == null) continue;

                float reward = Simulate(expandedNode, goalNode);
                Backpropagate(expandedNode, reward);

                // Check if the goal is reached
                if (expandedNode.Node == goalNode)
                {
                    Debug.Log("Goal reached during MCTS!");
                    return ReconstructPath(expandedNode);
                }
            }

            // Return the best path found
            MCTSNode bestNode = GetBestChild(root);
            return bestNode != null ? ReconstructPath(bestNode) : null;
        }

        private MCTSNode Select(MCTSNode node)
        {
            // Traverse the tree to find the most promising node
            while (node.Children.Count > 0)
            {
                node = node.GetBestChild();
            }
            return node;
        }

        private MCTSNode Expand(MCTSNode node, Node goalNode)
        {
            // Get neighbors of the current node
            List<Node> neighbors = grid.GetNeighbours(node.Node);

            foreach (Node neighbor in neighbors)
            {
                if (neighbor.walkable && !node.ContainsChild(neighbor))
                {
                    MCTSNode childNode = new MCTSNode(neighbor, node);
                    node.AddChild(childNode);
                    return childNode;
                }
            }

            return null; // No valid expansions possible
        }

        private float Simulate(MCTSNode node, Node goalNode)
        {
            Node currentNode = node.Node;

            for (int depth = 0; depth < maxSimulationDepth; depth++)
            {
                if (currentNode == goalNode)
                    return 1f; // Reward for reaching the goal

                List<Node> neighbors = grid.GetNeighbours(currentNode).FindAll(n => n.walkable);
                if (neighbors.Count == 0)
                    return 0f; // Simulation fails if no valid neighbors

                // Choose a random neighbor for simulation
                currentNode = neighbors[Random.Range(0, neighbors.Count)];
            }

            // Penalize for failing to reach the goal
            return -Vector3.Distance(currentNode.worldPosition, goalNode.worldPosition);
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

        private MCTSNode GetBestChild(MCTSNode node)
        {
            float bestValue = float.MinValue;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.Children)
            {
                float value = child.TotalReward / Mathf.Max(1, child.VisitCount);
                if (value > bestValue)
                {
                    bestValue = value;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        private List<Node> ReconstructPath(MCTSNode node)
        {
            List<Node> path = new List<Node>();
            while (node != null)
            {
                path.Add(node.Node);
                node = node.Parent;
            }

            path.Reverse(); // Reverse to get the path from start to goal
            return path;
        }

        private void MoveAlongPath()
        {
            Node targetNode = path[currentPathIndex];
            Vector3 targetPosition = targetNode.worldPosition;
            targetPosition.y = transform.position.y; // Maintain the car's height

            Vector3 direction = (targetPosition - transform.position).normalized;

            // Move towards the target node
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Smoothly rotate towards the target node
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            // Check if the car has reached the target node
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPathIndex++;
            }
        }
    }

    public class MCTSNode
    {
        public Node Node;
        public MCTSNode Parent;
        public List<MCTSNode> Children = new List<MCTSNode>();
        public int VisitCount = 0;
        public float TotalReward = 0;

        public MCTSNode(Node node, MCTSNode parent)
        {
            Node = node;
            Parent = parent;
        }

        public void AddChild(MCTSNode child)
        {
            Children.Add(child);
        }

        public bool ContainsChild(Node node)
        {
            return Children.Exists(child => child.Node == node);
        }

        public MCTSNode GetBestChild()
        {
            float bestValue = float.MinValue;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in Children)
            {
                float value = child.TotalReward / Mathf.Max(1, child.VisitCount);
                if (value > bestValue)
                {
                    bestValue = value;
                    bestChild = child;
                }
            }

            return bestChild;
        }
    }
}
