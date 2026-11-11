using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

/// <summary>
/// Class containing all dungeon map generation functionality
/// </summary>
public class ChunkGen : NetworkBehaviour
{
    public enum dungeonTile { air, floor1, wallBase }
    private Dictionary<dungeonTile, Tile> tileHash = new Dictionary<dungeonTile, Tile>();
    private Tile voidTile;

    private Tilemap dungeonGround;
    private Tilemap dungeonCollidable;
    private Tilemap dungeonDecorations;

    private void ProcessChunks(string[,] layout)
    {
        
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateGroundTileRpc(Vector2 chunk, Vector2 coordinate, dungeonTile tileType)
    {
        Vector3Int abspos = Vector3Int.FloorToInt(new Vector3(chunk.x * coordinate.x, chunk.y * coordinate.y, 0));
        if (tileType == dungeonTile.air)
        {
            dungeonGround.SetTile(abspos, voidTile);
        }
        else
        {
            dungeonGround.SetTile(abspos, tileHash[tileType]);
        }

    }

    [Rpc(SendTo.Everyone)]
    private void UpdateWallTileRpc(Vector2 chunk, Vector2 coordinate, dungeonTile tileType)
    {
        if (tileType == dungeonTile.air) { return; }

        Vector3Int abspos = Vector3Int.FloorToInt(new Vector3(chunk.x * coordinate.x, chunk.y * coordinate.y, 0));
        dungeonGround.SetTile(abspos, tileHash[tileType]);
    }

}

