using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class TerrainGenerateEasyInitialTest : MonoBehaviour {
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 4097; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length = 4097; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 1000; //defines the width of the terrain in meters
	private int terrainHeight = 500; //defines the maximum possible height of the terrain
	private int terrainLength = 1000; //defines the length of the terrain in meters
	
	//important note:
	//boundary of map defined by:
	//!((k+y) < 0 || (k + y) > (length-1) || (z + x) < 0 || (z + x) > (width-1))
	
	enum ground : int { Field, Mountain, Water, City };
	
	// Use this for initialization
	void Start () {
		runNow = true;
	}
	
	// Update is called once per frame
	void Update () {
		print("start:" + runNow + extraRun);
		if (runNow && extraRun)
		{
			convertInputIntoMap();
		}
		print("end:" + runNow + extraRun);
	}
	
	void convertInputIntoMap()
	{
		print("Begin system, but first yield once.");
		
		//yield return null;
		print("Start running processor melting program.");
		
		runNow = false;
		extraRun = false;
		
		////import picture,
		//createMatrix of colors
		setColors();
		//++++TestCreation
		testCreation();

		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix();
		//++++TestSetFloatMatrix
		testFloatMatrix();
		
		//Create terrain and send it through the world
		createTerrain();
	}
	
	void setColors ()
	{
		//throw new NotImplementedException ();
	}

	//##
	//START OF TEST METHODS
	//##
	private void testCreation()
	{
		//Not how it will work, this is just a test method, remember!
		initColorMap = new int[length, width];
		for (int k = 0; k<length;k++)
		{
			for (int z = 0; z < width; z++)
			{
				if (k < length/5*2)
				{
					initColorMap[k,z] = (int) ground.Mountain;
				}
				else if (k < length/5*3)
				{
					initColorMap[k, z] = (int) ground.Field;
				}
				else if (k < length/5*4)
				{
					initColorMap[k, z] = (int) ground.City;
				}
				else
				{
					initColorMap[k, z] = (int) ground.Water;
				}
			}
		}
	}
	
	private void testFloatMatrix()
	{
		for (int k = 0; k < length; k++)
		{
			for (int z = 0; z < width; z++)
			{
				if (initColorMap[k, z] == 0)
				{
					finalHeightMap[k, z] = 0.4f;
				}
				else if (initColorMap[k, z] == 1)
				{
					finalHeightMap[k, z] = 0.8f;
				}
				else if (initColorMap[k, z] == 2)
				{
					finalHeightMap[k, z] = 0.1f;
				}
				else if (initColorMap[k, z] == 3)
				{
					finalHeightMap[k, z] = 0.2f;
				}
			}
		}
	}
	
	private void createFloatMatrix()
	{
		finalHeightMap = new float[4097, 4097];
		for (int y = 0; y < length; y++)
		{
			for (int x = 0; x < width; x++)
			{
				//finalHeightMap[y, x] = (float)((heightMap[y, x]+minValue)/(maxValue+Math.Abs(minValue)));
			}
		}
	}
	
	private void createTerrain()
	{
		TerrainData terrainData = new TerrainData();
		
		terrainData.heightmapResolution = 4097;
		terrainData.baseMapResolution = 1024;
		terrainData.SetDetailResolution(1024, 16);
		
		terrainData.SetHeights(0, 0, finalHeightMap);
		terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
		//terrainData.splatPrototypes = m_splatPrototypes;
		//terrainData.treePrototypes = m_treeProtoTypes;
		//terrainData.detailPrototypes = m_detailProtoTypes;
		GameObject go = Terrain.CreateTerrainGameObject(terrainData);
		go.transform.position.Set(0,0,0);
		print("It made it to the end");
	}
	
}