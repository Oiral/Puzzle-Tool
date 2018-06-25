using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectorScript : MonoBehaviour {

    public TileType selectedType;

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
                break;
            case "Goal":

                selectedType = TileType.Goal;
                break;
            case "Hole":

                selectedType = TileType.Hole;
                break;
            case "Block":

                selectedType = TileType.Block;
                Log(changeTo);
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


}
