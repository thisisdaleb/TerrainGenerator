using UnityEngine;
using System.Collections;

public class ImageDistances
{

	enum ground : int
	{
		Field,
		Mountain,
		Water,
		City 
	}
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

							if (tex.GetPixel (loopX, loopY).r > 0.7) { //mountains
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Mountain;
							} else if (tex.GetPixel (loopX, loopY).b > 0.5) { //water 
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Water;

							} else if(tex.GetPixel (loopX, loopY).g > 0.5){
								colorMap [(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] = (int)ground.Field;
							}
							else{
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

		fixCityEdges (colorMap, fieldEdgeTypes);
		setDistances (pixelDistances, colorMap, fieldEdgeTypes);
	}

	private void fixCityEdges (int[,] colorMap, bool[,] fieldEdgeTypes)
	{
		int[, ] newColors = new int[ colorMap.GetLength(0),  colorMap.GetLength (1)];
		for (int y = 1; y < colorMap.GetLength (0)-1; y++) {
			for (int x = 1; x < colorMap.GetLength (1)-1; x++) {
				if (colorMap [y, x] == (int)ground.City) {
					if (colorMap [y, x - 1] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x - 1];
					} else if (colorMap [y, x + 1] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x + 1];
					} else if (colorMap [y - 1, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y - 1, x];
					} else if (colorMap [y + 1, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y + 1, x];
					}
					else if (colorMap [y, x - 2] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x - 2];
					} else if (colorMap [y, x + 2] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x + 2];
					} else if (colorMap [y - 2, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y - 2, x];
					} else if (colorMap [y + 2, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y + 2, x];
					}
					else if (colorMap [y, x - 3] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x - 3];
					} else if (colorMap [y, x + 3] != (int)ground.City) {
						newColors [y, x] = colorMap [y, x + 3];
					} else if (colorMap [y - 3, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y - 3, x];
					} else if (colorMap [y + 3, x] != (int)ground.City) {
						newColors [y, x] = colorMap [y + 3, x];
					}
				} 
				else newColors [y, x] = colorMap [y, x];
			}
		}
		colorMap = newColors;
	}

	private void setDistances (float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
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
				pixelTypeDistance (y, x, 0, 1, (int)ground.City, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 0, 1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = 0; y < pixelDistances.GetLength (0); y++) {
				pixelTypeDistance (y, x, -1, 0, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, -1, 0, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, -1, 0, (int)ground.City, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, -1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int x = 0; x < pixelDistances.GetLength (1); x++) {
			for (int y = pixelDistances.GetLength (0) - 1; y >= 0; y--) {
				pixelTypeDistance (y, x, 1, 0, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 1, 0, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 1, 0, (int)ground.City, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 1, 0, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

		for (int y = 0; y < pixelDistances.GetLength (0); y++) {
			for (int x = 0; x < pixelDistances.GetLength (1); x++) {
				pixelTypeDistance (y, x, 0, -1, (int)ground.Mountain, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 0, -1, (int)ground.Water, false, pixelDistances, colorMap, fieldEdgeTypes);
				pixelTypeDistance (y, x, 0, -1, (int)ground.City, false, pixelDistances, colorMap, fieldEdgeTypes);
				fieldDistance (y, x, 0, -1, false, pixelDistances, colorMap, fieldEdgeTypes);
			}
		}

	}

	private void pixelTypeDistance (int y, int x, int movingY, int movingX, int groundType, bool firstRun, 
		float[,] pixelDistances, int[,] colorMap, bool[,] fieldEdgeTypes)
	{
		if (colorMap [y, x] == groundType) {
			if (y ==0 || x == 0 || y == pixelDistances.GetLength (0) - 1 || x == pixelDistances.GetLength (1) - 1) {
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
