using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DATA STRUCTURES.
public enum VoxelType
{
    EMPTY,
    LIVE,
    REFERENCE
}

public struct Voxel
{
    // The current value of the TSDF.
    public float value;
    // How many updates did the voxel have?
    public int updates;
    // What was the last update frame?
    public int last_update_frame;
    // Is the voxel seen from reference or live camera?
    public VoxelType type;      
}

// MAIN CLASS.
public class DenseVoxelGrid : MonoBehaviour
{
    // 3D Array where Voxels are stored.
    public Voxel[ , , ] grid;

    // Number of voxels.
    [Header("Voxel count in x, y and z direction")]
    public Vector3Int grid_size;

    //Number of active voxels in grid (active!=EMPTY)
    [Header("Number of active voxels in grid")]
    [ReadOnly]
    public int active_count;

    [Header("Last Update Frame")]
    [ReadOnly]
    public int last_update_frame;

    private Mesh mesh;              //Mesh of the resulting PointCloud
    private List<Vector3> vertices; //Mesh vertices
    private List<Color> colors;     //Mesh vertex colors
    private List<int> indices;      //Mesh indices


    // UNITY.
    void Awake()
    {
        mesh = new Mesh();
        vertices = new List<Vector3>();
        colors = new List<Color>();
        indices = new List<int>();

        //Set all to empty, no updates, frame#=0
        grid = new Voxel[grid_size.x, grid_size.y, grid_size.z];
        for (int ix = 0; ix < grid_size.x; ix++)
        {
            for (int iy = 0; iy < grid_size.y; iy++)
            {
                for (int iz = 0; iz < grid_size.z; iz++)
                {
                    grid[ix, iy, iz].updates = 0;
                    grid[ix, iy, iz].last_update_frame = 0;
                    grid[ix, iy, iz].value = 0f;
                    grid[ix, iy, iz].type = VoxelType.EMPTY;
                }
            }
        }
    }

    public Vector3Int WorldCoordToDenseGridCoord(Vector3 worldcoord)
    {
        Vector3 local = transform.InverseTransformPoint(worldcoord) + new Vector3(0.5f, 0.5f, 0.5f);
        local.x *= grid_size.x;
        local.y *= grid_size.y;
        local.z *= grid_size.z;
        return new Vector3Int((int)local.x, (int)local.y, (int)local.z);
    }
    public Vector3 DenseGridCoordToWorldCoord(int x, int y, int z)
    {
        Vector3 local = new Vector3()
        {
            x = ((float)x) / grid_size.x - 0.5f + 1f / grid_size.x / 2f,
            y = ((float)y) / grid_size.y - 0.5f + 1f / grid_size.y / 2f,
            z = ((float)z) / grid_size.z - 0.5f + 1f / grid_size.z / 2f
        };
        return transform.TransformPoint(local);
    }
    public Vector3 DenseGridCoordToWorldCoord(Vector3Int gridcoord)
    {
        return DenseGridCoordToWorldCoord(gridcoord.x, gridcoord.y, gridcoord.z);
    }

    public Voxel GetVoxelFromGrid(Vector3Int gridcoord)
    {
        return grid[gridcoord.x, gridcoord.y, gridcoord.z];
    }
    public Voxel GetVoxelFromWorld(Vector3 worldcoord)
    {
        return GetVoxelFromGrid(WorldCoordToDenseGridCoord(worldcoord));
    }

    // Update is called once per frame
    void Update()
    {
    }

    //Used by Render Method
    private Vector3 DenseGridCoordToLocalCoord(float cx, float cy, float cz)
    {
        Vector3 local = new Vector3()
        {
            x = cx / grid_size.x - 0.5f,
            y = cy / grid_size.y - 0.5f,
            z = cz / grid_size.z - 0.5f
        };
        return local;
    }

