using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    public Vector2Int position;
    public int gCost;
    public int hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(Vector2Int pos)
    {
        position = pos;
    }
}
public class Pathfinding : MonoBehaviour
{
    public Tilemap roadTilemap;
    public Tilemap wallTilemap;
    public Transform finishLine;
    public Vector2 startPos = new Vector2(7.5f, -0.019f);
    public float moveSpeed = 100f;

    private List<Vector3> path;
    private int pathIndex = 0;
    private bool goingToFinish = true;

    private void Start()
    {
        path = FindPath(transform.position, finishLine.position);
    }

    private void Update()
    {
        if (path == null || path.Count == 0) return;

        Vector3 targetPos = path[pathIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                // Switch target
                goingToFinish = !goingToFinish;
                Vector3 newTarget = goingToFinish ? finishLine.position : startPos;
                path = FindPath(transform.position, newTarget);
                pathIndex = 0;
            }
        }
    }

    private List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector2Int startGrid = WorldToGrid(startWorld);
        Vector2Int targetGrid = WorldToGrid(targetWorld);

        List<Node> openSet = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(startGrid);
        openSet.Add(startNode);

        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>
        {
            { startGrid, startNode }
        };

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current.position);

            if (current.position == targetGrid)
            {
                return RetracePath(current);
            }

            foreach (Vector2Int neighbour in GetNeighbours(current.position))
            {
                if (closedSet.Contains(neighbour) || !IsWalkable(neighbour))
                    continue;

                int newCost = current.gCost + 1;
                if (!allNodes.ContainsKey(neighbour) || newCost < allNodes[neighbour].gCost)
                {
                    Node neighbourNode = allNodes.ContainsKey(neighbour) ? allNodes[neighbour] : new Node(neighbour);
                    neighbourNode.gCost = newCost;
                    neighbourNode.hCost = Heuristic(neighbour, targetGrid);
                    neighbourNode.parent = current;

                    if (!openSet.Contains(neighbourNode))
                        openSet.Add(neighbourNode);

                    allNodes[neighbour] = neighbourNode;
                }
            }
        }

        return null; // No path found
    }

    private List<Vector3> RetracePath(Node endNode)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Node current = endNode;

        while (current != null)
        {
            waypoints.Add(GridToWorld(current.position));
            current = current.parent;
        }

        waypoints.Reverse();
        return waypoints;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector2Int> GetNeighbours(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
        yield return new Vector2Int(pos.x, pos.y - 1);
    }

    private bool IsWalkable(Vector2Int pos)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        return roadTilemap.HasTile(tilePos) && !wallTilemap.HasTile(tilePos);
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cellPos = roadTilemap.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return roadTilemap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
    }
}
