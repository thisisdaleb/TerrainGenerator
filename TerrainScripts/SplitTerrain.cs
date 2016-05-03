using UnityEngine;
using System.Collections;
using System.Linq; // used for Sum of array

[ExecuteInEditMode]
public class SplitTerrain : MonoBehaviour {
	
	void Start () {
		// Get the attached terrain component
		Terrain terrain = GetComponent<Terrain>();
		
		// Get a reference to the terrain data
		TerrainData terrainData = terrain.terrainData;
		

	}
}