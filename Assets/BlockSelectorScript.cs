using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EditorChanger { Change,Remove,Add};

public class BlockSelectorScript : MonoBehaviour {

    public TileType selectedType;

    public EditorChanger action;
    public GameObject blockAddGridBlock;
    public Transform addGridParent;

    public GameObject tilePrefab;

    public bool modifyGrid = false;

    public static BlockSelectorScript instance;

    public Text logText;

    private void Start()
    {
        instance = this;
    }

    public void ChangeType(string changeTo)
    {
        switch (changeTo)
        {
            case "Default":
                selectedType = TileType.Default;
                action = EditorChanger.Change;
                RemoveTileGrid();
                break;
            case "Goal":

                selectedType = TileType.Goal;
                action = EditorChanger.Change;
                RemoveTileGrid();
                break;
            case "Hole":

                selectedType = TileType.Hole;
                action = EditorChanger.Change;
                RemoveTileGrid();
                break;
            case "Block":

                selectedType = TileType.Block;
                action = EditorChanger.Change;
                Log(changeTo);
                RemoveTileGrid();
                break;

            case "Remove":
                action = EditorChanger.Remove;
                RemoveTileGrid();
                break;
            case "Add":
                action = EditorChanger.Add;
                AddSelected();
                break;
            default:
                break;
        }
        Log(changeTo + " Selected");
    }

    public void Log(string StringToLog)
    {
        logText.text = StringToLog;
    }

    public void AddSelected()
    {
        foreach (TileScript tile in GameObject.FindGameObjectWithTag("Board").GetComponentsInChildren<TileScript>())
        {
            generateTileGrid(tile.gameObject);
        }
    }

    public void RemoveTileGrid()
    {
        foreach (Transform ghostTile in addGridParent.GetComponentsInChildren<Transform>())
        {
            if (ghostTile != addGridParent)
            {
                Destroy(ghostTile.gameObject);
            }
        }
    }

    public void generateTileGrid(GameObject tile)
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
                GameObject temp = Instantiate(blockAddGridBlock, SpawnPos, Quaternion.identity, addGridParent);
                temp.GetComponent<GhostBlockScript>().refBlocks.Add(tile);
            }
        }

    }

    private bool CheckIfGhost(Direction dir, GameObject tile)
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
            Debug.DrawRay(transform.position, raycastDir * hit.distance, Color.yellow);
            //Debug.Break();
            Debug.Log("Did Hit");
            if (hit.transform.CompareTag("Ghost"))
            {
                hit.transform.GetComponent<GhostBlockScript>().refBlocks.Add(tile);
            }
            return true;
        }else
        {
            return false;
        }

        //if so remove that direction and add this tile to its ref blocks
    }

}
