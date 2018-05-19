using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveCameraController : MonoBehaviour {

    [Range(0f, 2f)]
    public float depthDifferenceThreshold = 0.1f; // Threshold on the difference due to sensor imprecision
    [Range(0f, 1f)]
    public float spatialMappingIntensity = 1f;    // Intensity in color of the texture of the spatial mapping
    [Range(0f, 1f)] //assignes Range slider to the float depthLevel, later taken by the shader
    public float depthLevel = 1f;

    public bool renderDepthDifference;

    public Interface interfaceObject;
    public GameObject referenceCamera;
    public Shader depthShader;
    public Shader depthDifferenceShader;

    private Camera liveCamera;
    private RenderTexture depthT;
    private RenderTexture depthDifferenceT;
    private Material depthMaterial;
    private Material depthDifferenceMaterial;

	// Use this for initialization
	void Start () {
        liveCamera = GetComponent<Camera>();
        liveCamera.depthTextureMode = DepthTextureMode.Depth;
        depthMaterial = new Material(depthShader);
        depthDifferenceMaterial = new Material(depthDifferenceShader);
        depthT = new RenderTexture(liveCamera.pixelWidth, liveCamera.pixelHeight, 24);
        depthDifferenceT = new RenderTexture(liveCamera.pixelWidth, liveCamera.pixelHeight, 24);
    }

    // Update is called once per frame
    void Update () {
        referenceCamera.transform.position = this.transform.position;
        referenceCamera.transform.rotation = this.transform.rotation;
        interfaceObject.position = this.transform;
        interfaceObject.intrinsics = this.liveCamera;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        depthDifferenceMaterial.SetFloat("_DepthDifferenceThreshold", depthDifferenceThreshold);
        depthDifferenceMaterial.SetFloat("_SpatialMappingIntensity", spatialMappingIntensity);
        depthMaterial.SetFloat("_DepthLevel", depthLevel);

        referenceCamera.GetComponent<ReferenceCameraController>().setDepth(depthLevel);
        referenceCamera.GetComponent<Camera>().Render();

        if (renderDepthDifference)
        {
            Graphics.Blit(src, dest, depthDifferenceMaterial);
        }else
        {
            Graphics.Blit(src, dest, depthMaterial);
        }
        Graphics.Blit(src, depthDifferenceT, depthDifferenceMaterial);
        Graphics.Blit(src, depthT, depthMaterial);
        interfaceObject.ReloadLiveDepth(depthT);
        interfaceObject.ReloadDifference(depthDifferenceT);
        interfaceObject.hasData = true;
    }
}
