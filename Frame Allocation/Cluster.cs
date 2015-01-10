using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cluster{

	public List<Frame> frames { get; private set;}

	private bool isCentroidComputerAlready = false;
	private Vector3 _centroid = Vector3.zero;
	public Vector3 Centroid { 
		//lazy computing of centroid - only if getter is called
		get{
			if(isCentroidComputerAlready == true) return _centroid;
			else {
				isCentroidComputerAlready = true;
				foreach (Frame f in frames) {
					this._centroid.x += f.center.x;
					this._centroid.z += f.center.z;
				}
				this._centroid.x /= frames.Count;
				this._centroid.z /= frames.Count;

				return _centroid;
			}
		}
		private set{
			this._centroid = value;
		}
	}

	private GameObject debugCube;

	public Cluster (){
		this.frames = new List<Frame>();
	}

	public void addFrame(Frame frame){
		this.frames.Add (frame);
		isCentroidComputerAlready = false; // centroid is changed!
	}

	public void sortFramesByProfitDensity(){
		frames.Sort(new frameComparatorByProfitDensity());
	}

	public void debugCentroid(){
		debugCube = (GameObject) GameObject.CreatePrimitive(PrimitiveType.Sphere);
		debugCube.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
		debugCube.transform.position = this.Centroid + new Vector3 (0.0f,5.0f,0.0f);
	}
	public void destryDebug(){
		GameObject.Destroy(debugCube);
	}
}

