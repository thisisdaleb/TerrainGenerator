using UnityEngine;
using System.Collections;

public class ImageDistances
{

	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City}
	;

	public void setColors (Texture2D tex, int width, int length, float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
	{

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

	public void setDistances (float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
	{
		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				pixelTypeDistance (y, x, 0, -1, (int)ground.Mountain, true, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 0, -1, (int)ground.Water, true, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 0, -1, (int)ground.City, true, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 0, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				pixelTypeDistance (y, x, 0, 1, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 0, 1, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = 0; y < pixelDistances.GetLength (0); y++) {
				pixelTypeDistance (y, x, -1, 0, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, -1, 0, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
				pixelTypeDistance (y, x, 1, 0, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 1, 0, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}
	}

	private void pixelTypeDistance (int y, int x, int movingY, int movingX, int groundType, bool firstRun, 
	                                float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
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

	private void fieldDistance (int y, int x, int movingY, int movingX, bool firstRun, 
	                            float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
	{
		if (colorMap [y, x] == (int)ground.Field || colorMap [y, x] == (int)ground.City) {
			if (y == 0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) {
				pixelDistances [y, x] = 1;
				fieldEdgeTypes [y, x] = true; 
				//true = Mountain Edge
				//false = Water Edge
			} else if (colorMap [y + movingY, x + movingX] == (int)ground.Field
			         || colorMap [y + movingY, x + movingX] == (int)ground.City) {
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
