
public class ImplementationAttempt {

	public void createAndPrintInefficientMatrix() {
		float[][] startingArr = new float[2][2];
		
		System.out.println();
		System.out.println();
		System.out.print((startingArr[0][0] = 0.2f) + "  ");
		System.out.println((startingArr[0][1] = 0.8f) + "  ");
		//System.out.println(startingArr[0][2] = 0.3f);
		System.out.print((startingArr[1][0] = 0.1f) + "  ");
		System.out.print((startingArr[1][1] = 0.6f) + "  ");
		//System.out.println(startingArr[1][2] = 0.4f);
		//System.out.print((startingArr[2][0] = 0.3f) + "  ");
		//System.out.print((startingArr[2][1] = 0.5f) + "  ");
		//System.out.println(startingArr[2][2] = 0.7f);
		System.out.println();

		int finalSize = 5;
		System.out.print("Expand matrix to size (length and width): " + finalSize);
		//printer=cornersOfMatrix(startingArr, 4, 4);

		float[][] printer = smartestInterpolation(startingArr, finalSize);

		printOutMatrix(printer);
	}
	
	private void printOutMatrix(float[][] arr){
		for(int k=0;k<arr.length;k++){
			System.out.println();
			printDecimals(arr[k][0] );
			for(int x = 1; x < arr[0].length; x++){
				printDecimals(arr[k][x]);
			}
		}
	}
	
	private float[][] smartestInterpolation(float[][] oldMat, int expand){
		int newXY = expand*oldMat.length-expand+1;
		float[][] newMat = new float[newXY][newXY];
		int multiplier = (int) ((newXY-1)/(oldMat.length-1));
		int x1;
		int y1;
		int x2;
		int y2;
		float Q1;
		float Q2;
		float Q3;
		float Q4;
		System.out.println();

		for(int y=0; y< newMat.length-1; y++){

			for(int x = 0; x < newMat[0].length-1; x++){
				if(y%multiplier==0 && x%multiplier==0){
					newMat[y][x] = oldMat[y/multiplier][x/multiplier];
				}
				else{
					x1 = x/multiplier*multiplier;
					x2 = x1+multiplier;
					y1 = y/multiplier*multiplier;
					y2 = y1+multiplier;
					Q1 = oldMat[y/multiplier][x/multiplier];
					Q2 = oldMat[y/multiplier][x/multiplier+1];
					Q3 = oldMat[y/multiplier+1][x/multiplier];
					Q4 = oldMat[y/multiplier+1][x/multiplier+1];
					
					newMat[y][x] = fixedCalc((float)(x-x1)/(x2-x1), (float)(y-y1)/(y2-y1), Q1, Q2, Q3, Q4);
				}
			}
		}
		int y = newMat.length-1;
		for(int x = 0; x < newMat[0].length-1; x++){
			if(x%multiplier==0){
				newMat[y][x] = oldMat[y/multiplier][x/multiplier];
			}
			else{
				x1 = x/multiplier*multiplier;
				x2 = x1+multiplier;
				newMat[y][x] = interpolate(oldMat[y/multiplier][x1/multiplier], oldMat[y/multiplier][x2/multiplier], (float)(x-x1)/(x2-x1));
			}
		}
		
		int x = newMat.length-1;
		for(y = 0; y < newMat.length; y++){
			if(y%multiplier==0){
				newMat[y][x] = oldMat[y/multiplier][x/multiplier];
			}
			else{
				y1 = y/multiplier*multiplier;
				y2 = y1+multiplier;
				newMat[y][x] = interpolate(oldMat[y1/multiplier][x/multiplier], oldMat[y2/multiplier][x/multiplier], (float)(y-y1)/(y2-y1));
			}
		}

		return newMat;
	}
	
	private float fixedCalc(float fractionX, float fractionY, float Q1, float Q2, float Q3, float Q4){
				return (1 - fractionX) * 
				                        ((1 - fractionY) * Q1 + 
				                         fractionY * Q3) + 
				                    fractionX * 
				                        ((1 - fractionY) * Q2 + 
				                        fractionY * Q4);
	}

