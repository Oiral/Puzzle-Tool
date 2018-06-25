using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public PlayerMovement selectedMoveScript;
    //public PlayerMovement otherMoveScript;

    //public RotateBoard rotateScript;

    // Update is called once per frame
    void Update () {
		if (Input.GetButtonDown("Up"))
		{
            Move(Direction.North);
        }
		if (Input.GetButtonDown("Down"))
        {
            Move(Direction.South);
        }
		if (Input.GetButtonDown("Left"))
        {
            Move(Direction.West);
        }
		if (Input.GetButtonDown("Right"))
        {
            Move(Direction.East);
        }
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedMoveScript.DisableMovementInputOnRotation(rotateScript.waitTime + rotateScript.framesOfRotation * 0.02f);
            rotateScript.BeginRotation();
            //FlipSelected();
        }*/
    }

    public void Move(Direction dir)
    {
        selectedMoveScript.MovePlayer(dir);
    }
    public void TouchInputs(string dir)
    {
        switch (dir)
        {
            case "North":
                Move(Direction.North);
                break;
            case "South":
                Move(Direction.South);
                break;
            case "East":
                Move(Direction.East);
                break;
            case "West":
                Move(Direction.West);
                break;
        }
    }

    /*
    void FlipSelected()
    {
        PlayerMovement temp = selectedMoveScript;
        selectedMoveScript = otherMoveScript;
        otherMoveScript = temp;
    }*/
}
