using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleDiskUtils;

// @author: Dominik Frey, Michael Sommerhalder, Nikhilesh Alatur
// This script displays the curent framerate of the Unity Simulation to the user.

public class FPSController : MonoBehaviour {

    public Text text;
    public Text disk;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        int fps = ((int) (1f / Time.deltaTime));
        text.text = fps + "";
	}
}
