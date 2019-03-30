using UnityEngine;
using System.Collections;

public class ImageDistances
{

	//int[,] xChange, yChange;

	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City}
	;

	public void setColors (Texture2D tex, int width, int length, float[,] pixelDistances, int[,] colorMap)
	{

		//xChange = yChange = new int[colorMap.GetLength(0), colorMap.GetLength(1)];
		//Will use to calculate perfect distances. Everytime a pixel is one to the left, set xChange to the comparing pixel's
		// xChange - 1. if to the right, +1. If comparing pixel above, -1 to yChange, comparing pixel below, +1.
		//Then you calculate pythagorean theorem for every single cell.

		int imageLoopX = tex.width;
		int imageLoopY = tex.height;

		int loopX = 0;
		int loopY = 0;

		int xPlaced = width / imageLoopX;
		int yPlaced = length / imageLoopY;

		int placeX = 0;
		int placeY = 0;

		//1D array of colors of all pixels of texture
		Color[] texColor1D = tex.GetPixels ();
		//matrix of colors of texture
		Color[,] texColors = new Color[imageLoopX, imageLoopY];

		//loops horizontally and vertically through texture
		for (int Cy = 0; Cy < imageLoopY; Cy++) {
			for (int Cx = 0; Cx < imageLoopX; Cx++) {
				//pulls 1D array into matrix
				texColors [Cx, Cy] = texColor1D [(Cy * imageLoopX) + Cx];
			}
		}

		while (loopY < imageLoopY) {
			while (loopX < imageLoopX) {
				while (placeY < yPlaced) {
					while (placeX < xPlaced) {
						//if calculation is in the bounds of the image
						if (((yPlaced * loopY) + placeY) < length && ((xPlaced * loopX) + placeX) < width) {
							if (texColors [loopX, loopY].r > 0.7) { //mountains
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Mountain;
							} else if (texColors [loopX, loopY].b > 0.5) { //water 
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Water;
							} else if (texColors [loopX, loopY].g > 0.1) {
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Field;
							} else {
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.City;
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

	public void setDistances (float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		//Debug.Log ("Manhatten Distance");

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				fieldDistance (y, x, 0, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				fieldDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}
	}

	public void setDistancesChess (float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		Debug.Log ("Chess Distance");

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				fieldDistance (y, x, 0, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, -1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				fieldDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, -1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}
	}

	private void fieldDistance (int y, int x, int movingY, int movingX, bool firstRun, 
		float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		if (y == 0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) 
		{
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y, x];
		}

		else if (colorMap [y + movingY, x + movingX] == colorMap [y, x]) 
		{
			if (firstRun || pixelDistances [y, x] > pixelDistances [y + movingY, x + movingX])
			{
				pixelDistances [y, x] = pixelDistances [y + movingY, x + movingX] + 1;
				fieldEdgeTypes [y, x] = fieldEdgeTypes [y + movingY, x + movingX]; 
			}
		} 

		else 
		{
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y + movingY, x + movingX];
		}
	}


	public void setDistancesDiag (float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		Debug.Log ("Euclidean Distance");

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				fieldDiagDistance (y, x, 0, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, -1, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, -1, -1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				fieldDiagDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, 1, -1, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDiagDistance (y, x, 1, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}
	}

	private void fieldDiagDistance (int y, int x, int movingY, int movingX, bool firstRun, 
		float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		if (y == 0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) 
		{
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y, x];
		}

		else if (colorMap [y + movingY, x + movingX] == colorMap [y, x]) 
		{
			float comp = (float) System.Math.Sqrt (System.Math.Pow( movingY, 2) + System.Math.Pow( movingX, 2));
			Debug.Log (comp);

			if (firstRun || pixelDistances [y, x] > (pixelDistances [y + movingY, x + movingX] + comp ))
			{
				pixelDistances [y, x] = pixelDistances [y + movingY, x + movingX] + comp ;
				fieldEdgeTypes [y, x] = fieldEdgeTypes [y + movingY, x + movingX]; 
			}
		} 

		else 
		{
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y + movingY, x + movingX];
		}
	}

	/////////////////////////////////////
	/////////////////////////////////////
	//EUCLIDEAN VERSIONS OF ABOVE
	/////////////////////////////////////
	/////////////////////////////////////

	int[,] horiz;
	int[,] vert;

	public void setDistancesEuclidean (float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		Debug.Log ("Euclidean Distance");

		horiz = new int[pixelDistances.GetLength(0),pixelDistances.GetLength(1)];
		vert  = new int[pixelDistances.GetLength(0), pixelDistances.GetLength(1)];

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				fieldDistanceHoriz (y, x, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				fieldDistanceHoriz (y, x, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = 0; y < pixelDistances.GetLength (0); y++) {
				fieldDistanceVert (y, x, -1, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
				fieldDistanceVert (y, x, 1, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}
			
		Debug.Log (System.Math.Pow(-10, 2));
	}

	//
	//
	//
	//
	//

	private void fieldDistanceHoriz (int y, int x, int movingX, bool firstRun, float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		if (x == 0 || x == pixelDistances.GetLength (1) - 1) 
		{
			horiz [y, x] = movingX;
			vert [y, x] = 0;
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y, x];
		}

		else if (colorMap [y, x + movingX] == colorMap [y, x]) 
		{
			if (firstRun || pixelDistances [y, x] > pixelDistances [y, x + movingX])
			{
				horiz [y, x] = horiz [y, x + movingX] + movingX;
				vert [y, x] = 0;
				pixelDistances [y, x] = pixelDistances [y, x + movingX] + 1;
				fieldEdgeTypes [y, x] = fieldEdgeTypes [y, x + movingX]; 
			}
		} 

		else 
		{
			horiz [y, x] = movingX;
			vert [y, x] = 0;
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y, x + movingX];
		}
	}

	//
	//
	//

	private void fieldDistanceVert (int y, int x, int movingY, float[,] pixelDistances, int[,] colorMap, int[,] fieldEdgeTypes)
	{
		if (y == 0 || y == pixelDistances.GetLength (0) - 1) 
		{
			horiz [y, x] = 0;
			vert [y, x] = movingY;
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y, x];
		}

		else if (colorMap [y + movingY, x] == colorMap [y, x]) 
		{
			if (pixelDistances [y, x] > pixelDistances [y + movingY, x])
			{
				vert [y, x] = vert [y + movingY, x] + movingY;
				horiz [y, x] = horiz [y + movingY, x];

				pixelDistances [y, x] = (float) System.Math.Sqrt (System.Math.Pow(vert[y, x], 2) + System.Math.Pow(horiz[y, x], 2));

				fieldEdgeTypes [y, x] = fieldEdgeTypes [y + movingY, x]; 
			}
		} 

		else 
		{
			horiz [y, x] = 0;
			vert [y, x] = movingY;
			pixelDistances [y, x] = 1;
			fieldEdgeTypes [y, x] = colorMap [y + movingY, x];
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	//sets all the corner pixels to 0 to fix issue where corners are large spikes.
	//This is a workaround for some sort of bug
	public void removeCornerPillars (float[,] pixelDistances)
	{
		pixelDistances [0, 0] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, 0] = 0;
		pixelDistances [0, pixelDistances.GetLength (0) - 1] = 0;
		pixelDistances [pixelDistances.GetLength (0) - 1, pixelDistances.GetLength (0) - 1] = 0;
	}

}

//WOW