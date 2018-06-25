using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGridScript : MonoBehaviour {

	
	
	// Update is called once per frame
	void Update () {

        RaycastHit hit;

        if (Input.GetMouseButtonDown(0)){
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                SetBlockType(BlockSelectorScript.instance.selectedType, objectHit.gameObject);
                
                // Do something with the object that was hit by the raycast.
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

}
