using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interface : MonoBehaviour {

    //If false, the data has not yet been retrieved from the Render Pipeline. Only use the fields if this is true!
    public bool hasData;
    public Transform position;
    public Texture2D liveDepthImage;
    public Texture2D referenceDepthImage;
    public Texture2D differenceImage;
    public Camera intrinsics;           //e.g. intrinsics.cameraToWorldMatrix

    bool initializedReferenceDepth = false;
    bool initializedLiveDepth = false;
    bool initializedDifference = false;

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {

	}

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