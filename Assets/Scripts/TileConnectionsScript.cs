using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileConnectionsScript : MonoBehaviour {

    public List<List<GameObject>> grids = new List<List<GameObject>>() {};
    public List<GameObject> connections;

    private void OnDrawGizmosSelected()
    {
        if (connections.Count > 0)
        {
            foreach (GameObject neighbor in connections)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Average(transform.position, neighbor.transform.position), new Vector3(0.2f, 0.2f, 0.2f));
            }
        }
    }

    private Vector3 Average(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Lerp(pos1,pos2,0.4f);
        //return (pos1 + pos2) / 2;
    }


}
