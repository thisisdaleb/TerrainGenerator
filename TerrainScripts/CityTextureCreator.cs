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

		while (loopY < imageLoopY-1) {
			while (loopX < imageLoopX-1) {
				while (placeY < yPlaced) {
					while (placeX < xPlaced) {
						if ((yPlaced * loopY) + placeY < length && (xPlaced * loopX) + placeX < width) {

							if (tex.GetPixel (loopX, loopY).r > 0.7) { //mountains
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.01f;
							} else if (tex.GetPixel (loopX, loopY).b > 0.7) { //water 
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.03f;
							} else if(tex.GetPixel (loopX, loopY).g > 0.7){
								heightMap[(yPlaced * loopY) + placeY, (xPlaced * loopX) + placeX] += 0.06f;
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
		
	private bool checkSection()
	{
		return true;
	}
}
