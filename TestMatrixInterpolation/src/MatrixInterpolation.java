/*
 * This class was how I learned how to do bilinear Interpolation.
 * I'm really bad at math, so I absolutely required it.
 */

public class MatrixInterpolation {
	
	public static void main(String[] args) {
		float[][] arr = new float[4][4];
		arr[0][0] = 0.2f;
		arr[0][arr.length-1] = 0.8f;		
		arr[arr.length-1][0] = 0.1f;
		arr[arr.length-1][arr.length-1] = 0.6f;
		printIntendedVersion();
		ImplementationAttempt tryRhymesWithCry =  new ImplementationAttempt();
		tryRhymesWithCry.createAndPrintInefficientMatrix();
		//FillOutMatrix(arr, 2, 2);
		//printOutMatrix(arr);
	}
	
	private static void printIntendedVersion() {
		//Done stupidly simply to avoid ANY possible mistakes.
		System.out.println();
		//prints out row 1
		printDecimals(0.2f );
		printDecimals(interpolate(0.2f, 0.8f, 1/3f));
		printDecimals(interpolate(0.2f, 0.8f, 2/3f));
		printDecimals(0.8f);
		System.out.println();
		//prints out row 2
		printDecimals(interpolate(0.2f, 0.1f, 1/3f) );
		printDecimals(interpolate(0.4f, 0.26667f, 1/3f));
		printDecimals(interpolate(0.6f, 0.43333f, 1/3f));
		printDecimals(interpolate(0.8f, 0.6f, 1/3f));
		System.out.println();
		//prints out row 3
		printDecimals(interpolate(0.2f, 0.1f, 2/3f) );
		printDecimals(interpolate(0.4f, 0.26667f, 2/3f));
		printDecimals(interpolate(0.6f, 0.43333f, 2/3f));
		printDecimals(interpolate(0.8f, 0.6f, 2/3f));
		System.out.println();
		//prints out row 4
		printDecimals(0.1f);
		printDecimals(interpolate(0.1f, 0.6f, 1/3f));
		printDecimals(interpolate(0.1f, 0.6f, 2/3f));
		printDecimals(0.6f);
		System.out.println();
		System.out.println();
	}
	
	//could have the inputs be original size, minimum final size.
	//then fill nodes up horizontally on the rows that have values.
	//then fill nodes up vertically, in the remaining rows.
	
	private static void FillOutMatrix(float[][] arr, int spaceBetween, int origSize) {
		//manipulate the matrix in this method to create a complete matrix, interpolation
		
		//First, interpolate the top and bottom rows
		//by knowing the original size, and what to add to the original
		for(int y = 0; y < arr.length; y+=spaceBetween+1){
			for(int x = 0; x < arr.length; x++){
				
			}
		}
		//second, interpolate 
	}

	private static void printOutMatrix(float[][] arr){
		System.out.println();
		for(int k=0;k<arr.length;k++){
			System.out.println();
			printDecimals(arr[k][0] );
			for(int x = 1; x < arr[0].length; x++){
				printDecimals(arr[k][x]);
			}
		}
	}

	private static float interpolate(float a, float b, float x){
		return a*(1f-x) + b*x;
	}
	
	private static void printDecimals(float flt){
		System.out.printf("%.3f" + "  ", flt);
	}
}
