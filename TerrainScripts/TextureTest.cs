using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TextureTest : MonoBehaviour
{
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 3000; //defines the width of the terrain in meters
	private int terrainHeight = 100; //defines the maximum possible height of the terrain
	private int terrainLength = 3000; //defines the length of the terrain in meters
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

		tex = Resources.Load("InputPictureA") as Texture2D;

		if (tex != null) {
			print ("NULL");
		}
		else {
			print ("Worked");
		}
		if (tex.Equals(null)) {
			print ("NULL");
		}
		else {
			print ("Worked");
		}

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
		
	}

	private void createFloatMatrix ()
	{

		finalHeightMap = new float[length, width];

		
		for (int y = 0; y < length-1; y++) {
			print (tex.GetPixel(0, y).g);
			for (int x = 0; x < width-1; x++) {
				if(tex.GetPixel(x, y).g > 0.6){ //field
					finalHeightMap[y, x] = 0.5f;
				}else if(tex.GetPixel(x, y).r > 0.6){ //mountains
					finalHeightMap[y, x] = 0.9f;
				}else if(tex.GetPixel(x, y).b > 0.6){ //water 
					finalHeightMap[y, x] = 0.4f;
				}
				else{ //city
					finalHeightMap[y, x] = 0.1f;
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
