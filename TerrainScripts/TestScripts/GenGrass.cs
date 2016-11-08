using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GenGrass : MonoBehaviour
{
	public bool runNow;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	public int terrainWidth = 7000; //defines the width of the terrain in meters
	private int terrainHeight = 2400; //defines the maximum possible height of the terrain
	public int terrainLength = 5000; //defines the length of the terrain in meters
	private int[,] colorMap;
	public Texture2D tex;
	private float[, ] pixelDistances;
	private Boolean[, ] fieldEdgeTypes;
	SplatPrototype[] terrainTexs;
	public Texture2D[] textureList;
	public int waterHeight = 350;
	public int fieldHeight = 40;
	public int mountainHeight = 1500;
	private float waterSpace;
	private float fieldSpace;

	//important note:
	//boundary of map defined by:
	//!((k+y) < 0 || (k + y) > (length-1) || (z + x) < 0 || (z + x) > (width-1))
	
	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City 
	}
	;
	
	// Use this for initialization
	void Start ()
	{
		refreshVariables ();
	}

	void refreshVariables ()
	{
		length = width;
		runNow = false;

		colorMap = new int[width, length];

		pixelDistances = new float[width, length];

		fieldEdgeTypes = new Boolean[width, length];

		//tex = Resources.Load ("InputPictureG") as Texture2D;

		textureList = new Texture2D[3];

		textureList [0] = Resources.Load ("GrassB") as Texture2D;

		textureList [1] = Resources.Load ("MountainTexture") as Texture2D;

		textureList [2] = Resources.Load ("Snow") as Texture2D;

		if(textureList.Length == 4)
			terrainTexs = new SplatPrototype [4];
		else	
			terrainTexs = new SplatPrototype [3];
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (runNow && !(tex == null)) {
			convertInputIntoMap ();
		}
		else {
			refreshVariables();
		}
	}
	
	void convertInputIntoMap ()
	{
		print ("Start running processor melting program.");
		
		ImageDistances setImage = new ImageDistances ();
		setImage.setColors (tex, width, length, pixelDistances, colorMap, fieldEdgeTypes);

		pixelDistances [0, 0] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, 0] = 0;
		pixelDistances [0, pixelDistances.GetLength (0) - 1] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, pixelDistances.GetLength (0) - 1] = 0;
		
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix ();

		//Create terrain and send it through the world
		createTerrain ();
		
		runNow = false;
		refreshVariables ();
	}
	
	private void createFloatMatrix ()
	{
		
		finalHeightMap = new float[length, width];
		
		for (int y = 0; y < length-1; y++) {
			
			for (int x = 0; x < width-1; x++) {
				
				if (colorMap [y, x] == (int)ground.Field) { //field
					if (fieldEdgeTypes [y, x] == true) {
						if (pixelDistances [y, x] < 51)
							finalHeightMap [y, x] = 0.6f + smoothInterpolate (50f, 0f, pixelDistances [y, x] / 50f) /50f * 0.4f;
							//the last decimal number is how much of the field height this should take up.
						else
							finalHeightMap [y, x] = 0.6f;
							//this needs to be equal to that last number
					} else {
						if (pixelDistances [y, x] < 101) 
							finalHeightMap [y, x] = 0.0f + smoothInterpolate (0f, 100f, pixelDistances [y, x] / 100f) / 100f *0.6f;
						else
							finalHeightMap [y, x] = 0.6f;
					}
				} else if (colorMap [y, x] == (int)ground.Mountain) { //mountains
					finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x]-1) * 0.02f;
					
				} else if (colorMap [y, x] == (int)ground.Water) { //water 
					finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x]-1) * 0.02f;
				} else { //city
					finalHeightMap [y, x] = 0.0f;
				}
			}
		}
		setMin ();
	}
	
	private void setMin ()
	{
		float fieldMin = 20f;
		float fieldMax = -20f;
		float mountainMin = 20f;
		float mountainMax = -20f;
		float waterMin = 20f;
		float waterMax = -20f;
		length = width = finalHeightMap.GetLength (0);
		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				if (colorMap [y, x] == (int)ground.Water) {
					if (finalHeightMap [y, x] < waterMin) {
						waterMin = finalHeightMap [y, x];
					}
					if (finalHeightMap [y, x] > waterMax) {
						waterMax = finalHeightMap [y, x];
					}
				}
				else if (colorMap [y, x] == (int)ground.Mountain) {
					if (finalHeightMap [y, x] < mountainMin) {
						mountainMin = finalHeightMap [y, x];
					}
					if (finalHeightMap [y, x] > mountainMax) {
						mountainMax = finalHeightMap [y, x];
					}
				}
				else{
					if (finalHeightMap [y, x] < fieldMin) {
						fieldMin = finalHeightMap [y, x];
					}
					if (finalHeightMap [y, x] > fieldMax) {
						fieldMax = finalHeightMap [y, x];
					}
				}
			}
		}

		waterMin = Math.Abs (waterMin); 
		mountainMin = Math.Abs (mountainMin); 
		fieldMin = Math.Abs (fieldMin); 
		
		terrainHeight = waterHeight + mountainHeight + fieldHeight;

		waterSpace = (float)(waterHeight) / (float)(terrainHeight);
		float mountainSpace = (float)(mountainHeight) / (float)(terrainHeight);
		fieldSpace = (float)(fieldHeight) / (float)(terrainHeight);
		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				if (colorMap [y, x] == (int)ground.Water) {
					finalHeightMap [y, x] = waterSpace - ((finalHeightMap [y, x] + waterMin) / (waterMax + waterMin))*waterSpace;
				}
				else if (colorMap [y, x] == (int)ground.Mountain) {
					finalHeightMap [y, x] = waterSpace + fieldSpace + ((finalHeightMap [y, x] + mountainMin) / (mountainMax + mountainMin))*mountainSpace;
				}
				else{
					finalHeightMap [y, x] = waterSpace + ((finalHeightMap [y, x] + fieldMin) / (fieldMax + fieldMin))*fieldSpace;
				}
			}
		}

		finalHeightMap = averagePixels (finalHeightMap);

		addNoise ();
	}
	
	private float[, ] averagePixels (float[, ] finalHeightMap){
		float[, ] newFinalHeightMap = new float[width, length];
		for (int y = 1; y < length-2; y++) {
			
			for (int x = 1; x < width-2; x++) {
				newFinalHeightMap [y, x] = finalHeightMap [y, x]/2 +
					(
						finalHeightMap [y-1, x]+
						finalHeightMap [y+1, x]+
						finalHeightMap [y-1, x-1]+
						finalHeightMap [y, x-1]+
						finalHeightMap [y+1, x-1]+
						finalHeightMap [y-1, x+1]+
						finalHeightMap [y, x+1]+
						finalHeightMap [y+1, x+1]
						)/16f;
			}
		}
		return newFinalHeightMap;
	}

	private void addNoise ()
	{
		System.Random rand = new System.Random ();
		for (int y = 1; y < length - 2; y++) {

			for (int x = 1; x < width - 2; x++) {
				if(colorMap[y, x] == (int) ground.Mountain && pixelDistances[y, x] > 1)
					finalHeightMap [y, x] += (float) (rand.NextDouble()*0.002f);
				else if (colorMap[y, x] == (int) ground.Field && (fieldEdgeTypes[y, x] || pixelDistances[y, x] > 2))
					finalHeightMap [y, x] += (float) (rand.NextDouble()*0.0003f);
			}
		
		}
	}

	private void createTextures ()
	{
		for (int k = 0; k < textureList.Length; k++) {
			terrainTexs [k] = new SplatPrototype ();
			terrainTexs [k].texture = textureList [k];
			terrainTexs [k].tileSize = new Vector2 (10, 10);
		}
	}

	private void placeTextures (TerrainData terrainData)
	{
		SplatMapCreator spatMapper = new SplatMapCreator ();
		if(terrainTexs.Length>3)
			spatMapper.startTerrainPlacing (terrainData, true, waterSpace, (waterSpace+fieldSpace), colorMap, (int)ground.City);
		else
			spatMapper.startTerrainPlacing (terrainData, false, waterSpace, (waterSpace+fieldSpace), colorMap, (int)ground.City);
		//GameObject go = (GameObject)Instantiate(Resources.Load("WATERTIME"));
		//go.transform.position = (new Vector3 (4000f, (float) waterHeight - 3f, 4000f));
		//go.transform.localScale = new Vector3 (5000f, 0.001f, 5000f);
	}

	private void createGrass (TerrainData terrainData)
	{
		Texture2D grassTex = Resources.Load ("Grass4") as Texture2D;

		DetailPrototype[] details = new DetailPrototype[1];
		details [0] = new DetailPrototype ();
		details [0].renderMode = DetailRenderMode.GrassBillboard;
		details [0].prototypeTexture = grassTex;
		details [0].maxWidth = 4f;
		details [0].minWidth = 3.5f;
		terrainData.detailPrototypes = details;
	}

	private void placeGrass (TerrainData terrainData)
	{
		GrassCreator grassMap = new GrassCreator ();
		grassMap.startGrassPlacing (terrainData, colorMap, 2048, 8, waterSpace, (waterSpace+fieldSpace), (int) ground.City);
	}
	
	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		
		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = 2048;
		terrainData.alphamapResolution = 2048;
		
		terrainData.SetHeights (0, 0, finalHeightMap);
		terrainData.size = new Vector3 (terrainWidth, terrainHeight, terrainLength);

		createTextures ();

		terrainData.splatPrototypes = terrainTexs;
		
		placeTextures (terrainData);

		createGrass (terrainData);

		placeGrass (terrainData);

		//terrainData.treePrototypes = m_treeProtoTypes;
		GameObject go = Terrain.CreateTerrainGameObject (terrainData);
		go.GetComponent<Terrain> ().detailObjectDistance = 200;
		go.GetComponent<Terrain> ().detailObjectDensity = 1F;
		go.transform.position.Set (0, 0, 0);
		print ("It made it to the end");
	}

	private float smoothInterpolate (float a, float b, float x)
	{
		float ft = x * 3.1415927f;
		float f = (float)(1 - Math.Cos (ft)) * 0.5f;
		
		return  (float)(a * (1 - f) + b * f);
	}
}
