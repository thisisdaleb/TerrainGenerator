using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TextureTest : MonoBehaviour
{
	bool runNow;
	bool extraRun = true;
	private int[,] colorMap;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 14000; //defines the width of the terrain in meters
	private int terrainHeight = 500; //defines the maximum possible height of the terrain
	private int terrainLength = 10000; //defines the length of the terrain in meters
	private Texture2D tex;
	
	//important note:
	//boundary of map defined by:
	//!((k+y) < 0 || (k + y) > (length-1) || (z + x) < 0 || (z + x) > (width-1))
	
	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City }
	;
	
	// Use this for initialization
	void Start ()
	{
		length = width;
		runNow = true;

		colorMap = new int[width, length];

		tex = Resources.Load("InputPictureE") as Texture2D;
	}
	
	// Update is called once per frame
	void Update ()
	{
		print ("start:" + runNow + extraRun);
		if (runNow && extraRun) {
			convertInputIntoMap ();
		}
		print ("end:" + runNow + extraRun);
	}
	
	void convertInputIntoMap ()
	{
		print ("Start running processor melting program.");
		
		setColors();

		createFloatMatrix ();

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

							if(tex.GetPixel(loopX, loopY).g > 0.8)
							{ //field
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.Field;

							}
							else if(tex.GetPixel(loopX, loopY).r > 0.8)
							{ //mountains
								colorMap[(yPlaced*loopY)+placeY, (xPlaced*loopX)+placeX] = (int) ground.Mountain;

							}
							else if(tex.GetPixel(loopX, loopY).b > 0.8)
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

	private void createFloatMatrix ()
	{

		finalHeightMap = new float[length, width];

		
		for (int y = 0; y < length-1; y++) {

			for (int x = 0; x < width-1; x++) {

				if(colorMap[y, x] == (int) ground.Field){ //field
					finalHeightMap[y, x] = 0.52f;

				}else if(colorMap[y, x] == (int) ground.Mountain){ //mountains
					finalHeightMap[y, x] = 0.9f;

				}else if(colorMap[y, x] == (int) ground.Water){ //water 
					finalHeightMap[y, x] = 0.1f;

				}
				else{ //city
					finalHeightMap[y, x] = 0.5f;
				}
			}
		}

		//MAKES TERRAIN STRETCH FROM 0 TO 1
		float min = 1f;
		float max = 0f;
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
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				finalHeightMap [y, x] = (finalHeightMap [y, x] + min) / (max + min);
			}
		}
	}
	
	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		
		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = 1024;
		terrainData.SetDetailResolution (1024, 16);
		
		terrainData.SetHeights (0, 0, finalHeightMap);
		terrainData.size = new Vector3 (terrainWidth, terrainHeight, terrainLength);
		//terrainData.splatPrototypes = m_splatPrototypes;
		//terrainData.treePrototypes = m_treeProtoTypes;
		//terrainData.detailPrototypes = m_detailProtoTypes;
		GameObject go = Terrain.CreateTerrainGameObject (terrainData);
		go.transform.position.Set (0, 0, 0);
		print ("It made it to the end");
	}
}
