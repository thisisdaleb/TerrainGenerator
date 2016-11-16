using UnityEngine;
using System.Collections;
using System.Linq;

// used for Sum of array

public class CityTextureCreator
{
	public void setBuildings (Texture2D tex, int width, int length, float[,] heightMap)
	{//GetPixel is not efficient. This method could run 100X faster if I replace that with GetPixels or GetPixels32 or whatever I need.

		int imageLoopX = tex.width;
		int imageLoopY = tex.height;

		int loopX = 1;
		int loopY = 1;

		int xPlaced = width / imageLoopX;
		int yPlaced = length / imageLoopY;

		int placeX = 0;
		int placeY = 0;

		//1D array of colors of all pixels of texture
		Color[] texColor1D = tex.GetPixels ();
		//matrix of colors of texture
		Color[,] texColors = new Color[imageLoopX, imageLoopY];

		//loops horizontally and vertically through texture
		for(int Cy = 0; Cy < imageLoopY; Cy++)
		{
			for(int Cx = 0; Cx < imageLoopX; Cx++)
			{
				//pulls 1D array into matrix
				texColors [Cx, Cy] = texColor1D[(Cy*imageLoopX)+Cx];
			}
		}

		while (loopY < imageLoopY-2) {
			while (loopX < imageLoopX-2) {
				while (placeY < yPlaced) {
					while (placeX < xPlaced) {
						if ((yPlaced * loopY) + placeY < length && (xPlaced * loopX) + placeX < width) {

							if (checkRedSection(texColors,loopX, loopY, 0.7f)) 
							{ //mountains
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.01f;
							} 
							else if (checkBlueSection(texColors,loopX, loopY, 0.7f)) 
							{//water 
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.03f;
							} 
							else if (checkGreenSection(texColors,loopX, loopY, 0.7f)) 
							{
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.04f;
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
			loopX = 1;
			loopY++;
		}
	}
		
	private bool checkRedSection(Color[,] texColors, int loopX, int loopY, float amount)
	{
		if(texColors[loopX, loopY].r > amount ||
			texColors[loopX+1, loopY].r > amount ||
			texColors[loopX-1, loopY].r > amount ||
			texColors[loopX, loopY+1].r > amount ||
			texColors[loopX, loopY-1].r > amount ||
			texColors[loopX-1, loopY-1].r > amount ||
			texColors[loopX+1, loopY-1].r > amount ||
			texColors[loopX-1, loopY+1].r > amount ||
			texColors[loopX+1, loopY+1].r > amount)
			return true;
		return false;
	}

	private bool checkBlueSection(Color[,] texColors, int loopX, int loopY, float amount)
	{	
		if(texColors[loopX, loopY].b > amount ||
			texColors[loopX+1, loopY].b > amount ||
			texColors[loopX-1, loopY].b > amount ||
			texColors[loopX, loopY+1].b > amount ||
			texColors[loopX, loopY-1].b > amount ||
			texColors[loopX-1, loopY-1].b > amount ||
			texColors[loopX+1, loopY-1].b > amount ||
			texColors[loopX-1, loopY+1].b > amount ||
			texColors[loopX+1, loopY+1].b > amount)
			return true;
		return false;
	}

	private bool checkGreenSection(Color[,] texColors, int loopX, int loopY, float amount)
	{
		if(texColors[loopX, loopY].g > amount ||
			texColors[loopX+1, loopY].g > amount ||
			texColors[loopX-1, loopY].g > amount ||
			texColors[loopX, loopY+1].g > amount ||
			texColors[loopX, loopY-1].g > amount ||
			texColors[loopX-1, loopY-1].g > amount ||
			texColors[loopX+1, loopY-1].g > amount ||
			texColors[loopX-1, loopY+1].g > amount ||
			texColors[loopX+1, loopY+1].g > amount)
			return true;
		return false;
	}
}
