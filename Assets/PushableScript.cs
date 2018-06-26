using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableScript : MonoBehaviour {

    public GameObject currTile;
    public GameObject prevTile;

    TileType prevTileType = TileType.Default;

    private void Start()
    {
        prevTile = currTile = GetComponent<PlayerMovement>().targetTile;

        prevTile.GetComponentInParent<TileScript>().Type = prevTileType;
        currTile.GetComponentInParent<TileScript>().Type = TileType.Pushable;

        currTile.GetComponentInParent<TileScript>().pushableCube = gameObject;
    }

    private void Update()
    {
        //update curr tile
        currTile = GetComponent<PlayerMovement>().targetTile;

        //check if we have moved
        if (currTile != prevTile)
        {
            Debug.Log("Cube Moving");
            prevTile.GetComponentInParent<TileScript>().Type = prevTileType;

            prevTileType = currTile.GetComponentInParent<TileScript>().Type;

            //Set the curr tile to be a pushable tile
            currTile.GetComponentInParent<TileScript>().Type = TileType.Pushable;
            currTile.GetComponentInParent<TileScript>().pushableCube = gameObject;

            //Reset the tile move from
            prevTile.GetComponentInParent<TileScript>().Type = prevTileType;
            prevTile.GetComponentInParent<TileScript>().pushableCube = null;
        }

        //update prev tile to curr tile
        prevTile = currTile;
    }
}
