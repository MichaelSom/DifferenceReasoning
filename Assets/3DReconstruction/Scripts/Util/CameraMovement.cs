using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
            transform.position += transform.TransformDirection(Vector3.forward) * speed;
        else if (Input.GetKey(KeyCode.S))
            transform.position += transform.TransformDirection(Vector3.back) * speed;
        if (Input.GetKey(KeyCode.A))
            transform.position += transform.TransformDirection(Vector3.left) * speed;
        else if (Input.GetKey(KeyCode.D))
            transform.position += transform.TransformDirection(Vector3.right) * speed;
        if (Input.GetKey(KeyCode.Space))
            transform.position += Vector3.up * speed;
		else if (Input.GetKey(KeyCode.LeftShift))
            transform.position += Vector3.down * speed;
        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(Vector3.up, 5 * speed);
        else if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.down, 5 * speed);
	}
}
