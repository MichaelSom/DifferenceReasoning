using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SparseVoxelGrid))]
public class TESTING : MonoBehaviour {

    [ReadOnly]
    public Vector2Int mousePos;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        mousePos.x = (int)Input.mousePosition.x;
        mousePos.y = (int)Input.mousePosition.y;
	}
}
