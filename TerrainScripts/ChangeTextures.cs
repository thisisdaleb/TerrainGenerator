using UnityEngine;
using System.Collections;
public class ChangeTextures : MonoBehaviour
{
	public Terrain terrain;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//switch all painted in texture 1 to texture 2
			UpdateTerrainTexture(terrain.terrainData, 1, 2);
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			//switch all painted in texture 2 to texture 1
			UpdateTerrainTexture(terrain.terrainData, 2, 1);
		}
	}
	static void UpdateTerrainTexture(TerrainData terrainData, int textureNumberFrom, int textureNumberTo)
	{
		//get current paint mask
		float[, ,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
		// make sure every grid on the terrain is modified
		for (int i = 0; i < terrainData.alphamapWidth; i++)
		{
			for (int j = 0; j < terrainData.alphamapHeight; j++)
			{
				//for each point of mask do:
				//paint all from old texture to new texture (saving already painted in new texture)
				alphas[i, j, textureNumberTo] = Mathf.Max(alphas[i, j, textureNumberFrom], alphas[i, j, textureNumberTo]);
				//set old texture mask to zero
				alphas[i, j, textureNumberFrom] = 0f;
			}
		}
		// apply the new alpha
		terrainData.SetAlphamaps(0, 0, alphas);
	}
}