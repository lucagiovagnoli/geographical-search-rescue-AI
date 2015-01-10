using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid: MonoBehaviour{
			
//	public readonly static gameObject terrain; //= gameObject.FindgameObjectWithTag("grid");
	public readonly float cellSize = 1.0f;

	//grid variables
	public Vector3 upperLeftCorner;
	public Vector3 bottomRightCorner;
	public int xGridSize{ get; set;}
	public int zGridSize{ get; set;}

	private float[,] gridProfits;
	private bool[,] gridSLAM;

	void Start (){
		upperLeftCorner = new Vector3(this.gameObject.transform.position.x-this.gameObject.transform.localScale.x*10/2+cellSize/2,1.0f,
		                              this.gameObject.transform.position.z+this.gameObject.transform.localScale.z*10/2-cellSize/2);
		bottomRightCorner = new Vector3(this.gameObject.transform.position.x+this.gameObject.transform.localScale.x*10/2-cellSize/2,1.0f,
		                                this.gameObject.transform.position.z-this.gameObject.transform.localScale.z*10/2+cellSize/2);
		xGridSize = (int) Mathf.Floor((bottomRightCorner.x-upperLeftCorner.x)/cellSize);
		zGridSize = (int) Mathf.Floor((upperLeftCorner.z-bottomRightCorner.z)/cellSize);

		gridSLAM = new bool[zGridSize,xGridSize];
		gridProfits = new float[zGridSize,xGridSize];

		Vector3 lastPoint = upperLeftCorner;
		for(int z =0;z<zGridSize;z++,lastPoint.z-=cellSize){
			for(int i=0;i<xGridSize;i++,lastPoint.x+=cellSize){
				//gridSLAM[i,z] = lastPoint;
				gridProfits[z,i] = 0;
			//	Debug.DrawLine(new Vector3(10.0f,10.0f,10.0f),lastPoint,Color.red,15.0f);
			}
			lastPoint.x = upperLeftCorner.x;
		}
	}

	public void addFrameProfits(Frame frame){
		for (int t=0;t<frame.zSide;t++){
			for(int i=0;i<frame.xSide;i++){
				int z=indexToZ(frame.topLeftCorner)+t, x=indexToX(frame.topLeftCorner)+i;
				gridProfits[z,x] += frame.profitDensity;
			}
		}
	}

	public void setObstacleCell(Vector3 pos){
		gridSLAM [vectorToZ (pos), vectorToX (pos)] = true;
	}

	/* compute the intersecting zones where there is the best probability of finding debris */
	public List<Frame> getLikelyFrames(float explorableAreaInGivenTime){
		List<Frame> res = new List<Frame>();
		for (int t=0; t<zGridSize; t++) {
			for (int i=0; i<xGridSize; i++) {
				if(gridProfits[t,i] > 0){
					float intersectionProfit = gridProfits[t,i];
					int t1=t,i1=i,flagFirst=1,rightCoordinate=0,endFlag=0;
					for (;endFlag==0;t1++){
						//first row
						if(flagFirst==1){
							for (i1=i;gridProfits[t1,i1]==intersectionProfit;i1++)
								gridProfits[t1,i1] = 0; //so it will never be explored twice
							flagFirst = 0;
							rightCoordinate = i1;
						}
						else{
							for (i1=i;i1<rightCoordinate && endFlag==0;i1++){
								if(gridProfits[t1,i1]==intersectionProfit)
									gridProfits[t1,i1] = 0; //so it will never be explored twice
								else{
									for (int r=i;r<i1;r++) gridProfits[t1,r] = intersectionProfit; 
									endFlag=1;
								}
							}
						}
					}
					t1--;

					/* divide area in "small" frames*/
					//explorableAreaInGivenTime
					res.Add (new Frame(this,xzToIndex(i,t),xzToIndex(rightCoordinate,t1),intersectionProfit));
				}
			}
		}

		return res;
	}

	public void resetProfits() {
				Array.Clear (gridProfits, 0, xGridSize * zGridSize);
		}

	public int indexToX(int index){return index%xGridSize;}
	public int indexToZ(int index){return index/xGridSize;}
	public int xzToIndex(int x,int z){return z*xGridSize + x;}
	public int vectorToIndex(Vector3 v){return xzToIndex (vectorToX(v),vectorToZ(v));}		                                                  
	public int vectorToX(Vector3 v){return (int)Math.Floor(Math.Abs(v.x-upperLeftCorner.x)/cellSize);}
	public int vectorToZ(Vector3 v){return (int)Math.Ceiling(Math.Abs(v.z-upperLeftCorner.z)/cellSize);}
	public Vector3 indexToVector(int index){return new Vector3(upperLeftCorner.x+indexToX(index)*cellSize,0.0f,upperLeftCorner.z-indexToZ(index)*cellSize);}
	public Vector3 xzToVector(int x,int z){return indexToVector(xzToIndex(x,z));}
}




