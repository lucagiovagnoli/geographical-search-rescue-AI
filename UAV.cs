using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UAV : MonoBehaviour {

	private Navigation nav;
	private float walkableDistance = 100.0f; // fuel is limited

	private bool reachingFrame = false;
	private bool searchingFrame = false;

	private List<Frame> framesToVisit;
	private Frame currentFrame = null;
	private List<Frame>.Enumerator enFrame;
	private LinkedList<Vector3> route = null;
	private LinkedList<Vector3>.Enumerator enRoute;
	
	void Start () {
		this.nav = new Navigation (this.gameObject,Navigation.modelType.kinematic);
	}
	
	void FixedUpdate () {
		if (walkableDistance <= 0) return; // check if enough fuel
		if(currentFrame!=null){
			Vector3 beforePosition = this.gameObject.transform.position;
			//reaching the corner of the frame 
			if (reachingFrame == true) {
				if(nav.navigateEuclideanTo(currentFrame.grid.indexToVector(currentFrame.topLeftCorner))==true){
					nav.stop();
					reachingFrame=false;
					searchingFrame = true;
				}
			}
			//searching the inside of the frame
			else if (searchingFrame == true){
				/* GO TO next Hop */
				if(nav.navigateEuclideanTo(enRoute.Current)==true){ /* returns true if completed, otherwise moves towards the goal*/

					/* ROUTE COMPLETED?*/
					if(enRoute.MoveNext() == false){ 
						searchingFrame = false;
						currentFrame.destroyDebug();
						//check if the cluster contains other frames
						if(enFrame.MoveNext()==true){
							setFrameToExplore (this.enFrame.Current);
						}
						else currentFrame = null;
					}
				}
			}
			walkableDistance -= Mathf.Abs(Vector3.Magnitude(this.gameObject.transform.position - beforePosition));
		}
	}

	public void assignCluster(Cluster cluster){
		/* Sort frames inside the cluster for ascending value of profit
		 * so to start searching from areas with highest probability of findings. */
		cluster.sortFramesByProfitDensity ();
		this.framesToVisit = cluster.frames;
		this.enFrame = framesToVisit.GetEnumerator ();
		this.enFrame.MoveNext ();
		setFrameToExplore (this.enFrame.Current);
	}

	private void setFrameToExplore (Frame frame){
		this.currentFrame = frame;
		reachingFrame = true;
		computeParallelSweepRoute (frame);
		enRoute = route.GetEnumerator ();
		enRoute.MoveNext ();
	}

	private void computeParallelSweepRoute(Frame frame){
		
		route = new LinkedList<Vector3>();
		Grid grid = frame.grid;
		int k=1,i=0,startingPoint = frame.topLeftCorner;
		int startingX = grid.indexToX (startingPoint), startingZ = grid.indexToZ (startingPoint);
		for(int z=0;z<=frame.zSide;z++){
			route.AddLast(grid.indexToVector(grid.xzToIndex(i+startingX,z+startingZ)));
			i+=k*frame.xSide;
			route.AddLast(grid.indexToVector(grid.xzToIndex(i+startingX,z+startingZ)));
			k*=-1;
		}
	}

/*	private void ComputeCircularCicleRoute(Frame frame){

		int longestSide, shortestSide;
		if (frame.xSide > frame.zSide) {
			longestSide = frame.xSide;
		}
				else
						shortestSide = frame.xSide;

	}*/

}
