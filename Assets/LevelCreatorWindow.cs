using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum EditorActions { Change, Remove, Add, BlockAdd, BlockRemove , Nothing};

public class LevelCreatorWindow : EditorWindow
{
    //Toggle group bools
    bool generatingGrid;
    bool editingTiles;
    bool extraSettings;

    Vector2 scrollPos = Vector2.zero;

    bool gridBeenBuilt = false;

    bool ghostGridActive;
    bool ghostGridLastCheck;
    GameObject ghostGridParent;
    GameObject ghostBlock;

    EditorActions currentAction = EditorActions.Nothing;
    TileType currentTileTypeAction = TileType.Default;
    Material defaultTileMat;
    Material blockTileMat;
    Material goalTileMat;


    GameObject tileprefab;
    GameObject board;
    GameObject pushablePrefab;

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

        //Toggle group for the grid generation
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Space(20);
        GenerationToggleGroup();
        GUILayout.Space(20);
        EditLevelToggleGroup();
        GUILayout.Space(20);
        SettingsToggleGroup();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();


        //Check if there is a parent for the ghost grid
        if (ghostGridParent == null)
        {
            ghostGridParent = new GameObject();
            ghostGridParent.transform.name = "Ghost Grid Parent";
        }
        //check if we need to spawn in the ghost grid
        if (ghostGridActive != ghostGridLastCheck)
        {
            //update ghost grid
            if (ghostGridActive)
            {
                //Spawn in ghost grid
                GenerateGhostGrid();
            }
            else
            {
                //Remove Ghost Grid
                RemoveGhostGrid();
            }
        }
        ghostGridLastCheck = ghostGridActive;


