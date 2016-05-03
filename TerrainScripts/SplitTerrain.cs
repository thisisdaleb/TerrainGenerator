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

		//apparently TerrainData can't be held in arrays
		TerrainData terrainData1 = new TerrainData ();
		TerrainData terrainData2 = new TerrainData ();
		TerrainData terrainData3 = new TerrainData ();
		TerrainData terrainData4 = new TerrainData ();

		terrainData1.heightmapResolution = terrainData.heightmapResolution/2+1;
		terrainData1.baseMapResolution = terrainData.baseMapResolution/2;
		terrainData1.SetDetailResolution (terrainData.detailResolution / 2, 8);
		terrainData1.alphamapResolution = terrainData.alphamapResolution/2;
		terrainData1.size = new Vector3 (terrainData.size.x/2f, terrainData.size.y, terrainData.size.z/2f);
		print (terrainData.heightmapResolution + "  " 
			+ terrainData.baseMapResolution + "  " + 
			terrainData.alphamapResolution);
		print (terrainData1.heightmapResolution + "  " 
			+ terrainData1.baseMapResolution + "  " + 
			terrainData1.alphamapResolution);


		terrainData2.heightmapResolution = terrainData.heightmapResolution/2+1;
		terrainData2.baseMapResolution = terrainData.baseMapResolution/2;
		terrainData2.SetDetailResolution (terrainData.detailResolution / 2, 8);
		terrainData2.alphamapResolution = terrainData.alphamapResolution/2;
		terrainData2.size = new Vector3 (terrainData.size.x/2f, terrainData.size.y, terrainData.size.z/2f);

		terrainData3.heightmapResolution = terrainData.heightmapResolution/2+1;
		terrainData3.baseMapResolution = terrainData.baseMapResolution/2;
		terrainData3.SetDetailResolution (terrainData.detailResolution / 2, 8);
		terrainData3.alphamapResolution = terrainData.alphamapResolution/2;
		terrainData3.size = new Vector3 (terrainData.size.x/2f, terrainData.size.y, terrainData.size.z/2f);

		terrainData4.heightmapResolution = terrainData.heightmapResolution/2+1;
		terrainData4.baseMapResolution = terrainData.baseMapResolution/2;
		terrainData4.SetDetailResolution (terrainData.detailResolution / 2, 8);
		terrainData4.alphamapResolution = terrainData.alphamapResolution/2;
		terrainData4.size = new Vector3 (terrainData.size.x/2f, terrainData.size.y, terrainData.size.z/2f);
	
		float[,] heightmap1 = new float[terrainData1.heightmapResolution, terrainData1.heightmapResolution];
		float[,] heightmap2 = new float[terrainData2.heightmapResolution, terrainData2.heightmapResolution];
		float[,] heightmap3 = new float[terrainData3.heightmapResolution, terrainData3.heightmapResolution];
		float[,] heightmap4 = new float[terrainData4.heightmapResolution, terrainData4.heightmapResolution];


		for (int x = 0; x < terrainData.heightmapResolution; x++) {
			for (int y = 0; y < terrainData.heightmapResolution; y++) {
				// Normalise x/y coordinates to range 0-1 
				float y_01 = (float)y/(float)terrainData.alphamapHeight;
				float x_01 = (float)x/(float)terrainData.alphamapWidth;

				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
				float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );

				if (x > 1023 && y > 1023 && y < 1026 && x < 1026)
					print (x + "  " + y + "  " + height);
				if((x==0 && y==0) || (x== terrainData.heightmapResolution-1 && y == terrainData.heightmapResolution-1))
					print (x + "  " + y + "  " + height);
				if (x < terrainData.heightmapResolution / 2 + 1) {
					if (y < terrainData.heightmapResolution / 2 + 1) {
						heightmap1 [x, y] = height;
					}
					if (y >= terrainData.heightmapResolution / 2) {
						heightmap1 [x, y - terrainData.heightmapResolution / 2] = height;
					}
				}
				if (x >= terrainData.heightmapResolution / 2) {
					if (y < terrainData.heightmapResolution / 2 + 1) {
						heightmap1 [x - terrainData.heightmapResolution / 2, y] = height;
					}
					if (y >= terrainData.heightmapResolution / 2) {
						heightmap1 [x - terrainData.heightmapResolution / 2, y - terrainData.heightmapResolution / 2] = height;
					}

				}
			}
		}
		terrainData1.SetHeights (0, 0, heightmap1);
		terrainData2.SetHeights (0, 0, heightmap2);
		terrainData3.SetHeights (0, 0, heightmap3);
		terrainData4.SetHeights (0, 0, heightmap4);

		GameObject go1 = Terrain.CreateTerrainGameObject (terrainData1);
		GameObject go2 = Terrain.CreateTerrainGameObject (terrainData2);
		GameObject go3 = Terrain.CreateTerrainGameObject (terrainData3);
		GameObject go4 = Terrain.CreateTerrainGameObject (terrainData4);
		//go1.transform.position.Set (0, 0, 0);
		//go2.transform.position.Set (terrainData1.size.x, 0, 0);
		//go3.transform.position.Set (0, 0, terrainData1.size.z);
		//go4.transform.position.Set (terrainData1.size.x, 0, terrainData1.size.z);

	}
}
