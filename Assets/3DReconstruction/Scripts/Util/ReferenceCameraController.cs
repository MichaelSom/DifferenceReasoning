using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceCameraController : MonoBehaviour {

    public Interface interfaceObject;
    public Shader depthShader;

    private RenderTexture depthT;       //Buffer to store current Display content
    private Material depthMaterial;     //Material to render depth
    private float depthLevel;           //
    private Camera referenceCamera;

	// Use this for initialization
	void Start () {
        referenceCamera = GetComponent<Camera>();
        referenceCamera.depthTextureMode = DepthTextureMode.Depth;
        depthMaterial = new Material(depthShader);
        depthT = new RenderTexture(referenceCamera.pixelWidth, referenceCamera.pixelHeight, 24);
    }

    // Update is called once per frame
    void Update () {

	}

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        depthMaterial.SetFloat("_DepthLevel", depthLevel);
        Graphics.Blit(src, depthT, depthMaterial);
        interfaceObject.ReloadReferenceDepth(depthT);
    }

    //Called by LiveCamera
    public void setDepth(float depth)
    {
        this.depthLevel = depth;
    }
}
