using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@author: Michael Sommerhalder, Dominik Frey, Nikhilesh Alatur
//
//This is the main script that gets executed either once per frame or via the UI Reconstruction Button.
//It holds the HashTable field and the functions to reconstruct the scene.

public class TSDF : MonoBehaviour {

    //The update frame is increased every time the reconstruction is executed
    static int update_frame = 1;

    //Reference to the interfae
    public Interface interface_;

    //HashTable field
    public SparseVoxelGrid SVG;

    //Toggle if reconstruction is done offline or online
    public bool constant_reconstruction;

    //Calculates the time used to reconstruct the scene if in offline mode
    [ReadOnly]
    public float last_update_duration = 0f;

    //Voxel is Rendered if btw threshold values
    //TSDF parameters
    [Header("Lower and Upper Render threshold")]
    public float tsdf_max;
    public float tsdf_slope;
    public float tsdf_thres;

    //How many random samples should be drawn from single snapshot?
    [Header("Number of MonteCarlo Samples")]
    public int number_of_samples;
    public int number_of_samples_realtime;

    //This function is unused but has to be overwritten by Unity
    void Start () {}

    //If it is reconstructed online, ReconstructScene() gets called every frame
    void Update () {
        if (constant_reconstruction)
        {
            number_of_samples = number_of_samples_realtime;
            ReconstructScene();
        }
    }

    //Main Method to Update Voxel Value.
    //Input:
    //value - calculated from the TSD Function.
    //x, y, z - grid coordinates of Voxel to update
    //current_update_frame - this ensures that voxels only get updated once

    //Main Method Called From Reconstruction Button or via online update
    public void ReconstructScene()
    {
        Debug.Log("Reconstruction");
        //Used to measure reconstruction time
        var watch = System.Diagnostics.Stopwatch.StartNew();
        //Step 1: Choose Pixels at Random and Project Point to 3D View
        int width = interface_.differenceImage.width;
        int height = interface_.differenceImage.height;
        int ind = 0;
        for (int i = 0; i < number_of_samples; i++)
        {
            VoxelType vt = VoxelType.EMPTY;
            //1.1 Draw a Random Pixel from difference Image
            Vector2Int pixel = new Vector2Int();
            int overflow = 0;
            //1.2 Check if pixel is not empty and repeat if necessary
            while (vt == VoxelType.EMPTY && overflow < 1000)
            {
                pixel.x = (int)(Random.value * width);
                pixel.y = (int)(Random.value * height);
                vt = GetVoxelType(pixel);
                overflow++;
            }
            //1.3 Calculate coordinates of voxel from pixel depth value
            Vector3 voxelcoords = GetCoordsFromPixel(vt, pixel);
            //1.4 Recieve DenseVoxelGrid at this position or create a new one if necessary
            DenseVoxelGrid dvg = SVG.GetDenseVoxelGrid(voxelcoords);
            //1.5 Update DenseVoxelGrid at this position by backprojecting all voxels
            UpdateDenseVoxelGrid(dvg, update_frame);
            ind++;
        }
        Debug.Log(ind + " Pixels found a surface");

        //Step 3: Look for empty voxel blocks and remove them
        SVG.RemoveEmptyVoxelGrids();

        update_frame++;

        //Display elapsed time
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("Reconstruction took " + elapsedMs + "Milliseconds");
    }

    //Function to update each voxel within a voxel grid
    public void UpdateDenseVoxelGrid(DenseVoxelGrid dvg, int current_update_frame)
    {
        //Only update if voxelgrid was not updated this frame
        if (current_update_frame == dvg.last_update_frame)
            return;

        //Do for every voxel in the Grid:
        for(int z = 0;z < dvg.grid_size.z; z++)
        {
            for (int y = 0; y < dvg.grid_size.y; y++)
            {
                for (int x = 0; x < dvg.grid_size.x; x++)
                {
                    //Convert voxelgrid coordinates to world coordinates
                    Vector3 worldcoord = dvg.DenseGridCoordToWorldCoord(x, y, z);
                    //Retrieve corresponding pixel coordinates
                    Vector2Int pixelcoord = GetPixelFromCoords(worldcoord);
                    //Check if pixel is within camera image
                    if(pixelcoord.x >= 0 && pixelcoord.y >= 0 && pixelcoord.x < interface_.differenceImage.width && pixelcoord.y < interface_.differenceImage.height)
                    {
                        //Retrieve VoxelType from pixelcoords
                        VoxelType vt = GetVoxelType(pixelcoord);
                        //Only update value if it is green/red in the image
                        if (vt != VoxelType.EMPTY)
                        {
                            //Calculate the exact distance btw the camera and the voxel
                            float dist_exact = GetDistanceToCameraScreen(worldcoord);
                            //Calculate the measured distance btw the camera and the voxel
                            float dist_measured = GetMeasuredDistance(vt, pixelcoord);
                            bool out_of_range = false;
                            //Calculate the TSDF value of this voxel
                            float tsdf = GetTSDFValue(dist_exact, dist_measured, out out_of_range);
                            //If the voxel is out of range (last part of the tsdf), truncate, else update the voxel
                            if (!out_of_range)
                                dvg.active_count += UpdateVoxel(ref dvg.grid[x, y, z], tsdf, vt);
                        }
                    }
                }
            }
        }
        //Render the updated block
        dvg.Render();

        //Set current update frame in voxelgrid
        dvg.last_update_frame = current_update_frame;
    }

