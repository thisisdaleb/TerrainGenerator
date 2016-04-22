using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class TerrainEditor : MonoBehaviour {
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 129; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length = 129; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private int[,] colorMap; //defines each height point to be mountain, field, city, or water
	private int[,] minDistMountainHeight; //defines minimum distance of mountain height point to base of mountain
	private int[,] minDistWaterDepth; //defines minimum distance of water height point to shoreline
	private int[,] minDistFromMountainEdge; //define distance to mountain base for surrounding pixels
	private int[,] minDistFromCityEdge; //defines distance to city edge for surrounding pixels
	private int[,] heightMap; //defines the elevation of each height point in integer values
	private int maxValue = 0; //defines the highest point in the int heightMap
	private int minValue = 0; //defines the lowest point in the int heightMap
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
		
		//create larger matrix of colors and integers
		expandColors();
		//Loop through colorMap to define edge distances
		StartCoroutine(LoopForEdgeDistances());
		
		//create matrix of integers that define heights of world
		//also loops through to call the other methods
		StartCoroutine(createIntegerHeightMatrix());
		
		//create Simplex Noise matrix
		createSimplexMatrix();
		//add simplex noise to grass, water, and mountains
		//makes sure areas near cities have a gentle enough slope to leave
		addSimplexMatrix();
		//add complex Simplex Noise for mountains
		addMountainSimplexNoise();
		
		//set sides of all terrains to match up with touching terrains to avoid holes
		//in the world
		averageSides();
		//defines Min and Max values
		defineMinAndMax();
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix();
		//++++TestSetFloatMatrix
		//testFloatMatrix();
		
		//Create terrain and send it through the world
		createTerrain();
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
		colorMap = initColorMap;
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
	//##
	//END OF TEST METHODS
	//##
	
	private void setColors()
	{
		
	}
	
	private void expandColors()
	{
		setVariablesForExpandedMatrix();
	}
	
	private void setVariablesForExpandedMatrix()
	{
		minDistMountainHeight = new int[length, width];
		minDistWaterDepth = new int[length, width];
		minDistFromMountainEdge = new int[length, width];
		minDistFromCityEdge = new int[length, width];
		
		for (int k = 0; k < length; k++)
		{
			for (int z = 0; z < width; z++)
			{
				//break;
				//for finding height and depth of mountains and water
				minDistMountainHeight[k, z] = -1;
				minDistWaterDepth[k, z] = -1;
				//for finding how close a pixel is to the edge of a mountain or city
				minDistFromMountainEdge[k, z] = -1;
				minDistFromCityEdge[k, z] = -1;
				//large number very slightly more optimized than using -1 as infinity
			}
		}
	}
	
	private IEnumerator LoopForEdgeDistances()
	{
		print("Start Loop for distances!");
		for (int k = 0; k < length; k++)
		{
			print("Row completed");
			for (int z = 0; z < width; z++)
			{
				print("pixel");
				if (colorMap[k, z] == (int) ground.Mountain)
				{
					defineMountainData(k, z);
				}
				
				if (colorMap[k, z] == (int) ground.City)
				{
					//set integer distance to city edge of surrounding pixels
					defineDistanceToCityEdge(k, z);
				}
				
				if (colorMap[k, z] == (int) ground.Water)
				{
					//set integer distance to water edge of water pixels
					defineWaterEdgeDistance(k, z);
				}
				yield return null;
			}
		}
		for (int k = 0; k < length; k++)
		{
			for (int z = 0; z < width; z++)
			{
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						//set integer distance to mountain edge of mountain pixels secondaryCheck=true
						defineMountainHeight(true, k, z, y, x);
						yield return null;
					}
				}
			}
		}
	}
	
	private void defineMountainData(int k, int z)
	{
		//circle around the pixel, checking each pixel to see what happens
		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				if (!(y == 0 && x == 0))
				{
					if (!((k + y) < 0 && (k + y) > (length - 1) && (z + x) < 0 && (z + x) > (width - 1))) //if not out of bounds
					{
						if (colorMap[k, z] != (int)ground.Mountain) //if it isn't mountain
						{
							//says the minimum
							minDistMountainHeight[k, z] = 0;
							//set integer distance to mountain edge of surrounding pixels
							defineDistanceToMountainEdge(k, z, y, x);
						}
						else //if it is mountain
						{
							if (minDistMountainHeight[k, z] != 0)
							{
								minDistFromMountainEdge[k, z] = 1;
								//set integer distance to mountain edge of mountain pixels secondaryCheck=false
								defineMountainHeight(false, k, z, y, x);
							}
						}
					}
				}
			}
		}
	}
	
	private void defineDistanceToMountainEdge(int y, int x, int dirY, int dirX)
	{
		int currentMinDistance = 1;
		int moveY = y + dirY;
		int moveX = x + dirX;
		while (!(moveY < 0 && moveY > (length - 1) && moveX < 0 && moveX > (width - 1)))
		{
			//moves checking pointer foward one step
			moveY += dirY;
			moveX += dirX;
			if (!(moveY < 0 && moveY > (length - 1) && moveX < 0 && moveX > (width - 1)))
			{
				if(currentMinDistance < (terrainWidth/100))
				{
					if (minDistFromMountainEdge[y, x] > currentMinDistance)
					{
						minDistFromMountainEdge[y, x] = currentMinDistance;
					}
					break;
				}
			}
			currentMinDistance++;
		}
	}
	
	private void defineMountainHeight(bool secondRun, int y, int x, int dirY, int dirX)
	{
		int currentMinDistance = 1;
		int moveY = y + dirY;
		int moveX = x + dirX;
		while(!(moveY < 0 && moveY > (length - 1) && moveX < 0 && moveX > (width - 1)))
		{
			//moves checking pointer foward one step
			moveY+=dirY;
			moveX+=dirX;
			if (!(moveY < 0 || moveY > (length - 1) || moveX < 0 || moveX > (width - 1)))
			{
				if(colorMap[moveY, moveX] != (int) ground.Mountain)
				{
					if(minDistMountainHeight[y, x] > currentMinDistance)
					{
						minDistMountainHeight[y, x] = currentMinDistance;
					}
					break;
				}
				else
				{
					
					currentMinDistance++;
				}
			}
			else
			{
				currentMinDistance += 5;
				if (minDistMountainHeight[y, x] > currentMinDistance)
				{
					minDistMountainHeight[y, x] = currentMinDistance;
				}
			}
		}
	}
	
	private void defineWaterEdgeDistance(int y, int x)
	{
		//loop in a circle around the
		for (int loopY = -1; loopY <= 1; loopY++)
		{
			for (int loopX = -1; loopX <= 1; loopX++)
			{
				if (!((y + loopY) < 0 || (y + loopY) > (length - 1) || (x + loopX) < 0 || (x + loopX) > (width - 1))) //if not out of bounds
				{
					//not 0 and 0
					if (!(y == 0 && x == 0))
					{
						minimumDistanceWaterEdge(y, x, loopY, loopX);
					}
				}
			}
		}
	}
	
	private void minimumDistanceWaterEdge(int y, int x, int dirY, int dirX)
	{
		int currentMinDistance = 1;
		int moveY = y + dirY;
		int moveX = x + dirX;
		while (!(moveY < 0 || moveY > (length - 1) || moveX < 0 || moveX > (width - 1)))
		{
			//moves checking pointer foward one step
			moveY += dirY;
			moveX += dirX;
			if (!(moveY < 0 || moveY > (length - 1) || moveX < 0 || moveX > (width - 1)))
			{
				if (colorMap[moveY, moveX] != (int) ground.Water)
				{
					if (minDistWaterDepth[y, x] > currentMinDistance)
					{
						minDistWaterDepth[y, x] = currentMinDistance;
					}
					break;
				}
				else
				{
					currentMinDistance++;
				}
			}
			else
			{
				currentMinDistance += 5;
				if (minDistMountainHeight[y, x] > currentMinDistance)
				{
					minDistMountainHeight[y, x] = currentMinDistance;
				}
			}
		}
	}
	
	private void defineDistanceToCityEdge(int y, int x)
	{
		//loop in a circle around the
		for (int loopY = -1; loopY <= 1; loopY++)
		{
			for (int loopX = -1; loopX <= 1; loopX++)
			{
				if (!((y + loopY) < 0 || (y + loopY) > (length - 1) || (x + loopX) < 0 || (x + loopX) > (width - 1))) //if not out of bounds
				{
					//not 0 and 0
					if (!(y == 0 && x == 0))
					{
						minimumDistanceCityEdge(y, x, loopY, loopX);
					}
				}
			}
		}
	}
	
	private void minimumDistanceCityEdge(int y, int x, int dirY, int dirX)
	{
		int currentMinDistance = 1;
		int moveY = y + dirY;
		int moveX = x + dirX;
		while (!(moveY < 0 || moveY > (length - 1) || moveX < 0 || moveX > (width - 1)))
		{
			//moves checking pointer foward one step
			moveY += dirY;
			moveX += dirX;
			if (!(moveY < 0 || moveY > (length - 1) || moveX < 0 || moveX > (width - 1)))
			{
				if (currentMinDistance < (terrainWidth / 100))
				{
					if (minDistFromCityEdge[y, x] > currentMinDistance)
					{
						minDistFromCityEdge[y, x] = currentMinDistance;
					}
					break;
				}
			}
			currentMinDistance++;
		}
	}
	
	/// <summary>
	/// ######################
	/// ######################
	/// BEGIN SECTION DEVOTED TO THE INTEGER MATRIX INSTEAD OF COLOR MATRIX
	/// ######################
	/// ######################
	/// </summary>
	
	//No longer talking about creating 
	private IEnumerator createIntegerHeightMatrix()
	{
		heightMap = new int[length, width];
		for(int y = 0; y < length; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if(colorMap[y, x] == (int) ground.Water)
				{
					
					//set water heights
					setWaterDepth(y, x);
				}
				if (colorMap[y, x] == (int) ground.Mountain)
				{
					//set mountain heights
					setMountainHeight(y, x);
				}
				//raise heights of mountain pixels and pixels surrounding mountains
				//mountains raised by set amount, then further away you get, the less it is raised, up to a set distance
				raisePixelsNearMountains(y, x);

				yield return null;
			}
		}
	}
	
	private void setWaterDepth(int y, int x)
	{
		if (minDistWaterDepth[y, x] != 0)
		{
			heightMap[y, x] -= 50*minDistWaterDepth[y, x];
		}
	}
	
	
	private void setMountainHeight(int y, int x)
	{
		if (minDistMountainHeight[y, x] != 0)
		{
			heightMap[y, x] -= 50 * minDistMountainHeight[y, x];
		}
	}
	
	private void raisePixelsNearMountains(int y, int x)
	{
		if(minDistFromMountainEdge[y, x] != 0)
		{
			heightMap[y, x] += (terrainWidth/1000)/minDistFromMountainEdge[y, x];
		}
	}
	
	
	/// <summary>
	/// ######################
	/// ######################
	/// BEGIN SECTION DEVOTED TO NOISE
	/// ######################
	/// ######################
	/// </summary>
	
	private void createSimplexMatrix()
	{
		
	}
	
	private void addSimplexMatrix()
	{
		
	}
	
	private void addMountainSimplexNoise()
	{
		
	}
	
	private void averageSides()
	{
		
	}
	
	private void defineMinAndMax()
	{
		for (int y = 0; y < length; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if(heightMap[y, x] < minValue)
				{
					minValue = heightMap[y, x];
				}
				if (heightMap[y, x] > maxValue)
				{
					maxValue = heightMap[y, x];
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
				finalHeightMap[y, x] = (float)((heightMap[y, x]+minValue)/(maxValue+Math.Abs(minValue)));
			}
		}
	}
	
	private void createTerrain()
	{
		TerrainData terrainData = new TerrainData();
		
		terrainData.heightmapResolution = 129;
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