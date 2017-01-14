using UnityEngine;
using System.Collections;
using System.Linq;

 // used for Sum of array

public class GrassCreator
{
	
	public void startGrassPlacing (TerrainData terrainData, int[,] colorMap, int grassDensity, int patchDetail, float waterTop, float fieldTop, int cityGround)
	{
		terrainData.SetDetailResolution(grassDensity, patchDetail);

		int[,] map = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, 0);

		Vector3 terrainDataSize = terrainData.size;

		for (int i = 0; i < grassDensity; i++)
		{
			for (int j = 0; j < grassDensity; j++)
			{
				// Normalise x/y coordinates to range 0-1 
				float y_01 = (float)j/(float)terrainData.alphamapHeight;
				float x_01 = (float)i/(float)terrainData.alphamapWidth;

				// Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
				float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight),Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) ) / terrainDataSize.y;

				// Calculate the steepness of the terrain
				float steepness = terrainData.GetSteepness(y_01,x_01);

				if (height > waterTop+0.0001f && height < fieldTop && steepness < 24f && colorMap[i*2, j*2] != cityGround) {
					map [i, j] = 16;
				}
			}
		}
		terrainData.SetDetailLayer(0, 0, 0, map);
	}
}
