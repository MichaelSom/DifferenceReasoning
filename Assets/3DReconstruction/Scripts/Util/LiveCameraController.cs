using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// @author: Dominik Frey, Michael Sommerhalder, Nikhilesh Alatur
/* This script enables to tap the render-pipeline of the live camera
 * in order to compute and visualise its depth-map and the depth-difference map live-cam<->ref-cam.
*/

public class LiveCameraController : MonoBehaviour {

    // Parameter, which user can set in Unity with slider: Threshold on the difference due to sensor imprecision. Required by shader.
    [Range(0f, 2f)]
    public float depthDifferenceThreshold = 0.1f;
    // Parameter, which user can set in Unity with slider: Intensity in color of the texture of the spatial mapping. Required by shader.
    [Range(0f, 1f)]
    public float spatialMappingIntensity = 1f;
    // Parameter, which user can set in Unity with slider: How the depth information in the pixels of the image are scaled. Required by shader.
    [Range(0f, 1f)] 
    public float depthLevel = 1f;

    // Parameter, which user can set in Unity with checkbox: What gets visualised depthMap of live-cam or depth-difference map live-cam<->ref-cam.
    public bool renderDepthDifference;

    // Interface object where data will be shared, which are required by the other scripts.
    public Interface interfaceObject;
    // A reference to the reference-camera object.
    public GameObject referenceCamera;
    // Shader for rendering the depth.
    public Shader depthShader;
    // Shader for rendering the depth-difference between live-camera and reference-camera.
    public Shader depthDifferenceShader;

    // A reference to the live-camera object.
    private Camera liveCamera;
    // Image, where the depth-map of the live camera will be stored.
    private RenderTexture depthT;
    // Image, where the depth-difference map (live-cam <-> ref-cam) will be stored.
    private RenderTexture depthDifferenceT;
    // Material for the depth map (required by shader in order to know how the depth-map of the scene shall be rendered).
    private Material depthMaterial;
    // Material for the depth-difference map (required by shader in order to know how the depth-difference map of the scene shall be rendered).
    private Material depthDifferenceMaterial;

	// Use this for initialization
	void Start () {
        // Initialise fields.
        liveCamera = GetComponent<Camera>();
        liveCamera.depthTextureMode = DepthTextureMode.Depth;
        depthMaterial = new Material(depthShader);
        depthDifferenceMaterial = new Material(depthDifferenceShader);
        depthT = new RenderTexture(liveCamera.pixelWidth, liveCamera.pixelHeight, 24);
        depthDifferenceT = new RenderTexture(liveCamera.pixelWidth, liveCamera.pixelHeight, 24);
    }

    // Update is called once per frame
    void Update () {
        // Set the reference-camera's position and rotation to that of the live-camera. 
        // This is needed for computing the depth maps from the same viewpoint in order to do difference reasoning.
        referenceCamera.transform.position = this.transform.position;
        referenceCamera.transform.rotation = this.transform.rotation;
        // Write the data of the live-camera to the interface object, so other scripts can use this info.
        interfaceObject.position = this.transform;
        interfaceObject.intrinsics = this.liveCamera;
    }

    // Tap the render pipeline in the last rendering step (just before Unity displays the rendered view of the 3D scene).
    // After tapping, we first run a shader script on the tapped image befor re-inserting it into the render pipeline, which 
    // will then be displayed on the screen.
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Set the parameters, which the user chose with the sliding bars in Unity.
        depthDifferenceMaterial.SetFloat("_DepthDifferenceThreshold", depthDifferenceThreshold);
        depthDifferenceMaterial.SetFloat("_SpatialMappingIntensity", spatialMappingIntensity);
        depthMaterial.SetFloat("_DepthLevel", depthLevel);

        // Retrieve the depth map of the reference camera's view (required for computing the depth-difference map).
        referenceCamera.GetComponent<ReferenceCameraController>().setDepth(depthLevel);
        referenceCamera.GetComponent<Camera>().Render();

        // User can select what gets visualised with a checkbox.
        if (renderDepthDifference){
            // Call shader to render the difference between the depth-maps of live-camera and reference-camera.
            Graphics.Blit(src, dest, depthDifferenceMaterial);
        }
        else{
            // Call shader to render the depth map.
            Graphics.Blit(src, dest, depthMaterial);
        }
        // Irrelevant of what gets visualised, call shaders to render the depth-map and the depth-difference map.
        Graphics.Blit(src, depthDifferenceT, depthDifferenceMaterial);
        Graphics.Blit(src, depthT, depthMaterial);
        // Write the rendered depth-map and depth-difference map to the interface object, so other scripts can use/access it as well.
        interfaceObject.ReloadLiveDepth(depthT);
        interfaceObject.ReloadDifference(depthDifferenceT);
        interfaceObject.hasData = true;
    }
}