	private float[][] smarterInterpolation(float[][] oldMat, int newXY){
		float[][] newMat = new float[newXY][newXY];
		int multiplier = (int) ((newXY-1)/(oldMat.length-1));
		int x1;
		int y1;
		int x2;
		int y2;
		float Q11;
		float Q12;
		float Q21;
		float Q22;
		System.out.println();

		for(int y=0; y< newMat.length-1; y++){

			for(int x = 0; x < newMat[0].length-1; x++){
				if(y%multiplier==0 && x%multiplier==0){
					newMat[y][x] = oldMat[y/multiplier][x/multiplier];
				}
				else{
					x1 = x/multiplier*multiplier;
					x2 = x1+multiplier;
					y1 = y/multiplier*multiplier;
					y2 = y1+multiplier;
					Q11 = oldMat[y/multiplier][x/multiplier];
					Q12 = oldMat[y/multiplier][x/multiplier+1];
					Q21 = oldMat[y/multiplier+1][x/multiplier];
					Q22 = oldMat[y/multiplier+1][x/multiplier+1];
					if(x>3 && y<2){
						System.out.println(" " + y + " " + x + " " + x1 + " " + x2 + " " + y1 + " " + y2 + " " + Q11 + " " + Q21 + " " + Q12 + " " + Q22 + " ");
					}
					newMat[y][x] = calcInterpolatedValue(y, x, x1,  x2, y1, y2, Q11, Q21, Q12, Q22);
				}
			}
		}
		int y = newMat.length-1;
		for(int x = 0; x < newMat[0].length-1; x++){
			if(x%multiplier==0){
				newMat[y][x] = oldMat[y/multiplier][x/multiplier];
			}
			else{
				x1 = x/multiplier*multiplier;
				x2 = x1+multiplier;
				newMat[y][x] = interpolate(oldMat[y/multiplier][x1/multiplier], oldMat[y/multiplier][x2/multiplier], (float)(x-x1)/(x2-x1));
			}
		}
		
		int x = newMat.length-1;
		for(y = 0; y < newMat.length; y++){
			if(y%multiplier==0){
				newMat[y][x] = oldMat[y/multiplier][x/multiplier];
			}
			else{
				y1 = y/multiplier*multiplier;
				y2 = y1+multiplier;
				newMat[y][x] = interpolate(oldMat[y1/multiplier][x/multiplier], oldMat[y2/multiplier][x/multiplier], (float)(y-y1)/(y2-y1));
			}
		}

		return newMat;
	}

	private float calcInterpolatedValue(float x, float y, float x1,  float x2, float y1, float y2, float Q11, float Q21, float Q12, float Q22){

		/**
		 * (x1, y1) - coordinates of corner 1 - [Q11]
		 * (x2, y1) - coordinates of corner 2 - [Q21]
		 * (x1, y2) - coordinates of corner 3 - [Q12]
		 * (x2, y2) - coordinates of corner 4 - [Q22]
		 * 
		 * (x, y)   - coordinates of interpolation
		 * 
		 * Q11      - corner 1 value
		 * Q21      - corner 2 value
		 * Q12      - corner 3 value
		 * Q22      - corner 4 value
		 */

		float ans1 = (((x2-x)*(y2-y))/((x2-x1)*(y2-y1)))*Q11;
		float  ans2 = (((x-x1)*(y2-y))/((x2-x1)*(y2-y1)))*Q21;
		float ans3 = (((x2-x)*(y-y1))/((x2-x1)*(y2-y1)))*Q12;
		float ans4 = (((x-x1)*(y-y1))/((x2-x1)*(y2-y1)))*Q22;
		if(x>3 && y<2){
			System.out.println(ans1 + " " + ans2 + " " + ans3 + " " + ans4 + " ");
		}
		return (ans1+ans2+ans3+ans4);
	};

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
