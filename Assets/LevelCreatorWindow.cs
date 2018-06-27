using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelCreatorWindow : EditorWindow
{
    bool generatingGrid;


    GameObject tileprefab;
    GameObject board;


    Vector2 gridSetup;

    GameObject[,] tiles;
    Dictionary<Direction, int[]> directionMap = new Dictionary<Direction, int[]>()
    {
        {Direction.North, new int[2]{0,1} },
        {Direction.South, new int[2]{0,-1} },
        {Direction.East, new int[2]{1,0} },
        {Direction.West, new int[2]{-1,0} }
    };

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Level Generator")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        LevelCreatorWindow window = (LevelCreatorWindow)EditorWindow.GetWindow(typeof(LevelCreatorWindow));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        //generatingGrid = EditorGUILayout.Toggle("----------Generate Grid----------", generatingGrid);

        generatingGrid = EditorGUILayout.BeginToggleGroup(new GUIContent("Generate Grid","The Toggle for the basic generation of the grid"), generatingGrid);
        if (generatingGrid)
        {
            //EditorGUILayout.ObjectField(tileprefab);
            //tileprefab = (GameObject)EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Tile Prefab", tileprefab, typeof(GameObject));
            tileprefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Tile Prefab","The base tile of which the grid will be generated"),tileprefab, typeof(GameObject), false);

            gridSetup = EditorGUILayout.Vector2Field("X and Z amount", gridSetup);


            if (GUILayout.Button(new GUIContent("Generate Level","This will create a new level \n\n---WARNING---\nThis will destroy your previous level")))
            {
                SpawnObject();
            }
        }
        EditorGUILayout.EndToggleGroup();
    }

    private void OnSelectionChange()
    {
        if (Selection.gameObjects != null)//If something was selected
        {
            if (Selection.gameObjects[0].CompareTag("Tile"))//If it was a tile that was selected
            {
                Debug.Log("Changed");
            }else if (Selection.gameObjects[0].CompareTag("Ghost"))//If it was a ghost block that was selected
            {
                Debug.Log("Ghost Block!");
            }
        }
    }


    void SpawnObject()
    {
        //Check if there is a board
        if (GameObject.FindGameObjectWithTag("Board"))
        {
            DestroyImmediate(GameObject.FindGameObjectWithTag("Board"));
        }
        //board = Instantiate(new GameObject(), null);
        board = new GameObject();
        board.tag = "Board";
        board.name = "Level";


        int xAmount = (int)gridSetup.x;
        int zAmount = (int)gridSetup.y;

        tiles = new GameObject[xAmount, zAmount];

        //Creating the tile
        for (int x = 0; x < xAmount; x++)
        {
            for (int z = 0; z < zAmount; z++)
            {
                Vector3 boxPos = new Vector3(x, 0, z);
                GameObject tile = Instantiate(tileprefab, boxPos, Quaternion.identity, board.transform);
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
                        topConnectionScript.connections.Add((tiles[newTile[0], newTile[1]]).GetComponent<TileScript>().topPoint.gameObject);
                    }

                }
            }
        }

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().targetTile = tiles[0, 0].GetComponent<TileScript>().topPoint.gameObject;

    }


}
