using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

    public int xAmount;
    public int zAmount;

    public GameObject boxPrefab;
    
    GameObject[,] tiles;
    Dictionary<Direction, int[]> directionMap = new Dictionary<Direction, int[]>()
    {
        {Direction.North, new int[2]{0,1} },
        {Direction.South, new int[2]{0,-1} },
        {Direction.East, new int[2]{1,0} },
        {Direction.West, new int[2]{-1,0} }
    };


    private void Start()
    {
        tiles = new GameObject[xAmount, zAmount];

        //Creating the tile
        for (int x = 0; x < xAmount; x++)
        {
            for (int z = 0; z < zAmount; z++)
            {
                Vector3 boxPos = new Vector3(x, 0, z);
                GameObject tile = Instantiate(boxPrefab, boxPos, Quaternion.identity, transform);
                tiles[x, z] = tile;
                tile.name = "Cube (" + x + "," + z + ")";
            }
        }
        for (int x = 0; x < xAmount; x++)
        {
            for (int z = 0; z < zAmount; z++)
            {
                GameObject tile = tiles[x, z];
                TileConnectionsScript topConnectionScript = tile.GetComponent<TileScript>().topPoint;
                for (int i = 0; i < 4; i++)
                {
                    int[] mod = directionMap[(Direction)i];
                    int[] newTile = new int[] { x + mod[0], z + mod[1] };
                    if (((newTile[0] < 0 || newTile[0] >= xAmount) || (newTile[1] < 0 || newTile[1] >= zAmount)) == false)
                    {
                        topConnectionScript.connections.Add((tiles[newTile[0],newTile[1]]).GetComponent<TileScript>().topPoint.gameObject);
                    }

                }
            }
        }

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().targetTile = tiles[0, 0].GetComponent<TileScript>().topPoint.gameObject;

    }
}