        //check if there is a level in the scene
        if (GameObject.FindGameObjectWithTag("Board") == null) 
        {
            gridBeenBuilt = false;
        }
    }



    private void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)//If something was selected
        {
            GameObject activeObject = Selection.activeGameObject;
            if (activeObject.CompareTag("Tile"))//If it was a tile that was selected
            {
                switch (currentAction)
                {
                    case EditorActions.Change:
                        SetBlockType(currentTileTypeAction, activeObject);
                        Selection.SetActiveObjectWithContext(null,new Object());
                        break;
                    case EditorActions.Remove:
                        RemoveBlock(activeObject);
                        Selection.activeObject = null;
                        Debug.Log("Removing Block");
                        break;

                    case EditorActions.BlockAdd:
                        SpawnPushableBlock(activeObject);
                        break;
                    default:
                        break;
                }
            }else if (activeObject.CompareTag("Ghost") && currentAction == EditorActions.Add)
            {
                Debug.Log("ghost");
                SpawnTile(activeObject);
                Selection.activeObject = null;
            }
        }
    }

    public void SetBlockType(TileType type, GameObject tile)
    {
        TileScript tileScript = tile.GetComponentInParent<TileScript>();
        tileScript.Type = type;

        switch (type)
        {
            case TileType.Default:
                tile.GetComponent<Renderer>().material = defaultTileMat;
                break;
            case TileType.Goal:
                tile.GetComponent<Renderer>().material = goalTileMat;
                break;
            case TileType.Block:
                tile.GetComponent<Renderer>().material = blockTileMat;
                break;
        }

    }

    void SpawnBaseLevel()
    {
        gridBeenBuilt = true;
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



    void GenerateGhostGrid()
    {
        if (gridBeenBuilt)
        {
            foreach (TileScript tile in GameObject.FindGameObjectWithTag("Board").GetComponentsInChildren<TileScript>())
            {
                GenerateGhostTile(tile.gameObject);
            }
        }
    }

    public void RemoveGhostGrid()
    {
        if (gridBeenBuilt)
        {
            foreach (Transform ghostTile in ghostGridParent.GetComponentsInChildren<Transform>())
            {
                if (ghostTile != ghostGridParent.transform)
                {
                    DestroyImmediate(ghostTile.gameObject);
                }
            }
        }
    }

    void GenerateGhostTile(GameObject tile)
    {
        //Directions from the tile that are missing connections
        List<Direction> missingDirections = new List<Direction>() { Direction.East, Direction.West, Direction.North, Direction.South };
        Vector3 startingPos = tile.transform.position;

        //Check the connections find if any are missing
        TileConnectionsScript tileConnections = tile.GetComponentInChildren<TileConnectionsScript>();
        foreach (GameObject connection in tileConnections.connections)
        {
            Vector3 checkingPos = connection.transform.position;

            if (startingPos.x < checkingPos.x)
            {
                //Debug.Log("Have East");
                missingDirections.Remove(Direction.East);
            }
            else if (startingPos.x > checkingPos.x)
            {
                //Debug.Log("Have West");
                missingDirections.Remove(Direction.West);
            }
            else if (startingPos.z < checkingPos.z)
            {
                //Debug.Log("Have North");
                missingDirections.Remove(Direction.North);
            }
            else if (startingPos.z > checkingPos.z)
            {
                //Debug.Log("Have South");
                missingDirections.Remove(Direction.South);
            }
        }




        //iterate through the missing directions
        foreach (Direction dir in missingDirections)
        {
            bool spawn = true;
            Vector3 SpawnPos = tile.transform.position;
            switch (dir)
            {
                case Direction.North:
                    //Debug.Log("Spawning North");
                    if (CheckIfGhost(Direction.North, tile))
                    {
                        spawn = false;
                    }
                    SpawnPos += new Vector3(0, 0, 1);
                    break;
                case Direction.South:
                    //Debug.Log("Spawning South");
                    if (CheckIfGhost(Direction.South, tile))
                    {
                        spawn = false;
                    }
                    SpawnPos += new Vector3(0, 0, -1);
                    break;
                case Direction.East:
                    //Debug.Log("Spawning East");
                    if (CheckIfGhost(Direction.East, tile))
                    {
                        spawn = false;
                    }
                    SpawnPos += new Vector3(1, 0, 0);
                    break;
                case Direction.West:
                    //Debug.Log("Spawning West");
                    if (CheckIfGhost(Direction.West, tile))
                    {
                        spawn = false;
                    }
                    SpawnPos += new Vector3(-1, 0, 0);
                    break;
            }
            if (spawn)
            {
                GameObject temp = Instantiate(ghostBlock, SpawnPos, Quaternion.identity, ghostGridParent.transform);
                temp.GetComponent<GhostBlockScript>().refBlocks.Add(tile);
            }
        }

    }

    bool CheckIfGhost(Direction dir, GameObject tile)
    {
        //check if there is already a ghost block in that spot
        Vector3 raycastDir;
        switch (dir)
        {
            case Direction.North:
                raycastDir = new Vector3(0, 0, 1);
                break;
            case Direction.South:
                raycastDir = new Vector3(0, 0, -1);
                break;
            case Direction.East:
                raycastDir = new Vector3(1, 0, 0);
                break;
            case Direction.West:
                raycastDir = new Vector3(-1, 0, 0);
                break;
            default:
                raycastDir = new Vector3(0, 0, 0);
                break;
        }
        //Debug.Log("Checking dir");
        //Raycast?
        RaycastHit hit;

        if (Physics.Raycast(tile.transform.position, raycastDir, out hit, 1f))
        {
            Debug.DrawRay(tile.transform.position, raycastDir * hit.distance, Color.yellow);
            //Debug.Break();
            Debug.Log("Did Hit");
            if (hit.transform.CompareTag("Ghost"))
            {
                hit.transform.GetComponent<GhostBlockScript>().refBlocks.Add(tile);
            }
            return true;
        }
        else
        {
            return false;
        }

        //if so remove that direction and add this tile to its ref blocks
    }

    public void RemoveBlock(GameObject blockToRemove)
    {
        TileConnectionsScript connectionsScript = blockToRemove.GetComponentInChildren<TileConnectionsScript>();

        //remove the connections going to this block
        foreach (GameObject connection in connectionsScript.connections)
        {
            GameObject topConnectionPoint = blockToRemove.GetComponent<TileScript>().topPoint.gameObject;

            //check if it is connected
            if (connection.GetComponentInChildren<TileConnectionsScript>().connections.Contains(topConnectionPoint))
            {
                connection.GetComponentInChildren<TileConnectionsScript>().connections.Remove(topConnectionPoint);
            }
        }

        //Destroy the tile clicked
        DestroyImmediate(blockToRemove);

    }

    void SpawnTile(GameObject ghostBlockToChange)
    {
        List<GameObject> connections = new List<GameObject>();

        foreach (GameObject tileObject in ghostBlockToChange.transform.GetComponent<GhostBlockScript>().refBlocks)
        {
            connections.Add(tileObject.GetComponent<TileScript>().topPoint.gameObject);
        }
        //Spawn in the block
        GameObject spawnedTile = Instantiate(tileprefab, GameObject.FindGameObjectWithTag("Board").transform);

        //Set the connections
        TileConnectionsScript spawnedTileConnections = spawnedTile.GetComponentInChildren<TileConnectionsScript>();
        spawnedTileConnections.connections = connections;

        //Set each connection to be connected to this one
        foreach (GameObject otherCons in connections)
        {
            otherCons.GetComponent<TileConnectionsScript>().connections.Add(spawnedTile.GetComponent<TileScript>().topPoint.gameObject);
        }

        //Set the position

        spawnedTile.transform.position = ghostBlockToChange.transform.position;

        //Set the name
        spawnedTile.transform.name = "Cube (" + spawnedTile.transform.position.x + "," + spawnedTile.transform.position.z + ")";

        ResetGhostGrid();
    }

    public void SpawnPushableBlock(GameObject tileToSpawnOn)
    {
        TileScript tileScript = tileToSpawnOn.GetComponent<TileScript>();
        switch (tileScript.Type)
        {
            case TileType.Default:
                GameObject pushableTemp = Instantiate(pushablePrefab, tileScript.topPoint.transform.position, Quaternion.identity);
                pushableTemp.GetComponent<PlayerMovement>().targetTile = tileScript.topPoint.gameObject;

                break;
            default:
                break;
        }
        tileScript.Type = TileType.Pushable;
    }

    void ResetGhostGrid()
    {
        RemoveGhostGrid();
        GenerateGhostGrid();
    }

    #region ToggleGroups

    void GenerationToggleGroup()
    {
        generatingGrid = EditorGUILayout.BeginToggleGroup(new GUIContent("Generate Grid", "The Toggle for the basic generation of the grid"), generatingGrid);
        if (generatingGrid)
        {
            //EditorGUILayout.ObjectField(tileprefab);
            //tileprefab = (GameObject)EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Tile Prefab", tileprefab, typeof(GameObject));
            

            gridSetup = EditorGUILayout.Vector2Field("Height and Depth of generated grid", gridSetup);

            string genGridButtonText;
            if (gridBeenBuilt)
            {
                genGridButtonText = "Regenerate Grid";
            }
            else
            {
                genGridButtonText = "Generate Level";
            }
            if (GUILayout.Button(new GUIContent(genGridButtonText, "This will create a new level \n\n---WARNING---\nThis will destroy your previous level")))
            {
                //Check if everything is ok to spawn in
                SpawnBaseLevel();
            }
        }
        EditorGUILayout.EndToggleGroup();
    }

    void EditLevelToggleGroup()
    {
        editingTiles = EditorGUILayout.BeginToggleGroup(new GUIContent("Edit the level", "The Toggle for the basic generation of the grid"), editingTiles);
        if (editingTiles)
        {
            SetColor(EditorActions.Nothing);
            if (GUILayout.Button("Stop Editing"))
            {
                currentAction = EditorActions.Nothing;
                ghostGridActive = false;
            }
            GUILayout.Space(10);


            SetColor(EditorActions.Add);
            if (GUILayout.Button("Create new tiles"))
            {
                currentAction = EditorActions.Add;
                ghostGridActive = true;
            }
            SetColor(EditorActions.Remove);
            if (GUILayout.Button("Remove existing tile"))
            {
                currentAction = EditorActions.Remove;
                ghostGridActive = false;
            }
            GUILayout.Space(10);
            SetColor(EditorActions.Change);
            if (GUILayout.Button("Change tiles"))
            {
                currentAction = EditorActions.Change;
                ghostGridActive = false;
            }

            if (currentAction == EditorActions.Change)
            {
                ChangeToggleGroup();
            }

            GUILayout.Space(10);
            SetColor(EditorActions.BlockAdd);
            if (GUILayout.Button("Add a pushable Block"))
            {
                currentAction = EditorActions.BlockAdd;
                ghostGridActive = false;
            }
            SetColor(EditorActions.BlockRemove);
            if (GUILayout.Button("Remove pushable Block"))
            {
                currentAction = EditorActions.BlockRemove;
                ghostGridActive = false;
            }
            GUI.color = Color.white;

        }
        EditorGUILayout.EndToggleGroup();
    }

    void ChangeToggleGroup()
    {
        SetColor(TileType.Default);
        if (GUILayout.Button("Normal Tile"))
        {
            currentTileTypeAction = TileType.Default;
        }

        SetColor(TileType.Block);
        if (GUILayout.Button("Block Tile"))
        {
            currentTileTypeAction = TileType.Block;
        }

        SetColor(TileType.Goal);
        if (GUILayout.Button("Goal Tile"))
        {
            currentTileTypeAction = TileType.Goal;
        }
    }

    void SettingsToggleGroup()
    {
        
           extraSettings = EditorGUILayout.BeginToggleGroup("Extra Settings", extraSettings);
        if (extraSettings)
        {
            tileprefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Tile Prefab", "The base tile of which the grid will be generated"), tileprefab, typeof(GameObject), false);
            ghostBlock = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Ghost Prefab", "The base tile that will generate when you want to add more to the grid"), ghostBlock, typeof(GameObject), false);
            pushablePrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Pushable Prefab", "The pushable block"), pushablePrefab, typeof(GameObject), false);
            GUILayout.Space(10);

            defaultTileMat = (Material)EditorGUILayout.ObjectField("Normal Tile Material", defaultTileMat, typeof(Material), false);
            blockTileMat = (Material)EditorGUILayout.ObjectField("Block Tile Material", blockTileMat, typeof(Material), false);
            goalTileMat = (Material)EditorGUILayout.ObjectField("Goal Tile Material", goalTileMat, typeof(Material), false);
        }
        EditorGUILayout.EndToggleGroup();
    }
    
    void SetColor(EditorActions check)
    {
        if (currentAction == check)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.white;
        }
    }

    void SetColor(TileType check)
    {
        if (currentTileTypeAction == check)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.white;
        }
    }

    void SetColor(TileType check,Color activeColor)
    {
        if (currentTileTypeAction == check)
        {
            GUI.color = activeColor;
        }
        else
        {
            GUI.color = Color.white;
        }
    }

    #endregion
}
