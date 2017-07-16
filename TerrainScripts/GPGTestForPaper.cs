using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GPGTestForPaper : MonoBehaviour
{
	//THIS IS THE CURRENT MAIN PROGRAM FOR GENERATING WORLDS BY INPUT IMAGE

	private int length;
	private int[,]    colorMap;				//the terrain type of each pixel
	private float[, ] pixelDistances;		//the distance from each pixel to a pixel of a different color
	private int[, ]   fieldEdgeTypes;		//tells field pixels whether they are closest to mountains or water
	private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch ();

	//public values that users can edit in GUI.
	[Tooltip ("Checking this box starts the system")]
	public bool runNow;
	[Tooltip ("The image used for making the world")]
	public Texture2D tex;
	[Tooltip ("how many pixels wide the output should be (2049 or 4097)")]
	public int width = 2049;


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
		colorMap =       new   int[width, length];
		pixelDistances = new float[width, length];
		fieldEdgeTypes = new   int[width, length];
	}

	// Update is called once per frame
	void Update ()
	{
		//if the run button has been checked and there is a map image loaded 
		if (runNow && tex != null) {
			refreshVariables ();
			convertInputIntoMap ();
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
		print ("starting Timer");
		timer.Start ();
		setImage.setDistances (pixelDistances, colorMap, fieldEdgeTypes);
		print ("Time: " + timer.ElapsedMilliseconds);

		Debug.Log ("top left: " + pixelDistances [0, 0]);
		Debug.Log ("one pixel over: " + pixelDistances [1, 0]);
		Debug.Log (pixelDistances [pixelDistances.GetLength (0) - 1, 0]);
		Debug.Log (pixelDistances [0, pixelDistances.GetLength (0) - 1]);
		Debug.Log (pixelDistances [pixelDistances.GetLength (0) - 1, pixelDistances.GetLength (0) - 1]);

		setImage.removeCornerPillars (pixelDistances);

		Debug.Log ("top left: " + pixelDistances [0, 0]);
		Debug.Log ("one pixel over: " + pixelDistances [1, 0]);
		Debug.Log (pixelDistances [pixelDistances.GetLength (0) - 1, 0]);
		Debug.Log (pixelDistances [0, pixelDistances.GetLength (0) - 1]);
		Debug.Log (pixelDistances [pixelDistances.GetLength (0) - 1, pixelDistances.GetLength (0) - 1]);


		//Now that we know the distances to a different field type for each pixel,
		//we can finish up by creating the height map matrix
		Debug.Log ("FINISHED");

	}
}
