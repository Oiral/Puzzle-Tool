using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGridScript : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {

        RaycastHit hit;

        if (Input.GetMouseButton(0)){
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                switch (BlockSelectorScript.instance.action)
                {
                    case EditorChanger.Change:
                        SetBlockType(BlockSelectorScript.instance.selectedType, objectHit.gameObject);
                        break;
                    case EditorChanger.Remove:
                        RemoveBlock(objectHit.gameObject);
                        break;
                    case EditorChanger.Add:
                        if (objectHit.CompareTag("Ghost"))
                        {
                            Debug.Log("ghost");
                            SpawnTile(objectHit.gameObject);
                        }
                        break;
                    default:
                        break;
                }
                // Do something with the object that was hit by the raycast.
            }
        }
    }

    void SpawnTile(GameObject ghostBlockToChange)
    {
        List<GameObject> connections = new List<GameObject>();

        foreach (GameObject tileObject in ghostBlockToChange.transform.GetComponent<GhostBlockScript>().refBlocks)
        {
            connections.Add(tileObject.GetComponent<TileScript>().topPoint.gameObject);
        }
        //Spawn in the block
        GameObject spawnedTile = Instantiate(BlockSelectorScript.instance.tilePrefab, GameObject.FindGameObjectWithTag("Board").transform);

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

        StartCoroutine(ResetGhostGrid());
    }

    IEnumerator ResetGhostGrid()
    {
        BlockSelectorScript.instance.RemoveTileGrid();
        yield return new WaitForEndOfFrame();
        BlockSelectorScript.instance.AddSelected();
    }

    public void SetBlockType(TileType type, GameObject tile)
    {
        TileScript tileScript = tile.GetComponentInParent<TileScript>();
        tileScript.Type = type;

        switch (type)
        {
            case TileType.Default:
                tile.GetComponent<Renderer>().material.color = Color.white;
                break;
            case TileType.Hole:
                tile.GetComponent<Renderer>().material.color = Color.grey;
                break;
            case TileType.Goal:
                tile.GetComponent<Renderer>().material.color = Color.green;
                break;
            case TileType.Block:
                tile.GetComponent<Renderer>().material.color = Color.red;
                break;
        }

        BlockSelectorScript.instance.Log("Changed block to " + type.ToString());
        
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
        Destroy(blockToRemove);

    }
}
