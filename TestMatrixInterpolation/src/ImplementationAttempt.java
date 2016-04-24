
public class ImplementationAttempt {
	
	public void createAndPrintInefficientMatrix() {
		float[][] startingArr = new float[2][2];
		float[][] printer = new float[4][4];
		
		System.out.println(startingArr[0][0] = 0.2f);
		System.out.println(startingArr[0][1] = 0.8f);
		System.out.println(startingArr[1][0] = 0.1f);
		System.out.println(startingArr[1][1] = 0.6f);
		System.out.println();
		System.out.println();
		
		printer=cornersOfMatrix(startingArr, 4, 4);
		
		printIt(printer, true);
	}

	private void printIt(float[][] printer, boolean doIt) {
		if(!doIt){
			System.out.println();
			printDecimals(printer[0][0]);
			printDecimals(printer[0][3]);
			System.out.println();
			printDecimals(printer[3][0]);
			printDecimals(printer[3][3]);
		}
		else{
		System.out.println();
		printDecimals(printer[0][0]);
		printDecimals(printer[0][1]);
		printDecimals(printer[0][2]);
		printDecimals(printer[0][3]);
		System.out.println();
		printDecimals(printer[1][0]);
		printDecimals(printer[1][1]);
		printDecimals(printer[1][2]);
		printDecimals(printer[1][3]);
		System.out.println();
		printDecimals(printer[2][0]);
		printDecimals(printer[2][1]);
		printDecimals(printer[2][2]);
		printDecimals(printer[2][3]);
		System.out.println();
		printDecimals(printer[3][0]);
		printDecimals(printer[3][1]);
		printDecimals(printer[3][2]);
		printDecimals(printer[3][3]);
		}
	}
	
	private float[][] cornersOfMatrix(float[][] origArr, int xSize, int ySize) {
		
		//newMatrix
		float newMat[][] = new float[xSize][ySize];
		
		//original matrix size
		float origRows = origArr.length;
		float origCols = origArr[0].length;
		
		//what number to use
		float addRows = 3;
		float addCols = 3;
		
		float timesToRunVertical = ySize/origCols;
		
		//current row and column being used for the calculation
		int row = 0, col = 0, newRow = 0, newCol = 0;
		float currPoint=1;
		
		//Inputs into interpolating method
		float A = 0f;
		float B = 0f;
		float distanceBetween = 0f;
		
		//
		//Begin calculation of columns of main rows
		//
		while(row<origRows){
			
			newMat[newRow][newCol] = origArr[row][col];
			
			newCol++;
			
			while(col<origCols-1){
				
				A = origArr[row][col];
				B = origArr[row][col+1];
				
				while(currPoint<addCols){
					
					distanceBetween = currPoint/addCols;
					System.out.println(A + " " + B + " " + distanceBetween);

					newMat[newRow][newCol] = interpolate(A, B, distanceBetween);
					
					newCol++;
					currPoint++;
				}
				currPoint=1;
				col++;
				//set end of current section to correct number
				newMat[newRow][newCol] = origArr[row][col];
			}
			
			//reset to row
			row++;
			newRow+=addRows;
			
			//reset to firstColumn
			col=0;
			newCol=0;
		}
		
		row=0;
		col=0;
		newRow = 0;
		newCol = 0;
		currPoint=0;
		System.out.println();
		
		//Now does the rows in between
		
		//while the row counter is less than the final row of the original matrix
		////move to next row
		////while the columns hasn't gone over the end of original matrix
		//////A is equal to the original number right above where the average is being done
		//////B is right below where the average is being done
		//////while the current pointer is less than the rows being added in:
		////////the distance in-between is set to the distance between A and B on the new Matrix
		////////the value on the new matrix is set
		////////newCol increases, currPoint increases
		//////currPoint is set to 0
		//////the column of the original 
		//////
		
		//THE FREAKING ISSUE:
		//I DONT NEED TO BE LOOKING AT THE ORIGINAL MATRIX ANYMORE
		//WHAT IS WRONG WITH THIS PROGRAMMER
		
		return newMat;
	}
	
	private float interpolate(float a, float b, float x){
		return a*(1f-x) + b*x;
	}

	private void printDecimals(float flt){
		System.out.printf("%.3f" + "  ", flt);
	}
}
