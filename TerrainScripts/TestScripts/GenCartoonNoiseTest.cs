using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GenCartoonNoiseTest : MonoBehaviour
{
	public bool runNow;
	private int[,] initColorMap;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	public int terrainWidth = 10000; //defines the width of the terrain in meters
	private int terrainHeight = 2400; //defines the maximum possible height of the terrain
	public int terrainLength = 10000; //defines the length of the terrain in meters
	private int[,] colorMap;
	public Texture2D tex;
	private float[, ] pixelDistances;
	private Boolean[, ] fieldEdgeTypes;
	SplatPrototype[] terrainTexs;
	public Texture2D[] textureList;
	public int waterHeight = 400;
	public int fieldHeight = 100;
	public int mountainHeight = 2000;
	private SimplexNoiseGenerator simplex;
	private List<float[,]> noiseMat;
	private int[] matSizes;
	private float[, ] samples5;
	private float[, ] samples6;
	private float[, ] samples7;
	private float[, ] samples8;
	private float[, ] samples9;
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

		tex = Resources.Load ("InputPictureG") as Texture2D;

		textureList = new Texture2D[3];

		textureList [0] = Resources.Load ("GrassB") as Texture2D;

		textureList [1] = Resources.Load ("MountainTexture") as Texture2D;

		textureList [2] = Resources.Load ("Snow") as Texture2D;

		if(textureList.Length == 4)
			terrainTexs = new SplatPrototype [4];
		else	
			terrainTexs = new SplatPrototype [3];

		matSizes = new int[10];
		for (int mats = 0; mats < (matSizes.GetLength(0)); mats++) {
			matSizes [mats] = 4 * ((int)Math.Pow (2, mats));
		}
		samples5 = new float[width, width];
		samples6 = new float[width, width];
		samples7 = new float[width, width];
		samples8 = new float[width, width];
		samples9 = new float[width, width];
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (runNow) {
			convertInputIntoMap ();
		}
		else {
			refreshVariables();
		}
	}
	
	void convertInputIntoMap ()
	{
		print ("Start running processor melting program.");
		
		setColors ();
		
		setDistances ();

		createSimplexMatrix ();
		
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix ();

		//Create terrain and send it through the world
		createTerrain ();
		
		runNow = false;
		refreshVariables ();
	}

	private void createSimplexMatrix ()
	{
		noiseMat = new List<float[,]> ();

		for (int noiseNumber= 0; noiseNumber<10; noiseNumber++) {
			simplex = new SimplexNoiseGenerator ((long)(UnityEngine.Random.Range (int.MinValue, int.MaxValue)));
			noiseMat.Add (new float[matSizes [noiseNumber], matSizes [noiseNumber]]);
			for (int y = 0; y < matSizes[noiseNumber]; y++) {
				for (int x = 0; x < matSizes[noiseNumber]; x++) {
					noiseMat [noiseNumber] [y, x] = (float)(simplex.Evaluate (x, y));
				}
			}
		}
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
		
		print ("Values:  " + imageLoopX + "  " + imageLoopY + "  " + loopX + "  " + loopY + "  ");
		print ("Values:  " + xPlaced + "  " + yPlaced + "  " + placeX + "  " + placeY + "  ");
		
		while (loopY < imageLoopY) {
			while (loopX < imageLoopX) {
				while (placeY < yPlaced) {
					while (placeX < xPlaced) {
						if ((yPlaced * loopY) + placeY < length && (xPlaced * loopX) + placeX < width) {
							
							if (tex.GetPixel (loopX, loopY).g > 0.5) { //field
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Field;
								
							} else if (tex.GetPixel (loopX, loopY).r > 0.7) { //mountains
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Mountain;
							} else if (tex.GetPixel (loopX, loopY).b > 0.7) { //water 
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Water;
								
							} else { //city
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Field;
							}
						}
						placeX++;
					}
					placeX = 0;
					placeY++;
				}
				placeY = 0;
				loopX++;
			}
			loopX = 0;
			loopY++;
		}
	}
	
	private void setDistances ()
	{
		for (int y = 0; y < pixelDistances.GetLength(0); y++) {
			for (int x = 0; x < pixelDistances.GetLength(1); x++) {
				pixelTypeDistance (y, x, 0, -1, (int)ground.Mountain, true);
				pixelTypeDistance (y, x, 0, -1, (int)ground.Water, true);
				fieldDistance (y, x, 0, -1, true);
			}
		}
		
		for (int  y = 0; y < pixelDistances.GetLength(0); y++) {
			for (int x = pixelDistances.GetLength(1)-1; x >= 0; x--) {
				pixelTypeDistance (y, x, 0, 1, (int)ground.Mountain, false);
				pixelTypeDistance (y, x, 0, 1, (int)ground.Water, false);
				fieldDistance (y, x, 0, 1, false);
			}
		}
		
		for (int x = 0; x < pixelDistances.GetLength(1); x++) {
			for (int y = 0; y < pixelDistances.GetLength(0); y++) {
				pixelTypeDistance (y, x, -1, 0, (int)ground.Mountain, false);
				pixelTypeDistance (y, x, -1, 0, (int)ground.Water, false);
				fieldDistance (y, x, -1, 0, false);
			}
		}
		
		for (int x = 0; x < pixelDistances.GetLength(1); x++) {
			for (int y = pixelDistances.GetLength(0)-1; y >= 0; y--) {
				pixelTypeDistance (y, x, 1, 0, (int)ground.Mountain, false);
				pixelTypeDistance (y, x, 1, 0, (int)ground.Water, false);
				fieldDistance (y, x, 1, 0, false);
			}
		}

		for (int y = 0; y < pixelDistances.GetLength(0); y++) {
			for (int x = 0; x < pixelDistances.GetLength(1); x++) {
				pixelTypeDistance (y, x, 0, -1, (int)ground.Mountain, false);
				pixelTypeDistance (y, x, 0, -1, (int)ground.Water, false);
				fieldDistance (y, x, 0, -1,  false);
			}
		}

	}
	
	private void pixelTypeDistance (int y, int x, int movingY, int movingX, int groundType, Boolean firstRun)
	{
		if (colorMap [y, x] == groundType) {
			if (y == 0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) {
				pixelDistances [y, x] = 10;
			} else if (colorMap [y + movingY, x + movingX] == groundType) {
				if (firstRun || pixelDistances [y, x] > pixelDistances [y + movingY, x + movingX])
					pixelDistances [y, x] = pixelDistances [y + movingY, x + movingX] + 1;
			} else {
				pixelDistances [y, x] = 1;
			}
		}
	}

	private void fieldDistance (int y, int x, int movingY, int movingX, Boolean firstRun)
	{
		if (colorMap [y, x] == (int)ground.Field) {
			if (y == 0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) {
				pixelDistances [y, x] = 1;
				fieldEdgeTypes [y, x] = true; 
				//true = Mountain Edge
				//false = Water Edge
			} else if (colorMap [y + movingY, x + movingX] == (int)ground.Field) {
				if (firstRun || pixelDistances [y, x] > pixelDistances [y + movingY, x + movingX]) {
					pixelDistances [y, x] = pixelDistances [y + movingY, x + movingX] + 1;
					fieldEdgeTypes [y, x] = fieldEdgeTypes [y + movingY, x + movingX]; 
				}
			} else {
				pixelDistances [y, x] = 1;
				if (colorMap [y + movingY, x + movingX] == (int)ground.Mountain)
					fieldEdgeTypes [y, x] = true;
				else if (colorMap [y + movingY, x + movingX] == (int)ground.Water)
					fieldEdgeTypes [y, x] = false;
			}
		}
	}
	
	private void createFloatMatrix (){
		float[] persistence = new float[10]; 

		//Fraction of how much each level of noise is worth compared to the next level
		float perst = 0.5f; 


		finalHeightMap = new float[length, width];
		if (width > 4000) {
			samples5 = smartestInterpolation (noiseMat [5], 17 * 2);
			samples6 = smartestInterpolation (noiseMat [6], 9 * 2);
			samples7 = smartestInterpolation (noiseMat [7], 10);
			samples8 = smartestInterpolation (noiseMat [8], 5);
			samples9 = smartestInterpolation (noiseMat [9], 3);
		} else if (width > 2000) {
			samples5 = smartestInterpolation (noiseMat [5], 17);
			samples6 = smartestInterpolation (noiseMat [6], 9);
			samples7 = smartestInterpolation (noiseMat [7], 5);
			samples8 = smartestInterpolation (noiseMat [8], 3);
		} else {
			samples5 = smartestInterpolation (noiseMat [5], 10);
			samples6 = smartestInterpolation (noiseMat [6], 5);
			samples7 = smartestInterpolation (noiseMat [7], 3);
		}

		for (int z = 0; z < 8; z++) {
			persistence[z] = (float)(Math.Pow (perst, z));
		}

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
					finalHeightMap [y, x] = 0.0f + (float)(pixelDistances [y, x]) * 0.02f;
				} else { //city
					finalHeightMap [y, x] = 0.0f;
				}
			}
		}

		setMin ();
			
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				finalHeightMap [y, x] = finalHeightMap [y, x] + Math.Abs(addNoise(y, x, persistence)*0.2f);
			}
		}
	}

	private float addNoise (int y, int x, float[] persistence){
	return samples5[y, x]*persistence[5] +
		samples6[y, x]*persistence[6] + 
		samples7[y, x]*persistence[7] +
		samples8[y, x]*persistence[8]+
		samples9[y, x]*persistence[9];
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

		print ("bottom: " + waterSpace + " top: " + (fieldSpace+waterSpace));
		print ("Min: " + mountainMin + " Max: " + (mountainMax));

		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				if (colorMap [y, x] == (int)ground.Water) {
					finalHeightMap [y, x] = waterSpace - ((Math.Abs(finalHeightMap [y, x]) - waterMin) / (waterMax - waterMin))*waterSpace;
				}
				else if (colorMap [y, x] == (int)ground.Mountain) {
					finalHeightMap [y, x] = waterSpace + fieldSpace + ((Math.Abs(finalHeightMap [y, x]) - mountainMin) / (mountainMax - mountainMin))*mountainSpace;
				}
				else{
							finalHeightMap [y, x] = waterSpace + ((Math.Abs(finalHeightMap [y, x]) - fieldMin) / (fieldMax - fieldMin))*fieldSpace;
				}
			}
		}

		finalHeightMap = averagePixels (finalHeightMap);
	}
	
	private float[, ] averagePixels (float[, ] finalHeightMap){
		float[, ] newFinalHeightMap = new float[width, length];
		for (int y = 1; y < length-2; y++) {
			
			for (int x = 1; x < width-2; x++) {
				if (pixelDistances [y, x] > 1)
					newFinalHeightMap [y, x] = finalHeightMap [y, x] / 2 +
					(
					    finalHeightMap [y - 1, x] +
					    finalHeightMap [y + 1, x] +
					    finalHeightMap [y - 1, x - 1] +
					    finalHeightMap [y, x - 1] +
					    finalHeightMap [y + 1, x - 1] +
					    finalHeightMap [y - 1, x + 1] +
					    finalHeightMap [y, x + 1] +
					    finalHeightMap [y + 1, x + 1]
					) / 16f;
				else
					newFinalHeightMap [y, x] = finalHeightMap [y, x];
			}
		}
		return newFinalHeightMap;
	}



	private float[, ] smartestInterpolation(float[, ] oldMat, int expand){
		int newXY = expand*oldMat.GetLength(0)-expand+1;
		float[, ] newMat = new float[newXY, newXY];
		int multiplier = (int) ((newXY-1)/(oldMat.GetLength(0)-1));
		int x1;
		int y1;
		int x2;
		int y2;
		float Q1;
		float Q2;
		float Q3;
		float Q4;

		for(int y=0; y< newMat.GetLength(0)-1; y++){

			for(int x = 0; x < newMat.GetLength(0)-1; x++){
				if(y%multiplier==0 && x%multiplier==0){
					newMat[y, x] = oldMat[y/multiplier, x/multiplier];
				}
				else{
					x1 = x/multiplier*multiplier;
					x2 = x1+multiplier;
					y1 = y/multiplier*multiplier;
					y2 = y1+multiplier;
					Q1 = oldMat[y/multiplier, x/multiplier];
					Q2 = oldMat[y/multiplier, x/multiplier+1];
					Q3 = oldMat[y/multiplier+1, x/multiplier];
					Q4 = oldMat[y/multiplier+1, x/multiplier+1];

					newMat[y, x] = fixedCalc((float)(x-x1)/(x2-x1), (float)(y-y1)/(y2-y1), Q1, Q2, Q3, Q4);
				}
			}
		}
		int newY = newMat.GetLength(0)-1;
		for(int x = 0; x < newMat.GetLength(0)-1; x++){
			if(x%multiplier==0){
				newMat[newY, x] = oldMat[newY/multiplier, x/multiplier];
			}
			else{
				x1 = x/multiplier*multiplier;
				x2 = x1+multiplier;
				newMat[newY, x] = interpolate(oldMat[newY/multiplier, x1/multiplier], oldMat[newY/multiplier, x2/multiplier], (float)(x-x1)/(x2-x1));
			}
		}

		int newX = newMat.GetLength(0)-1;
		for(int y = 0; y < newMat.GetLength(0); y++){
			if(y%multiplier==0){
				newMat[y, newX] = oldMat[y/multiplier, newX/multiplier];
			}
			else{
				y1 = y/multiplier*multiplier;
				y2 = y1+multiplier;
				newMat[y, newX] = interpolate(oldMat[y1/multiplier, newX/multiplier], oldMat[y2/multiplier, newX/multiplier], (float)(y-y1)/(y2-y1));
			}
		}

		return newMat;
	}


	//Q1 = top left Number
	//Q2 = top right number
	//Q3 = bottom left number
	//Q4 = bottom right number
	//fractionX and fractionY are the fraction/percentage of how far from top left to bottom right they are for each coordinate 
	private float fixedCalc(float fractionX, float fractionY, float Q1, float Q2, float Q3, float Q4){
		return (1 - fractionX) * ((1 - fractionY) * Q1 + fractionY * Q3) + fractionX * ((1 - fractionY) * Q2 + fractionY * Q4);
	}


	private void createTextures ()
	{
		terrainTexs [0] = new SplatPrototype ();
		terrainTexs [0].texture = textureList [0];
		terrainTexs [0].tileSize = new Vector2 (15, 15);
		terrainTexs [1] = new SplatPrototype ();
		terrainTexs [1].texture = textureList [1];
		terrainTexs [1].tileSize = new Vector2 (15, 15);
		terrainTexs [2] = new SplatPrototype ();
		terrainTexs [2].texture = textureList [2];
		terrainTexs [2].tileSize = new Vector2 (15, 15);
		if (textureList.Length > 3) {
			terrainTexs [3] = new SplatPrototype ();
			terrainTexs [3].texture = textureList [3];
			terrainTexs [3].tileSize = new Vector2 (15, 15);
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
	
	private void createTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		
		terrainData.heightmapResolution = width;
		terrainData.baseMapResolution = 2048;
		terrainData.SetDetailResolution (2048, 16);
		terrainData.alphamapResolution = 2048;
		
		terrainData.SetHeights (0, 0, finalHeightMap);
		terrainData.size = new Vector3 (terrainWidth, terrainHeight, terrainLength);

		createTextures ();

		terrainData.splatPrototypes = terrainTexs;
		
		placeTextures (terrainData);
		//terrainData.treePrototypes = m_treeProtoTypes;
		//terrainData.detailPrototypes = m_detailProtoTypes;
		GameObject go = Terrain.CreateTerrainGameObject (terrainData);
		go.transform.position.Set (0, 0, 0);
		print ("It made it to the end");
	}

	private float smoothInterpolate (float a, float b, float x)
	{
		float ft = x * 3.1415927f;
		float f = (float)(1 - Math.Cos (ft)) * 0.5f;
		
		return  (float)(a * (1 - f) + b * f);
	}
		
	private float interpolate(float a, float b, float x){
		return a*(1f-x) + b*x;
	}
}
