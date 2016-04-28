using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GenGuidedTerrainTextureTest : MonoBehaviour
{
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 14000; //defines the width of the terrain in meters
	private int terrainHeight = 2000; //defines the maximum possible height of the terrain
	private int terrainLength = 10000; //defines the length of the terrain in meters
	private int[,] colorMap;
	private Texture2D tex;
	private float[, ] pixelDistances;
	SplatPrototype[] terrainTexs;
	private Texture2D[] textureList;

	//important note:
	//boundary of map defined by:
	//!((k+y) < 0 || (k + y) > (length-1) || (z + x) < 0 || (z + x) > (width-1))
	
	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City 
	};
	
	// Use this for initialization
	void Start ()
	{
		length = width;
		runNow = true;
		
		colorMap = new int[width, length];
		
		pixelDistances = new float[width, length];
		
		tex = Resources.Load("InputPictureG") as Texture2D;

		textureList = new Texture2D[2];

		textureList[0] =  Resources.Load("GrassB") as Texture2D;

		textureList[1] =  Resources.Load("MountainTexture") as Texture2D;

		terrainTexs = new SplatPrototype [2];
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (runNow && extraRun) {
			convertInputIntoMap ();
		}
	}
	
	void convertInputIntoMap ()
	{
		print ("Start running processor melting program.");
		
		setColors ();
		
		setDistances ();
		
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix ();

		//Create terrain and send it through the world
		createTerrain ();
		
		runNow = false;
		extraRun = false;
	}
	
	void setColors ()
	{
		//GetPixel is not efficient. This method could run 100X faster if I replace that with GetPixels or GetPixels32 or whatever I need.
		
		int imageLoopX = tex.width;
		int imageLoopY = tex.height;
		
		int loopX = 0;
		int loopY = 0;
		
		int xPlaced = width / imageLoopX;
		int yPlaced = length / imageLoopY;
		
		int placeX = 0;
		int placeY = 0;
		
		print ("Values:  " + imageLoopX +  "  " + imageLoopY + "  " + loopX +  "  " + loopY + "  ");
		print ("Values:  " + xPlaced +  "  " + yPlaced + "  " + placeX +  "  " + placeY + "  ");
		
		while (loopY < imageLoopY) {
			while (loopX < imageLoopX) {
				while(placeY < yPlaced){
					while(placeX < xPlaced){
						if((yPlaced*loopY)+placeY < length && (xPlaced*loopX)+placeX < width){
							
							if(tex.GetPixel(loopX, loopY).g > 0.5)
							{ //field
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.Field;
								
							}
							else if(tex.GetPixel(loopX, loopY).r > 0.7)
							{ //mountains
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.Mountain;
								
							}
							else if(tex.GetPixel(loopX, loopY).b > 0.7)
							{ //water 
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.Water;
								
							}
							else
							{ //city
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.City;
							}
						}
						placeX++;
					}
					placeX=0;
					placeY++;
				}
				placeY=0;
				loopX++;
			}
			loopX=0;
			loopY++;
		}
	}
	
	private void setDistances ()
	{
		for(int y = 0; y < pixelDistances.GetLength(0); y++)
		{
			for(int x = 0; x < pixelDistances.GetLength(1); x++)
			{
				pixelTypeDistance(y, x, 0, -1, (int) ground.Mountain, true);
				pixelTypeDistance(y, x, 0, -1, (int) ground.Water, true);
			}
		}
		
		for(int y = pixelDistances.GetLength(0)-1; y >= 0; y--)
		{
			for(int x = pixelDistances.GetLength(1)-1; x >= 0; x--)
			{
				pixelTypeDistance(y, x, 0, 1, (int) ground.Mountain, false);
				pixelTypeDistance(y, x, 0, 1, (int) ground.Water, false);
			}
		}

		for(int y = 0; y < pixelDistances.GetLength(0); y++)
		{
			for(int x = 0; x < pixelDistances.GetLength(1); x++)
			{
				pixelTypeDistance(y, x, -1, 0, (int) ground.Mountain, false);
				pixelTypeDistance(y, x, -1, 0, (int) ground.Water, false);
			}
		}

		for(int y = pixelDistances.GetLength(0)-1; y >= 0; y--)
		{
			for(int x = pixelDistances.GetLength(1)-1; x >= 0; x--)
			{
				pixelTypeDistance(y, x, 1, 0, (int) ground.Mountain, false);
				pixelTypeDistance(y, x, 1, 0, (int) ground.Water, false);
			}
		}
	}
	
	private void pixelTypeDistance(int y, int x, int movingY, int movingX, int groundType, Boolean secondRun){
		if(colorMap[y, x]== groundType){
			if(y==0 || x==0 || y == pixelDistances.GetLength(0)-1 || x == pixelDistances.GetLength(1)-1){
				pixelDistances[y, x] = 1;
			}
			else if(colorMap[y+movingY, x+movingX] == (int) groundType){
				if(secondRun || pixelDistances[y, x] >= pixelDistances[y+movingY, x+movingX])
					pixelDistances[y, x] = pixelDistances[y+movingY, x+movingX]+1;
			}
			else{
				pixelDistances[y, x] = 1;
			}
		}
	}
	
	
	
	private void fieldDistance(){
		
	}
	
	private void createFloatMatrix ()
	{
		
		print ("Mountain Height check  " + pixelDistances[5, 5]);
		
		finalHeightMap = new float[length, width];
		
		for (int y = 0; y < length-1; y++) {
			
			for (int x = 0; x < width-1; x++) {
				
				if(colorMap[y, x] == (int) ground.Field){ //field
					finalHeightMap[y, x] = 0.0f;
					
				}else if(colorMap[y, x] == (int) ground.Mountain){ //mountains
					finalHeightMap[y, x] = 0.0f + (float)(pixelDistances[y, x])*0.02f;
					
				}else if(colorMap[y, x] == (int) ground.Water){ //water 
					finalHeightMap[y, x] = 0.0f - (float)(pixelDistances[y, x])*0.01f;
				}
				else{ //city
					finalHeightMap[y, x] = 0.0f;
				}
			}
		}
		setMin ();
	}
	
	private void setMin (){
		float min = 1f;
		float max = 0f;
		length = width = finalHeightMap.GetLength(0);
		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				if(finalHeightMap [y, x] < min){
					min = finalHeightMap [y, x];
				}
				if(finalHeightMap [y, x] > max){
					max = finalHeightMap [y, x];
				}
			}
		}
		
		min = Math.Abs (min); 
		
		terrainHeight = (int)((max+Math.Abs(min))*500f);
		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				finalHeightMap [y, x] = (finalHeightMap [y, x] + min) / (max + min);
			}
		}
	}

	private void createTextures ()
	{
		terrainTexs [0] = new SplatPrototype ();
		terrainTexs [0].texture = textureList [0];
		terrainTexs [0].tileSize = new Vector2 (5, 5);
		terrainTexs [1] = new SplatPrototype ();
		terrainTexs [1].texture = textureList [1];
		terrainTexs [1].tileSize = new Vector2 (5, 5);
	}
	
	private void placeTextures ()
	{

	}
	
	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		
		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = 1024;
		terrainData.SetDetailResolution (1024, 16);
		
		terrainData.SetHeights (0, 0, finalHeightMap);
		terrainData.size = new Vector3 (terrainWidth, terrainHeight, terrainLength);

		createTextures ();

		terrainData.splatPrototypes = terrainTexs;
		
		
		placeTextures ();
		//terrainData.treePrototypes = m_treeProtoTypes;
		//terrainData.detailPrototypes = m_detailProtoTypes;
		GameObject go = Terrain.CreateTerrainGameObject (terrainData);
		go.transform.position.Set (0, 0, 0);
		print ("It made it to the end");
	}
}
