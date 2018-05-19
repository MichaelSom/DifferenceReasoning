using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleDiskUtils;

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
