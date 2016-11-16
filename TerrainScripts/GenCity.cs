using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GenCity : MonoBehaviour
{
	//THIS IS THE CURRENT MAIN PROGRAM FOR GENERATING WORLDS BY INPUT

	private int width = 4097; 			//These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length;
	private float[,] finalHeightMap;	//defines the elevation of each height point between 0.0 and 1.0
	private int terrainHeight = 2400;	//defines the maximum possible height of the terrain
	private int[,] colorMap;			//the terrain type of each pixel
	private float[, ] pixelDistances;	//the distance from each pixel to a pixel of a different color
	private Boolean[, ] fieldEdgeTypes;	//tells field pixels whether they are closest to mountains or water
	SplatPrototype[] terrainTexs;		//set of textures used by map
	private float waterSpace;			//percentage of map height used by water
	private float fieldSpace;			//percentage of map height used by field

	//public values that users can edit in GUI.
	[Tooltip("Checking this box starts the system")]
	public bool runNow;
	[Tooltip("The width in meters of the terrain")]
	public int terrainWidth = 7000; //defines the width of the terrain in meters
	[Tooltip("The length in meters of the terrain")]
	public int terrainLength = 5000; //defines the length of the terrain in meters
	[Tooltip("The image used for making the world")]
	public Texture2D tex;
	[Tooltip("Textures placed on the terrain")]
	public Texture2D[] textureList; //These textures accessable in the GUI are added to terrainTexs
	[Tooltip("height of water section in meters")]
	public int waterHeight = 350;
	[Tooltip("height of field section in meters")]
	public int fieldHeight = 40;
	[Tooltip("height of mountain section in meters")]
	public int mountainHeight = 1500;
	[Tooltip("height of top half of field section in meters, unused currently")]
	public float topHalfField = 20f;
	[Tooltip("height of bottom half field section in meters, unused currently")]
	public float bottomHalfField = 20f;
	[Tooltip("How many pixels out does the top half of fields use?")]
	public float topFieldLength = 50f;
	[Tooltip("How many pixels out does the bottom half of fields use?")]
	public float bottomFieldLength = 50f;
	[Tooltip("Detail resolution of the map")]
	public int grassSize = 2048;
	[Tooltip("Image that can be used for City Creation")]
	public Texture2D cityTex;


	//defines the types of ground states that each point on the map can be
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

	//re-initialized all variables
	//allows the program to be run multile times from the same script
	void refreshVariables ()
	{
		length = width;
		runNow = false;

		colorMap = new int[width, length];

		pixelDistances = new float[width, length];

		fieldEdgeTypes = new Boolean[width, length];

		cityTex = Resources.Load ("EserimCities") as Texture2D;

		textureList = new Texture2D[3];

		textureList [0] = Resources.Load ("GrassB") as Texture2D;

		textureList [1] = Resources.Load ("MountainTexture") as Texture2D;

		textureList [2] = Resources.Load ("Snow") as Texture2D;

		terrainTexs = new SplatPrototype [textureList.Length];
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if the run button has been checked and there is a texture loaded 
		if (runNow && tex != null) {
			convertInputIntoMap ();
			refreshVariables();
		}
	}
	
	void convertInputIntoMap ()
	{
		//
		//yPlaced = length / tex.height
		//while (loopY < imageLoopY) {
		//while (placeY < yPlaced) {
		//(yPlaced * loopY) + placeY
		print ("4097 Start running processor melting program. " + ( ((length/tex.height) * (tex.height-1)) + (length/tex.height) - 1)   );
		runNow = false;

		//creates matrix of all field types
		//creates matrix of distances from each pixel to another pixel of a different field type
		//marks down which field type that pixel is closest to, and the number itself
		ImageDistances setImage = new ImageDistances ();
		setImage.setColors (tex, width, length, pixelDistances, colorMap, fieldEdgeTypes);

		//sets all the corner pixels to 0 to fix issue where corners are large spikes.
		pixelDistances [0, 0] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, 0] = 0;
		pixelDistances [0, pixelDistances.GetLength (0) - 1] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, pixelDistances.GetLength (0) - 1] = 0;
		
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix ();

		//if the city texture exists, use it to find all buildings on map and place them into the world
		if(cityTex != null)
			addBuildings ();

		//Create terrain and send it through the world
		createTerrain ();
	}
	
	private void createFloatMatrix ()
	{
		
		finalHeightMap = new float[length, width];
		
		for (int y = 0; y < length-1; y++) {
			
			for (int x = 0; x < width-1; x++) {
				
				if (colorMap [y, x] == (int)ground.Mountain) { //mountains
					finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x]-1) * 0.02f;
					
				} else if (colorMap [y, x] == (int)ground.Water) { //water 
					finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x]-1) * 0.02f;
				} 
				//CITIES AND FIELDS
				else { 
					if (colorMap [y, x] == (int)ground.City && pixelDistances [y, x] > 6)
						fieldEdgeTypes[y, x] = false;
					if (fieldEdgeTypes [y, x] == true) {
						if (pixelDistances [y, x] < topFieldLength + 1)
							finalHeightMap [y, x] = 0.6f + smoothInterpolate (topFieldLength, 0f, pixelDistances [y, x] / topFieldLength) /topFieldLength * 0.4f;
						//the last decimal number is how much of the field height this should take up.
						else
							finalHeightMap [y, x] = 0.6f;
						//this needs to be equal to that last number
					} else {
						if (pixelDistances [y, x] < bottomFieldLength+1) 
							finalHeightMap [y, x] = 0.0f + smoothInterpolate (0f, 
								bottomFieldLength, pixelDistances [y, x] / bottomFieldLength) / bottomFieldLength *0.6f;
						else
							finalHeightMap [y, x] = 0.6f;
					}
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
				if (colorMap [y, x] == (int)ground.Mountain && pixelDistances [y, x] > 1)
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.002f);
				else if (colorMap [y, x] == (int)ground.Field && (fieldEdgeTypes [y, x] || pixelDistances [y, x] > 2)) 
				{
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.0003f);
				}
				else if (colorMap [y, x] == (int)ground.City)
				{
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.0002f);
				}
			}
		}
	}

	private void addBuildings()
	{
		CityTextureCreator setCity = new CityTextureCreator ();
		setCity.setBuildings (cityTex, width, length, finalHeightMap);
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
		details [0].healthyColor = details [0].dryColor = new Color (0, 233, 158);
		terrainData.detailPrototypes = details;
	}

	private void placeGrass (TerrainData terrainData)
	{
		GrassCreator grassMap = new GrassCreator ();
		grassMap.startGrassPlacing (terrainData, colorMap, grassSize, 8, waterSpace, (waterSpace+fieldSpace), (int) ground.City);
	}
	
	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		
		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = width-1;
		terrainData.alphamapResolution = width-1;
		
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
