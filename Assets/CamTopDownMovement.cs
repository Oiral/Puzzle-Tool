using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTopDownMovement : MonoBehaviour {

    public bool dragging;

    public float dragSpeed = 1;

    public Vector3 dragOrigin = new Vector3(0, 0, 0);

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(2))
        {
            dragging = true;
            dragOrigin = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            dragging = false;
        }

        if (dragging == true)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

            transform.Translate(move, Space.World);
        }


    }
}
