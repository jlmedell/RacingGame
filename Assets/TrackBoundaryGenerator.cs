using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TrackBoundaryGenerator : MonoBehaviour
{
    public Tilemap roadTilemap;        // Assign in Inspector
    public Tilemap boundaryTilemap;    // Assign in Inspector
    public TileBase wallTile;          // Assign your wall tile here

    public Vector2Int minGridPos;      // Bottom-left corner of track area
    public Vector2Int maxGridPos;      // Top-right corner of track area

    void Start()
    {
        GenerateBoundaries();
        boundaryTilemap.RefreshAllTiles();
    }
    // Call this method after your road tiles are painted/generated
    public void GenerateBoundaries()
    {
        // Collect all positions where road tiles exist
        HashSet<Vector3Int> roadPositions = new HashSet<Vector3Int>();
        foreach (var pos in roadTilemap.cellBounds.allPositionsWithin)
        {
            if (roadTilemap.HasTile(pos))
            {
                roadPositions.Add(pos);
            }
        }

        // Fill all tiles not occupied by road with wall tile
        for (int x = minGridPos.x; x <= maxGridPos.x; x++)
        {
            for (int y = minGridPos.y; y <= maxGridPos.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!roadPositions.Contains(pos))
                {
                    boundaryTilemap.SetTile(pos, wallTile);
                }
            }
        }
    }
}
