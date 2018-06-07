using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// @author: Dominik Frey, Michael Sommerhalder, Nikhilesh Alatur
// This script enables to tap the render-pipeline of the reference camera in order to compute its depth-map.

public class ReferenceCameraController : MonoBehaviour {

    // Interface object where data will be shared, which are required by the other scripts.
    public Interface interfaceObject;
    // Shader for rendering the depth.
    public Shader depthShader;

    // Image, where the depth-map of the live camera will be stored.
    private RenderTexture depthT;
    // Material for the depth map (required by shader in order to know how the depth-map shall be rendered).
    private Material depthMaterial;
    // How the depth information in the pixels of the image are scaled.Required by shader.
    private float depthLevel;    
    // A reference to the reference-camera object.
    private Camera referenceCamera;

	// Use this for initialization
	void Start () {
        // Initialise fields.
        referenceCamera = GetComponent<Camera>();
        referenceCamera.depthTextureMode = DepthTextureMode.Depth;
        depthMaterial = new Material(depthShader);
        depthT = new RenderTexture(referenceCamera.pixelWidth, referenceCamera.pixelHeight, 24);
    }

    // Update is called once per frame
    void Update () {

	}

    // Tap the render pipeline in the last rendering step (just before Unity displays the rendered view of the 3D scene).
    // After tapping, we first run a shader script on the tapped image befor re-inserting it into the render pipeline, which 
    // will then be displayed on the screen.
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Set the parameters.
        depthMaterial.SetFloat("_DepthLevel", depthLevel);
        // Call shader to render the depth map.
        Graphics.Blit(src, depthT, depthMaterial);
        // Write the rendered depth-map to the interface object, so other scripts can use/access it as well.
        interfaceObject.ReloadReferenceDepth(depthT);
    }

    // Called by LiveCamera
    public void setDepth(float depth)
    {
        this.depthLevel = depth;
    }
}
