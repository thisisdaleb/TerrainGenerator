using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GuidedProceduralGenerator : MonoBehaviour
{
	//THIS IS THE CURRENT MAIN PROGRAM FOR GENERATING WORLDS BY INPUT IMAGE

	private int width = 2049;				//These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length;
	private float[,] finalHeightMap;		//defines the elevation of each height point between 0.0 and 1.0
	private int terrainHeight = 2400;		//defines the maximum possible height of the terrain
	private int[,] colorMap;				//the terrain type of each pixel
	private float[, ] pixelDistances;		//the distance from each pixel to a pixel of a different color
	private int[, ] fieldEdgeTypes;		//tells field pixels whether they are closest to mountains or water
	private SplatPrototype[] terrainTexs;	//set of textures used by map
	private float waterSpace;				//percentage of map height used by water
	private float fieldSpace;				//percentage of map height used by field
	private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch ();


	//public values that users can edit in GUI.
	[Tooltip ("Checking this box starts the system")]
	public bool runNow;
	/*[Tooltip ("Checking this box uses euclidean distance instead of manhatten or chess")]
	public bool Euclidean;*/
	[Tooltip ("Checking this box uses chessboard distance instead of manhatten, but does not overried Euclidean box")]
	public bool Chess;
	[Tooltip ("The width in meters of the terrain")]
	public int terrainWidthInMeters = 5000;
	[Tooltip ("The length in meters of the terrain")]
	public int terrainLengthInMeters = 5000;
	[Tooltip ("The image used for making the world")]
	public Texture2D tex;
	[Tooltip ("Textures placed on the terrain")]
	public Texture2D[] textureList;
	[Tooltip ("height of water section in meters")]
	public int waterHeight = 350;
	[Tooltip ("height of field section in meters")]
	public int fieldHeight = 40;
	[Tooltip ("height of mountain section in meters")]
	public int mountainHeight = 1500;
	[Tooltip ("How many pixels out does the top half of fields use?")]
	public float topFieldLength = 75f;
	[Tooltip ("How many pixels out does the bottom half of fields use?")]
	public float bottomFieldLength = 100f;
	[Tooltip ("Detail resolution of the map (ie amount of grass)")]
	public int detailResolution = 2048;


	//defines the types of ground states that each point on the map can be
	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City
	};

	void Start ()
	{
		refreshVariables ();
	}

	//allows the program to be run multiple times from the same script
	void refreshVariables ()
	{
		runNow = false;

		length = width;
		colorMap = new int[width, length];
		pixelDistances = new float[width, length];
		fieldEdgeTypes = new int[width, length];
		finalHeightMap = new float[length, width];

		textureList = new Texture2D[3];
		textureList [0] = Resources.Load ("GrassB") as Texture2D;
		textureList [1] = Resources.Load ("MountainTexture") as Texture2D;
		textureList [2] = Resources.Load ("Snow") as Texture2D;
		terrainTexs = new SplatPrototype [textureList.Length];
	}

	// Update is called once per frame
	void Update ()
	{
		//if the run button has been checked and there is a map image loaded 
		if (runNow && tex != null) {
			timer.Start ();
			convertInputIntoMap ();
			print ("Time: " + timer.ElapsedMilliseconds);
			timer.Reset ();
			print ("It made it to the end");
			refreshVariables ();
		}
	}

	void convertInputIntoMap ()
	{
		runNow = false;

		//creates matrix of all field types
		//creates matrix of distances from each pixel to another pixel of a different field type
		//marks down which field type that pixel is closest to, and the number itself
		ImageDistances setImage = new ImageDistances ();
		setImage.setColors (tex, width, length, pixelDistances, colorMap);
		/*if(Euclidean)
			setImage.setDistancesDiag (pixelDistances, colorMap, fieldEdgeTypes);
		else*/
		if(Chess)
			setImage.setDistancesChess (pixelDistances, colorMap, fieldEdgeTypes);
		else
			setImage.setDistances (pixelDistances, colorMap, fieldEdgeTypes);
		setImage.removeCornerPillars (pixelDistances);

		//Now that we know the distances to a different field type for each pixel,
		//we can finish up by creating the height map matrix
		createFloatMatrix ();
		setAllHeightPointsToBeBetweenZeroAndOne ();
		for (int smooth = 1; smooth < 3; smooth++) {
			smoothHeightMapPixels (smooth);
		}
		//smoothHeightMapPixels (1);
		addNoise ();
		addNoise ();
		createTerrain ();
	}

	private void createFloatMatrix ()
	{
		for (int y = 0; y < length - 1; y++) {
			for (int x = 0; x < width - 1; x++) {
				createInitialHeightMapMatrixValues (y, x);
			}
		}
	}

	private void createInitialHeightMapMatrixValues (int y, int x)
	{
		if (colorMap [y, x] == (int)ground.Mountain) {
			finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x] - 1) * 0.02f;

		} else if (colorMap [y, x] == (int)ground.Water) {
			finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x] - 1) * 0.02f;
		} 
		//CITIES AND FIELDS
		else { 
			//if city, make it so it pretends that area is near water.
			if (colorMap [y, x] == (int)ground.City && pixelDistances [y, x] > 6)
				fieldEdgeTypes [y, x] = (int)ground.Water;
			if (fieldEdgeTypes [y, x] == (int)ground.Mountain) {
				if (pixelDistances [y, x] < topFieldLength + 1)
					finalHeightMap [y, x] = 0.6f + smoothInterpolate (topFieldLength, 0f, pixelDistances [y, x] / topFieldLength) / topFieldLength * 0.4f;
				//the last decimal number is how much of the field height this should take up.
				else
					finalHeightMap [y, x] = 0.6f;
				//this needs to be equal to that last number
			} else {
				if (pixelDistances [y, x] < bottomFieldLength + 1)
					finalHeightMap [y, x] = 0.0f + smoothInterpolate (0f, 
						bottomFieldLength, pixelDistances [y, x] / bottomFieldLength) / bottomFieldLength * 0.6f;
				else
					finalHeightMap [y, x] = 0.6f;
			}
		}
	}

	private void setAllHeightPointsToBeBetweenZeroAndOne ()
	{
		float fieldMin = 20f;
		float fieldMax = -20f;
		float mountainMin = 20f;
		float mountainMax = -20f;
		float waterMin = 20f;
		float waterMax = -20f;
		length = width = finalHeightMap.GetLength (0);

		for (int y = 0; y < length - 1; y++) {
			for (int x = 0; x < width - 1; x++) {
				if (colorMap [y, x] == (int)ground.Water) {
					setGroundMinMax (ref waterMin, ref waterMax, y, x);
				} else if (colorMap [y, x] == (int)ground.Mountain) {
					setGroundMinMax (ref mountainMin, ref mountainMax, y, x);
				} else {
					setGroundMinMax (ref fieldMin, ref fieldMax, y, x);
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

		for (int y = 0; y < length - 1; y++) {
			for (int x = 0; x < width - 1; x++) {
				if (colorMap [y, x] == (int)ground.Water) {
					finalHeightMap [y, x] = waterSpace - ((finalHeightMap [y, x] + waterMin) / (waterMax + waterMin)) * waterSpace;
				} else if (colorMap [y, x] == (int)ground.Mountain) {
					finalHeightMap [y, x] = waterSpace + fieldSpace + ((finalHeightMap [y, x] + mountainMin) / (mountainMax + mountainMin)) * mountainSpace;
				} else {
					finalHeightMap [y, x] = waterSpace + ((finalHeightMap [y, x] + fieldMin) / (fieldMax + fieldMin)) * fieldSpace;
				}
			}
		}
	}

	private void setGroundMinMax(ref float groundMin, ref float groundMax, int y, int x){
		if (finalHeightMap [y, x] < groundMin) {
			groundMin = finalHeightMap [y, x];
		}
		if (finalHeightMap [y, x] > groundMax) {
			groundMax = finalHeightMap [y, x];
		}
	}



	private void smoothHeightMapPixels (int k)
	{
		float[, ] newFinalHeightMap = new float[width, length];
		for (int y = k; y < length - 2 - k; y++) {
			for (int x = k; x < width - 2 - k; x++) {
				newFinalHeightMap [y, x] = averagePixelCalculation (y, x, k);
			}
		}
		finalHeightMap = newFinalHeightMap;
	}

	private float averagePixelCalculation (int y, int x, int k)
	{
		float averagePixel = 0;
		averagePixel += finalHeightMap [y, x] * 0.25f;
		averagePixel +=	
			(
		    finalHeightMap [y - k, x] +
		    finalHeightMap [y + k, x] +
		    finalHeightMap [y - k, x - k] +
		    finalHeightMap [y, x - k] +
		    finalHeightMap [y + k, x - k] +
		    finalHeightMap [y - k, x + k] +
		    finalHeightMap [y, x + k] +
		    finalHeightMap [y + k, x + k]
		) / 8f * 0.75f;
		return averagePixel;
	}

	private void addNoise ()
	{
		System.Random rand = new System.Random ();
		for (int y = 1; y < length - 2; y++) {
			for (int x = 1; x < width - 2; x++) {
				if (colorMap [y, x] == (int)ground.Mountain && pixelDistances [y, x] > 1)
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.001f);
				else if (colorMap [y, x] == (int)ground.Field && (fieldEdgeTypes [y, x]== (int)ground.Mountain && pixelDistances [y, x] > 2)) {
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.0003f);
				} else if (colorMap [y, x] == (int)ground.City) {
					finalHeightMap [y, x] += (float)(rand.NextDouble () * 0.0002f);
				}
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
		if (terrainTexs.Length > 3)
			spatMapper.startTerrainPlacing (terrainData, true, waterSpace, (waterSpace + fieldSpace), colorMap, (int)ground.City);
		else
			spatMapper.startTerrainPlacing (terrainData, false, waterSpace, (waterSpace + fieldSpace), colorMap, (int)ground.City);
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
		grassMap.startGrassPlacing (terrainData, colorMap, detailResolution, 8, waterSpace, (waterSpace + fieldSpace), (int)ground.City);
	}

	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();

		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = width - 1;
		terrainData.alphamapResolution = width - 1;

		terrainData.SetHeights (0, 0, finalHeightMap);
		terrainData.size = new Vector3 (terrainWidthInMeters, terrainHeight, terrainLengthInMeters);

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
	}

	private float smoothInterpolate (float a, float b, float x)
	{
		float ft = x * 3.1415927f;
		float f = (float)(1 - Math.Cos (ft)) * 0.5f;

		return  (float)(a * (1 - f) + b * f);
	}
}
