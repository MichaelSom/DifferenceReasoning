using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@author: Michael Sommerhalder, Dominik Frey, Nikhilesh Alatur
//
//This script is attache to the Interface GameObject. It connects the DifferenceReasoning-Code with the Reconstruction Code.
//RenderTextures and Camera information get stored here.

public class Interface : MonoBehaviour {

    //If false, the data has not yet been retrieved from the Render Pipeline. Only use the fields if this is true!
    public bool hasData;
    //Position of the live camera
    public Transform position;
    //Depth image of the live camera
    public Texture2D liveDepthImage;
    //Depth image of the reference camera
    public Texture2D referenceDepthImage;
    //Depth Difference image created by both cameras
    public Texture2D differenceImage;
    //Intrinsics of the camera
    public Camera intrinsics;

    //True once the depth image of the reference camera has been loaded
    bool initializedReferenceDepth = false;
    //True once the depth image of the live camera has been loaded
    bool initializedLiveDepth = false;
    //True once the depth difference image of the live and reference camera has been loaded
    bool initializedDifference = false;

    //This function is unused but has to be overwritten by Unity
    void Start () {}

    //This function is unused but has to be overwritten by Unity
    void Update () {}

    //Reloads the reference depth image and converts the raw RenderTexture to the useful Texture2D format
    public void ReloadReferenceDepth(RenderTexture referenceDepth)
    {
        RenderTexture.active = referenceDepth;
        if (!initializedReferenceDepth)
        {
            referenceDepthImage = new Texture2D(referenceDepth.width, referenceDepth.height);
            initializedReferenceDepth = true;
        }
        referenceDepthImage.ReadPixels(new Rect(0, 0, referenceDepth.width, referenceDepth.height), 0, 0);
        referenceDepthImage.Apply();
        RenderTexture.active = null;
    }

    //Reloads the depth difference image and converts the raw RenderTexture to the useful Texture2D format
    public void ReloadDifference(RenderTexture difference)
    {
        RenderTexture.active = difference;
        if (!initializedDifference)
        {
            differenceImage = new Texture2D(difference.width, difference.height);
            initializedDifference = true;
        }
        differenceImage.ReadPixels(new Rect(0, 0, difference.width, difference.height), 0, 0);
        differenceImage.Apply();
        RenderTexture.active = null;
    }

    //Reloads the live depth image and converts the raw RenderTexture to the useful Texture2D format
    public void ReloadLiveDepth(RenderTexture liveDepth)
    {
        RenderTexture.active = liveDepth;
        if (!initializedLiveDepth)
        {
            liveDepthImage = new Texture2D(liveDepth.width, liveDepth.height);
            initializedLiveDepth = true;
        }
        liveDepthImage.ReadPixels(new Rect(0, 0, liveDepth.width, liveDepth.height), 0, 0);
        liveDepthImage.Apply();
        RenderTexture.active = null;
    }

}