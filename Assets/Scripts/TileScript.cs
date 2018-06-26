using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { Default,Hole,Goal,Block,Pushable}

public class TileScript : MonoBehaviour {
    public TileConnectionsScript topPoint;

    public TileType Type = TileType.Default;

    public GameObject pushableCube;
}