    //Updates the voxel using the tsdf value and the proposed VoxelType
    public int UpdateVoxel(ref Voxel v, float tsdf, VoxelType vt)
    {
        //See paper
        v.value = (v.value * v.updates + tsdf) / (v.updates + 1);
        v.updates++;
        //If value is within threshold, set voxel to proposed VoxelType
        if (v.value > -tsdf_thres && v.value < tsdf_thres)
        {
            v.type = vt;
            return 1;
        }
        else
        {
            //Set voxel to be empty
            v.type = VoxelType.EMPTY;
            if (vt != VoxelType.EMPTY)
                return -1;
            else
                return 0;
        }
    }

    //Calculates the Pixel coordinates from world coordinates
    public Vector2Int GetPixelFromCoords(Vector3 worldcoord)
    {
        //Camera Position in World Frame
        Vector3 camPos = interface_.position.position;
        //Voxel Position in World Frame
        Vector3 voxelPos = worldcoord;
        //Calculate Distance Vector from Camera to Voxel
        Vector3 vectorCameraToVoxel = voxelPos - camPos;
        //Normal Vector of Camera Screen
        Vector3 screenDir = interface_.position.TransformDirection(Vector3.forward);
        //Calculate Angle between Screen Normal and Distance Vector btw Camera and Voxel
        float anglePixelToNormal = Vector3.Angle(vectorCameraToVoxel, screenDir) / 180 * Mathf.PI;
        //Calculate Distance Vector from Camera to Pixel in World Frame
        Vector3 vectorCameraToPixel = new Vector3()
        {
            x = vectorCameraToVoxel.x,
            y = vectorCameraToVoxel.y,
            z = vectorCameraToVoxel.z
        };
        vectorCameraToPixel.Normalize();
        vectorCameraToPixel /= Mathf.Cos(anglePixelToNormal);
        //Calculate Pixel Coordinates. x and y are pixel coordinates, z is distance
        Vector3 pixelHom = interface_.intrinsics.WorldToScreenPoint(vectorCameraToPixel + camPos);

        Vector2Int pixel = new Vector2Int((int)pixelHom.x, (int)pixelHom.y);
        return pixel;
    }

    //Calculates the world coordinates of the voxel using the depth information at the pixel coordinates
    public Vector3 GetCoordsFromPixel(VoxelType vt, Vector2Int pixelcoords)
    {
        Vector3 campos = interface_.position.position;
        Vector3 pixelpos = new Vector3(pixelcoords.x, pixelcoords.y, 1f);
        Vector3 worldpos = interface_.intrinsics.ScreenToWorldPoint(pixelpos);
        //Calculate Distance Vector from Camera to Pixel
        Vector3 vectorCameraToPixel = worldpos - campos;
        vectorCameraToPixel.Normalize();
        //Normal Vector of Camera Screen
        Vector3 screenDir = interface_.position.TransformDirection(Vector3.forward);
        //Calculate Angle between Screen Normal and Distance Vector btw Camera and Voxel
        float anglePixelToNormal = Vector3.Angle(vectorCameraToPixel, screenDir) / 180 * Mathf.PI;
        float distanceToVoxel = (GetMeasuredDistance(vt, pixelcoords)) / Mathf.Cos(anglePixelToNormal);
        vectorCameraToPixel *= distanceToVoxel;
        return campos + vectorCameraToPixel;

    }

    //Returns the type visible in the depth difference image
    public VoxelType GetVoxelType(Vector2Int pixelcoord)
    {
        Color p = interface_.differenceImage.GetPixel(pixelcoord.x, pixelcoord.y);
        if (p.r > 0)
            return VoxelType.REFERENCE;
        if (p.b > 0)
            return VoxelType.LIVE;
        return VoxelType.EMPTY;
    }

    //Calculates the TSDF value using the measured and calculated distance
    public float GetTSDFValue(float dist1, float dist2, out bool out_of_range)
    {
        float dif = dist1 - dist2;

        float dist = tsdf_max / tsdf_slope;

        out_of_range = false;
        if (dif < -dist)
            return tsdf_max;
        else if (dif > dist)
        {
            out_of_range = true;
            return 0;
        }
        else
            return -tsdf_slope * dif;
    }

    //Decodes depth information and returns depth in meter
    public float GetMeasuredDistance(VoxelType vt, Vector2Int pixelcoords)
    {
        float depthFactor = 1000;
        if (vt == VoxelType.LIVE)
            return DecodeColor(interface_.liveDepthImage.GetPixel(pixelcoords.x, pixelcoords.y)) * depthFactor;
        else
            return DecodeColor(interface_.referenceDepthImage.GetPixel(pixelcoords.x, pixelcoords.y)) * depthFactor;
    }

    //Calculates the distance from a point in worldspace to the camera screen's position
    public float GetDistanceToCameraScreen(Vector3 worldcoord)
    {
        //Camera Position in World Frame
        Vector3 camPos = interface_.position.position;
        //Voxel Position in World Frame
        Vector3 voxelPos = worldcoord;
        //Calculate Distance Vector from Camera to Voxel
        Vector3 vectorCameraToVoxel = voxelPos - camPos;
        //Normal Vector of Camera Screen
        Vector3 screenDir = interface_.position.TransformDirection(Vector3.forward);

        float angle = Vector3.Angle(vectorCameraToVoxel, screenDir) / 180 * Mathf.PI;

        return Vector3.Magnitude(vectorCameraToVoxel) * Mathf.Cos(angle);
    }

    //Decodes the depth encoded by the depth shader
    private float DecodeColor(Color c)
    {
        Vector4 color1 = new Vector4(c.r, c.g, c.b, c.a);
        Vector4 color2 = new Vector4(1f, 1f / 255, 1f / 65025, 1f / 160581375);
        return Vector4.Dot(color1, color2);
    }

}
