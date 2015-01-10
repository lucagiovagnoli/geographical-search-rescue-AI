using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frameComparatorByProfitDensity : IComparer<Frame>  {
	int IComparer<Frame>.Compare(Frame t, Frame o)  {
		return (int) (o.profitDensity - t.profitDensity);
	}
}

public class Frame {

	public Grid grid{ get; set;}
	public int topLeftCorner{get;set;}
	public int bottomRightCorner{get;set;}
	
	public int xSide{ get; private set; }
	public int zSide{ get; private set; }
	public Vector3 center{ get; private set; } 
	public float area{ get; private set;}
	public float profitDensity{get;private set;} //profitPerSquareUnit 
	public float totalProfit{ get; private set;} //quality takes into account the

	//debug cube
	private GameObject debugCube;

	public Frame (Grid grid, Vector3 topLeftCorner, Vector3 bottomRightCorner, float profitDensity=1.0f){
		this.grid = grid;
		this.topLeftCorner = grid.vectorToIndex(topLeftCorner);
		this.bottomRightCorner = grid.vectorToIndex(bottomRightCorner);
		this.profitDensity = profitDensity;
		computeProperties ();
    }
	public Frame (Grid grid, int topLeftCorner, int bottomRightCorner, float profitDensity=1.0f){
		this.grid = grid;
		this.topLeftCorner = topLeftCorner;
		this.bottomRightCorner = bottomRightCorner;
		this.profitDensity = profitDensity;
		computeProperties ();
	}

	private void computeProperties(){
		this.xSide = Math.Abs(grid.indexToX(bottomRightCorner)-grid.indexToX(topLeftCorner));
		this.zSide = Math.Abs(grid.indexToZ(bottomRightCorner)-grid.indexToZ(topLeftCorner));
		this.center = grid.indexToVector(topLeftCorner)+new Vector3((float)xSide/2,0.0f,-(float)zSide/2);
		this.area =  xSide*zSide;
		this.totalProfit = this.profitDensity*this.area;
	}

	public void displayDebug(Color color, float height=1.0f){
		debugCube = (GameObject) GameObject.CreatePrimitive(PrimitiveType.Cube);
		debugCube.renderer.material.color = color; 
		debugCube.transform.localScale = new Vector3(this.xSide,1.0f,this.zSide);
		debugCube.transform.position = this.center+new Vector3(0.0f,height,0.0f);
    }
	public void destroyDebug(){GameObject.Destroy(debugCube);}

}




