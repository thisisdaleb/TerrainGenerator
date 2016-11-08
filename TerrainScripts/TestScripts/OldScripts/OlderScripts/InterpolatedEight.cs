using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InterpolatedEight : MonoBehaviour
{
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 4097; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 30000; //defines the width of the terrain in meters
	private int terrainHeight = 2750; //defines the maximum possible height of the terrain
	private int terrainLength = 30000; //defines the length of the terrain in meters
	private SimplexNoiseGenerator simplex;
	private List<float[,]> noiseMat;
	private int[] matSizes;
	private float[, ] samples0;
	private float[, ] samples1;
	private float[, ] samples2;
	private float[, ] samples3;
	private float[, ] samples4;
	private float[, ] samples5;
	private float[, ] samples6;
	private float[, ] samples7;
	
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
		matSizes = new int[8];
		for (int mats = 0; mats < (matSizes.GetLength(0)); mats++) {
			matSizes [mats] = 4 * ((int)Math.Pow (2, mats));
		}

		samples2 = new float[width, width];
		samples3 = new float[width, width];
		samples4 = new float[width, width];
		samples5 = new float[width, width];
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
		print ("Begin system, but first yield once.");
		
		//yield return null;
		print ("Start running processor melting program.");
		
		////import picture,
		//createMatrix of colors
		
		//setColors();
		
		//testCreation();
		
		//create Simplex Noise matrix
		createSimplexMatrix ();
		//add simplex noise to grass, water, and mountains
		//makes sure areas near cities have a gentle enough slope to leave
		//add complex Simplex Noise for mountains
		
		//create matrix of floats, set to the integer matrix where the minimum
		//integer value is normalized to 0.0f and the maximum value is at 1.0f
		createFloatMatrix ();
		//++++TestSetFloatMatrix
		//testFloatMatrix();
		
		//Create terrain and send it through the world
		createTerrain ();
		
		runNow = false;
		extraRun = false;
	}
	
	void setColors ()
	{
		
	}
	
	//##
	//START OF TEST METHODS
	//##
	private void testCreation ()
	{
		//Not how it will work, this is just a test method, remember!
		initColorMap = new int[length, width];
		for (int k = 0; k<length; k++) {
			for (int z = 0; z < width; z++) {
				if (k < length / 5 * 2) {
					initColorMap [k, z] = (int)ground.Mountain;
				} else if (k < length / 5 * 3) {
					initColorMap [k, z] = (int)ground.Field;
				} else if (k < length / 5 * 4) {
					initColorMap [k, z] = (int)ground.City;
				} else {
					initColorMap [k, z] = (int)ground.Water;
				}
			}
		}
	}
	
	//##
	//END OF TEST METHODS
	//##
	
	
	/// <summary>
	/// ######################
	/// ######################
	/// BEGIN SECTION DEVOTED TO NOISE
	/// ######################
	/// ######################
	/// </summary>
	
	private void createSimplexMatrix ()
	{
		noiseMat = new List<float[,]> ();
		
		for (int noiseNumber= 0; noiseNumber<8; noiseNumber++) {
			simplex = new SimplexNoiseGenerator ((long)(UnityEngine.Random.Range (int.MinValue, int.MaxValue)));
			noiseMat.Add (new float[matSizes [noiseNumber], matSizes [noiseNumber]]);
			for (int y = 0; y < matSizes[noiseNumber]; y++) {
				for (int x = 0; x < matSizes[noiseNumber]; x++) {
					noiseMat [noiseNumber] [y, x] = (float)(simplex.Evaluate (x, y));
				}
			}
		}
	}
	
	private void createFloatMatrix ()
	{
		finalHeightMap = new float[length, width];
		
		samples0 = smartestInterpolation (noiseMat[0], 820*2);
		samples1 = smartestInterpolation (noiseMat[1], 310*2);
		samples2 = smartestInterpolation (noiseMat[2], 142*2);
		samples3 = smartestInterpolation (noiseMat[3], 67*2);
		samples4 = smartestInterpolation (noiseMat[4], 34*2);
		samples5 = smartestInterpolation (noiseMat[5], 17*2);
		samples6 = smartestInterpolation (noiseMat[6], 9*2);
		samples7 = smartestInterpolation (noiseMat[7], 5*2);
		print (samples0.GetLength(0));
		print (samples1.GetLength(0));
		print (samples2.GetLength(0));
		print (samples3.GetLength(0));
		print (samples4.GetLength(0));
		print (samples5.GetLength(0));
		print (samples6.GetLength(0));
		print (samples7.GetLength(0));

		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				finalHeightMap[y, x] += samples0[y, x];
				finalHeightMap[y, x] += samples1[y, x]*1/2f;
				finalHeightMap[y, x] += samples2[y, x]*1/4f;
				finalHeightMap[y, x] += samples3[y, x]*1/8f;
				finalHeightMap[y, x] += samples4[y, x]*1/16f;
				finalHeightMap[y, x] += samples5[y, x]*1/32f;
				finalHeightMap[y, x] += samples6[y, x]*1/64f;
				finalHeightMap[y, x] += samples7[y, x]*1/128f;
			}
		}
				
		//
		//Set Min and Max, make map be all 1
		//
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
		
		for (int y = 0; y < length-1; y++) {
			for (int x = 0; x < width-1; x++) {
				finalHeightMap [y, x] = (finalHeightMap [y, x] + min) / (max + min);
			}
		}
		//
		//Set Min and Max, make map be all 1
		//
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
	
	//inputs A and B are the numbers that are being interpolated between.
	//X is the fraction of difference between A and B.
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
