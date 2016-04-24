
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
		
		//possible floating point precision error
		int totalSections = (int) (newMat[0].length/(addRows+1));
		
		newMat =  completedVertical(newMat, addRows, totalSections);
		
		return newMat;
	}
	
	private float[][] completedVertical(float[][] newMat, float rowsBetween, float totalSections){
		
		//fill in all rows in between each already completed row
		
		float row = 1;
		float col = 0;
		
		float totalCols = newMat[0].length;
		float totalRows = newMat.length;
		
		float ARow = 0f;
		float BRow = rowsBetween; 
		float A = 0f;
		float B = 0f;
		float fraction = 0f;
		
		//This implementation is so inefficient, going row by row, instead of column by column
		
		//loop through all rows
		while(row<totalRows-1){
			//loop through all columns
			while(col < totalCols){
				A = newMat[(int) ARow][(int) col];
				B = newMat[(int) BRow][(int) col];
				fraction = (row-ARow)/(BRow-ARow);
				System.out.println(A + "  " + B + "  " + fraction);
				newMat[(int) row][(int) col] = interpolate(A, B, fraction);
				col++;
			}
			
			col=0;
			row++;
			if(row%rowsBetween==0){
				row++;
				ARow+=rowsBetween;
				BRow+=rowsBetween;
			}
			
		}
		
		
		return newMat;
	}
	
	private float interpolate(float a, float b, float x){
		return a*(1f-x) + b*x;
	}

	private void printDecimals(float flt){
		System.out.printf("%.3f" + "  ", flt);
	}
}
