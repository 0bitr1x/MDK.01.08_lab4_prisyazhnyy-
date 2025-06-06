using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphLab
{
    public class Node<T>
    {
        public T Data { get; set; }
        public int Index { get; set; }
        public List<Node<T>> Neightbors { get; set; } = new();
        public List<int> Weights { get; set; } = new();
    }

    public class Edge<T>
    {
        public Node<T> From { get; set; }
        public Node<T> To { get; set; }
        public int Weight { get; set; }
    }

    public class Graph<T>
    {
        private bool _isDirected = false;
        private bool _isWeighted = false;
        public List<Node<T>> Nodes { get; set; } = new();

        public Graph(bool isDirected, bool isWeighted)
        {
            _isDirected = isDirected;
            _isWeighted = isWeighted;
        }

        public Edge<T> this[int from, int to]
        {
            get
            {
                Node<T> nodeFrom = Nodes[from];
                Node<T> nodeTo = Nodes[to];
                int i = nodeFrom.Neightbors.IndexOf(nodeTo);
                if (i > 0)
                {
                    Edge<T> edge = new Edge<T>()
                    {
                        From = nodeFrom,
                        To = nodeTo,
                        Weight = i < nodeFrom.Weights.Count ? nodeFrom.Weights[i] : 0
                    };
                    return edge;
                }
                return null;
            }
        }

        public Node<T> AddNode(T value)
        {
            Node<T> node = new Node<T> { Data = value };
            Nodes.Add(node);
            UpdateIndeces();
            return node;
        }

        public void RemoveNode(Node<T> nodeToRemove)
        {
            Nodes.Remove(nodeToRemove);
            UpdateIndeces();
            foreach (Node<T> node in Nodes)
            {
                RemoveEdge(node, nodeToRemove);
            }
        }

        public void AddEdge(Node<T> from, Node<T> to, int weight = 0)
        {
            from.Neightbors.Add(to);
            if (_isWeighted) from.Weights.Add(weight);
            if (!_isDirected)
            {
                to.Neightbors.Add(from);
                if (_isWeighted) to.Weights.Add(weight);
            }
        }

        public void RemoveEdge(Node<T> from, Node<T> to)
        {
            int index = from.Neightbors.FindIndex(n => n == to);
            if (index >= 0)
            {
                from.Neightbors.RemoveAt(index);
                if (_isWeighted) from.Weights.RemoveAt(index);
            }
        }

        public List<Edge<T>> GetEdges()
        {
            List<Edge<T>> edges = new List<Edge<T>>();
            foreach (Node<T> from in Nodes)
            {
                for (int i = 0; i < from.Neightbors.Count; i++)
                {
                    Edge<T> edge = new Edge<T>()
                    {
                        From = from,
                        To = from.Neightbors[i],
                        Weight = i < from.Weights.Count ? from.Weights[i] : 0
                    };
                    edges.Add(edge);
                }
            }
            return edges;
        }

        private void UpdateIndeces()
        {
            int i = 0;
            Nodes.ForEach(n => n.Index = i++);
        }

        public List<Node<T>> DFS()
        {
            bool[] isVisited = new bool[Nodes.Count];
            List<Node<T>> result = new List<Node<T>>();
            DFS(isVisited, Nodes[0], result);
            return result;
        }

        private void DFS(bool[] isVisited, Node<T> node, List<Node<T>> result)
        {
            result.Add(node);
            isVisited[node.Index] = true;
            foreach (Node<T> neighbor in node.Neightbors)
            {
                if (!isVisited[neighbor.Index]) DFS(isVisited, neighbor, result);
            }
        }

        public List<Node<T>> BFS()
        {
            return BFS(Nodes[0]);
        }

        private List<Node<T>> BFS(Node<T> startNode)
        {
            bool[] isVisited = new bool[Nodes.Count];
            isVisited[startNode.Index] = true;
            List<Node<T>> result = new List<Node<T>>();
            Queue<Node<T>> queue = new Queue<Node<T>>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                Node<T> currentNode = queue.Dequeue();
                result.Add(currentNode);

                foreach (Node<T> neighbor in currentNode.Neightbors)
                {
                    if (!isVisited[neighbor.Index])
                    {
                        isVisited[neighbor.Index] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return result;
        }

        public List<Node<T>> TopologicalSort(out bool isPossible)
        {
            Dictionary<Node<T>, bool> visited = new();
            Dictionary<Node<T>, bool> inStack = new();
            Stack<Node<T>> result = new();

            isPossible = true;

            foreach (var node in Nodes)
            {
                visited[node] = false;
                inStack[node] = false;
            }

            foreach (var node in Nodes)
            {
                if (!visited[node])
                {
                    DFS_Topological(node, visited, inStack, result, ref isPossible);
                    if (!isPossible)
                        return null;
                }
            }

            return result.ToList();
        }

        private void DFS_Topological(
            Node<T> node,
            Dictionary<Node<T>, bool> visited,
            Dictionary<Node<T>, bool> inStack,
            Stack<Node<T>> result,
            ref bool isPossible)
        {
            visited[node] = true;
            inStack[node] = true;

            foreach (var neighbor in node.Neightbors)
            {
                if (!visited[neighbor])
                {
                    DFS_Topological(neighbor, visited, inStack, result, ref isPossible);
                }
                else if (inStack[neighbor])
                {
                    isPossible = false;
                    return;
                }
            }

            inStack[node] = false;
            result.Push(node);
        }
    }

    // Вспомогательный класс для запуска приложения
    internal class App
    {
        public void Run()
        {
            Graph<string> graph = new Graph<string>(isDirected: true, isWeighted: false);

            Node<string> A = graph.AddNode("A");
            Node<string> B = graph.AddNode("B");
            Node<string> C = graph.AddNode("C");
            Node<string> D = graph.AddNode("D");

            graph.AddEdge(A, B);
            graph.AddEdge(A, C);
            graph.AddEdge(B, D);
            graph.AddEdge(C, D);

            List<Node<string>> order = graph.TopologicalSort(out bool isPossible);

            if (isPossible)
            {
                Console.WriteLine("Вершины в порядке, где все дуги ведут от меньшего к большему:");
                for (int i = 0; i < order.Count; i++)
                {
                    Console.WriteLine($"{i} — {order[i].Data}");
                }
            }
            else
            {
                Console.WriteLine("Невозможно выполнить топологическую сортировку — есть цикл.");
            }
        }
    }

    // Точка входа
    internal class Program
    {
        private static void Main(string[] args)
        {
            App app = new App();
            app.Run();
        }
    }
}
