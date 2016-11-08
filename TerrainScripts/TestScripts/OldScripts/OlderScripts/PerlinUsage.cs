using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PerlinUsage : MonoBehaviour
{
	bool runNow;
	bool extraRun = true;
	private int[,] initColorMap;
	private int width = 2049; //These 2 defined by input! Each terrain 4097 pixels wide and long
	private int length; //Input is amount of tiles in width and length (Ex: 2x3 tiles)
	private float[,] finalHeightMap; //defines the elevation of each height point between 0.0 and 1.0
	private int terrainWidth = 1000; //defines the width of the terrain in meters
	private int terrainHeight = 500; //defines the maximum possible height of the terrain
	private int terrainLength = 1000; //defines the length of the terrain in meters
	private SimplexNoiseGenerator simplex;
	private List<float[,]> noiseMat;
	private int[] matSizes;


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
		matSizes = new int[11];
		for (int mats = 0; mats < (matSizes.Length); mats++) {
			matSizes [mats] = 4 * ((int)Math.Pow (2, mats));
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
		print ("Begin system, but first yield once.");

		//yield return null;
		print ("Start running processor melting program.");

		////import picture,
		//createMatrix of colors

		//setColors();

		//testCreation();

		//create Simplex Noise matrix
		//createSimplexMatrix ();
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

		for (int noiseNumber= 0; noiseNumber<5; noiseNumber++) {
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
		float max = 0f;
		float min = 1f;
		finalHeightMap = new float[length, width];
		
		for (int y = 0; y < length; y++) {
			for (int x = 0; x < width; x++) {
				finalHeightMap [y, x] = Mathf.PerlinNoise(1/(y+1), 1/(x+1));
				if (finalHeightMap [y, x] > max)
					max = finalHeightMap [y, x];
				if (finalHeightMap [y, x] < min)
					min = finalHeightMap [y, x];
			}
		}

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

	//inputs A and B are the numbers that are being interpolated between.
	//X is the fraction of difference between A and B.
	private float SmoothInterpolate (float a, float b, float x)
	{
		float ft = x * 3.1415927f;
		float f = (float)(1 - Math.Cos (ft)) * 0.5f;

		return  (float)(a * (1 - f) + b * f);
	}

	private float Interpolate(float a, float b, float x){
		return a*(1f-x) + b*x;
	}


	/*Bilinear Interpolation
	 float fractionX = ... //the fraction part of the x coordinate
	float integerX = ... //the integer part of the x coordinate
	float fractionY, integerY = ...
	interpolatedValue = (1 - fractionX) * 
                        ((1 - fractionY) * data[integerX, integerY] + 
                         fractionY * data[integerX, integerY + 1]) + 
                    fractionX * 
                        ((1 - fractionY) * data[integerX + 1, integerY] + 
                        fractionY * data[integerX + 1, integerY + 1]);
*/

}