    public void Render()
    {
        vertices.Clear();
        indices.Clear();
        colors.Clear();
        for (int ix = 0; ix < grid_size.x; ix++)
        {
            for (int iy = 0; iy < grid_size.y; iy++)
            {
                for (int iz = 0; iz < grid_size.z; iz++)
                {
                    //                    Debug.Log(grid[ix, iy, iz].updates + " " + grid[ix, iy, iz].value);
                    //If the voxel is not empty and not yet rendered, render it
                    if (grid[ix, iy, iz].type != VoxelType.EMPTY)
                    {
                        switch (grid[ix, iy, iz].type)
                        {
                            case VoxelType.REFERENCE:
                                RenderVoxel(ix, iy, iz, Color.red);
                                break;
                            case VoxelType.LIVE:
                                RenderVoxel(ix, iy, iz, Color.blue);
                                break;
                            default:
                                Debug.Log("Voxel Render Type not supported!");
                                break;
                        }
                    }
                }
            }
        }
        /*
        RenderVoxel(0, 0, 0, Color.green);
        RenderVoxel(grid_size.x - 1, 0, 0, Color.green);
        RenderVoxel(0, grid_size.y - 1, 0, Color.green);
        RenderVoxel(grid_size.x - 1, grid_size.y - 1, 0, Color.green);
        RenderVoxel(0, 0, grid_size.z - 1, Color.green);
        RenderVoxel(grid_size.x - 1, 0, grid_size.z - 1, Color.green);
        RenderVoxel(0, grid_size.y - 1, grid_size.z - 1, Color.green);
        RenderVoxel(grid_size.x - 1, grid_size.y - 1, grid_size.z - 1, Color.green);*/
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Custom_Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void RenderVoxel(int ix, int iy, int iz, Color color)
    {
        if (iy < grid_size.y - 1)
        {
            if (grid[ix, iy + 1, iz].type == VoxelType.EMPTY)
                RenderTop(ix, iy, iz, color);
        }
        else
            RenderTop(ix, iy, iz, color);

        if (iy > 0)
        {
            if (grid[ix, iy - 1, iz].type == VoxelType.EMPTY)
                RenderBottom(ix, iy, iz, color);
        }
        else
            RenderBottom(ix, iy, iz, color);

        if (ix < grid_size.x - 1)
        {
            if (grid[ix + 1, iy, iz].type == VoxelType.EMPTY)
                RenderRight(ix, iy, iz, color);
        }
        else
            RenderRight(ix, iy, iz, color);

        if (ix > 0)
        {
            if (grid[ix - 1, iy, iz].type == VoxelType.EMPTY)
                RenderLeft(ix, iy, iz, color);
        }
        else
            RenderLeft(ix, iy, iz, color);

        if (iz < grid_size.z - 1)
        {
            if (grid[ix, iy, iz + 1].type == VoxelType.EMPTY)
                RenderBack(ix, iy, iz, color);
        }
        else
            RenderBack(ix, iy, iz, color);

        if (iz > 0)
        {
            if (grid[ix, iy, iz - 1].type == VoxelType.EMPTY)
                RenderFront(ix, iy, iz, color);
        }
        else
            RenderFront(ix, iy, iz, color);

    }
    private void RenderTop(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz + 1f));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 3);
    }
    private void RenderBottom(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz + 1f));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 3);
        indices.Add(vsize + 2);
    }
    private void RenderFront(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 3);
    }
    private void RenderBack(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz + 1f));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 3);
        indices.Add(vsize + 2);
    }
    private void RenderLeft(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix, iy + 1f, iz + 1f));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 3);
        indices.Add(vsize + 2);
    }
    private void RenderRight(int ix, int iy, int iz, Color color)
    {
        int vsize = vertices.Count;
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy, iz + 1f));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz));
        vertices.Add(DenseGridCoordToLocalCoord(ix + 1f, iy + 1f, iz + 1f));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        indices.Add(vsize + 0);
        indices.Add(vsize + 2);
        indices.Add(vsize + 1);
        indices.Add(vsize + 1);
        indices.Add(vsize + 2);
        indices.Add(vsize + 3);
    }
}