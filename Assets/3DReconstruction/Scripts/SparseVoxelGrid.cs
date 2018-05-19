using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* DESCRIPTION
*   Infinite Voxel Grid: 
*       Origin: Should be exactly at (0,0,0) of the global coordinate system (vertex of 8 neighbouring cubes).
*       Orientation: Along the coordinate axes of the global coordinate system
*
* REFERENCES: 
*   This algorithm will be developed based on the paper:
*   "Real-time 3D Reconstruction at Scale using Voxel Hashing" 
* 
* OTHER SCRIPTS:
*   Interface.cs: Supplies all the sensor data.
*   VoxelGrid.cs: Defines Methods, which are required by VoxelGeneration.
*   VoxelGeneration.cs: Integrates DepthMap + DifferenceMap into existing VoxelGrid.
*/

// DATA STRUCTURES.
// Value for the hash table (key->value).
public struct HashtableValue
{
    // Voxel Block (basically, just a smaller voxel grid).
    public DenseVoxelGrid voxel_block;
    // Center of Voxel Block in global coordinates (== Key to the hash table).
    // Assume the center to only consist of integer values.
    public Vector3Int voxel_block_center;
    // Closest SDF value voxel block to zero (required if we want to check if voxel block shall be deleted).
    public float smallest_abs_sdf_value;
}

public class SparseVoxelGrid : MonoBehaviour {

    // Hash table. Key: Center of voxel block which contains a point. Value: hashtable_value.
    private Hashtable voxelblock_hash_table = new Hashtable();

    //Prefab to DenseVoxelGrid
    [Header("Prefab of DenseGrid Object")]
    public GameObject dense_voxel_grid;

    //Side Length in [m] of a single dense cube
    [Header("Side Length in [m] of a DenseGrid")]
    public float grid_size;

    //Radius of the generated blocks around a block. If r=1, a 3x3x3 grid gets generated
    [Header("Radius of Generated Grids around Target")]
    public int generation_radius;

    [Header("How many cells are stored in the Hashmap?")]
    [ReadOnly]
    public int active_cells;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        transform.localScale = new Vector3(grid_size, grid_size, grid_size);
        active_cells = voxelblock_hash_table.Count;
	}

    //IN: World coordinates
    //OUT: Dense voxel grid at this location. If no grid is located, create new grids and return this one
    public DenseVoxelGrid GetDenseVoxelGrid(Vector3 position)
    {
        Vector3Int key = WorldCoordsToSparseGridCoords(position);
        if (!voxelblock_hash_table.Contains(ToKey(key)))
            GenerateDenseVoxelGrids(key);
        GameObject g = voxelblock_hash_table[ToKey(key)] as GameObject;
        return g.GetComponent<DenseVoxelGrid>();
    }

    public Hashtable GetVoxelGrids()
    {
        return voxelblock_hash_table;
    }

    //IN: Grid Position
    //OUT: Encoded Key for Hashmap
    public string ToKey(Vector3Int position)
    {
        return position.x + "_" + position.y + "_" + position.z;
    }

    //Creates VoxelGrids and saves them in the HashMap.
    //Always creates a grid with size (block_generator_radius*2+1)^3
    public void GenerateDenseVoxelGrids(Vector3Int gridcoords)
    {
        for(int z = gridcoords.z - generation_radius; z <= gridcoords.z + generation_radius; z++)
        {
            for(int y = gridcoords.y - generation_radius; y <= gridcoords.y + generation_radius; y++)
            {
                for(int x = gridcoords.x - generation_radius; x <= gridcoords.x + generation_radius; x++)
                {
                    Vector3Int position = new Vector3Int(x, y, z);
                    if (!voxelblock_hash_table.ContainsKey(ToKey(position)))
                    {
                        GameObject g = Instantiate(dense_voxel_grid);
                        g.transform.parent = this.transform;
                        g.transform.localScale = new Vector3(1, 1, 1);
                        g.transform.localRotation = Quaternion.identity;
                        g.transform.localPosition = new Vector3(x, y, z);
                        g.name = "Dense_" + x + "_" + y + "_" + z;
                        DenseVoxelGrid dvg = g.GetComponent<DenseVoxelGrid>();
                        dvg.Render();
                        voxelblock_hash_table.Add(ToKey(position), g);
                    }
                }
            }
        }
    }

    public void GenerateDenseVoxelGrids(Vector3 worldcoords)
    {
        Vector3Int gridcoords = WorldCoordsToSparseGridCoords(worldcoords);
        GenerateDenseVoxelGrids(gridcoords);
    }

    //Does not have to be called every time, can also run in background
    //Removes all voxel Grids that do not contain any active voxels
    public void RemoveEmptyVoxelGrids()
    {
        List<string> toRemove = new List<string>();
        foreach(DictionaryEntry de in voxelblock_hash_table)
        {
            GameObject g = de.Value as GameObject;
            DenseVoxelGrid dvg = g.GetComponent<DenseVoxelGrid>();
            if (dvg.active_count == 0)
            {
                toRemove.Add(de.Key as string);
                Destroy(g);
            }
        }
        foreach(string s in toRemove)
            voxelblock_hash_table.Remove(s);
    }

    //I: coordinates in world-frame
    //O: key to dense voxel grid that contains I
    public Vector3Int WorldCoordsToSparseGridCoords(Vector3 worldcoords)
    {
        Vector3 pos = transform.InverseTransformPoint(worldcoords);
        return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

}
