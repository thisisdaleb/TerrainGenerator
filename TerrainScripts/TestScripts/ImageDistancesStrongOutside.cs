using UnityEngine;
using System.Collections;

public class ImageDistancesStrongOutside
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
		//GetPixel is not efficient. This method could run 100X faster if I replace that with GetPixels or GetPixels32 or whatever I need.

		int imageLoopX = tex.width;
		int imageLoopY = tex.height;

		int loopX = 0;
		int loopY = 0;

		int xPlaced = width / imageLoopX;
		int yPlaced = length / imageLoopY;

		int placeX = 0;
		int placeY = 0;

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

		setDistances (pixelDistances, colorMap, fieldEdgeTypes);
	}

	private void setDistances (float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
	{
		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				pixelTypeDistance (y, x, 0, -1, (int)ground.Mountain, true, pixelDistances, colorMap);
				pixelTypeDistance (y, x, 0, -1, (int)ground.Water, true, pixelDistances, colorMap);
				fieldDistance (y, x, 0, -1, true, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = pixelDistances.GetLength (1) - 1; x >= 0; x--) {
				pixelTypeDistance (y, x, 0, 1, (int)ground.Mountain, false, pixelDistances, colorMap);
				pixelTypeDistance (y, x, 0, 1, (int)ground.Water, false, pixelDistances, colorMap);
				fieldDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = 0; y < pixelDistances.GetLength (0); y++) {
				pixelTypeDistance (y, x, -1, 0, (int)ground.Mountain, false, pixelDistances, colorMap);
				pixelTypeDistance (y, x, -1, 0, (int)ground.Water, false, pixelDistances, colorMap);
				fieldDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
				pixelTypeDistance (y, x, 1, 0, (int)ground.Mountain, false, pixelDistances, colorMap);
				pixelTypeDistance (y, x, 1, 0, (int)ground.Water, false, pixelDistances, colorMap);
				fieldDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		setTrueEdges (pixelDistances, colorMap);

	}

	private void pixelTypeDistance (int y, int x, int movingY, int movingX, int groundType, bool firstRun, 
	                                float[,] pixelDistances, int[,] colorMap)
	{
		if (colorMap [y, x] == groundType) {
			if (y < 2 || x < 2 || y > pixelDistances.GetLength (0) - 3 || x > pixelDistances.GetLength (1) - 3) {
				pixelDistances [y, x] = 1000;
			} else if (colorMap [y + movingY, x + movingX] == groundType) {
				if (firstRun || pixelDistances [y, x] > pixelDistances [y + movingY, x + movingX])
					pixelDistances [y, x] = pixelDistances [y + movingY, x + movingX] + 1;
			} else {
				pixelDistances [y, x] = 1;
			}
		}
	}

	private void setTrueEdges (float[,] pixelDistances, int[,] colorMap)
	{
		for (int k = 1; k < pixelDistances.GetLength (0) - 2; k++) {
			//left side
			pixelDistances [k, 2] = pixelDistances [k, 3] - 1;
			pixelDistances [k, 1] = pixelDistances [k, 2] - 1;
			pixelDistances [k, 0] = pixelDistances [k, 1] - 1;
			//top side
			pixelDistances [2, k] = pixelDistances [3, k] - 1;
			pixelDistances [1, k] = pixelDistances [2, k] - 1;
			pixelDistances [0, k] = pixelDistances [1, k] - 1;
			//bottom side
			pixelDistances [pixelDistances.GetLength (0) - 3, k] = pixelDistances [pixelDistances.GetLength (0) - 4, k] - 1;
			pixelDistances [pixelDistances.GetLength (0) - 2, k] = pixelDistances [pixelDistances.GetLength (0) - 3, k] - 1;
			pixelDistances [pixelDistances.GetLength (0) - 1, k] = pixelDistances [pixelDistances.GetLength (0) - 2, k] - 1;
			//right side
			pixelDistances [k, pixelDistances.GetLength (0) - 3] = pixelDistances [k, pixelDistances.GetLength (0) - 4] - 1;
			pixelDistances [k, pixelDistances.GetLength (0) - 2] = pixelDistances [k, pixelDistances.GetLength (0) - 3] - 1;
			pixelDistances [k, pixelDistances.GetLength (0) - 1] = pixelDistances [k, pixelDistances.GetLength (0) - 2] - 1;
		}
	}

	private void fieldDistance (int y, int x, int movingY, int movingX, bool firstRun, 
	                            float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
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
}
