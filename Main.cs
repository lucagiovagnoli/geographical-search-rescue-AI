using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	public Grid grid;
	private UAV[] UAVs; 

	//Frame Manager singleton pattern 
	private FrameManager hydra = FrameManager.Instance; //hydra name is taken from a research paper

	private bool firstTaken=false;
	private MouseInput mouse;
	private Vector3 mousePoint;

	// Use this for initialization
	void Start () {
		mouse = new MouseInputPhysics(MouseInput.clickType.middle);
		GameObject[] temp = GameObject.FindGameObjectsWithTag("uav");
		this.UAVs = new UAV[temp.Length];
		for (int i=0; i<temp.Length; i++) {
			this.UAVs [i] = temp [i].GetComponent<UAV> ();
		}
	
		hydra.grid = grid;
		hydra.UAVs = this.UAVs;
	}
	
	// Update is called once per frame
	void Update () {

		if(mouse.isMouseInputReady()) {
			if(firstTaken==false){
				firstTaken = true;
				mousePoint = mouse.getOutput();
			}
			else{
				firstTaken=false;
				Vector3 temp = mousePoint;
				mousePoint = mouse.getOutput();

				// computes the real top left corner and bottom right corner of the frame
				// from the 2 clicked points on the map.
				Vector3[] corners = getTopLeftBottonRightCorners(temp,mousePoint);
				// create frame
				Frame frame = new Frame(grid,corners[0],corners[1],1); 
				frame.displayDebug(Color.red);
				hydra.addArea(frame);
			}
		}
	}

	private Vector3[] getTopLeftBottonRightCorners (Vector3 firstPoint, Vector3 secondPoint){

		Vector3[] res = new Vector3[2];

		if (firstPoint.x < secondPoint.x && firstPoint.z > secondPoint.z) {
			res [0] = firstPoint;
			res [1] = secondPoint;
		} 
		else if (firstPoint.x > secondPoint.x && firstPoint.z < secondPoint.z) {
			res [0] = secondPoint;
			res [1] = firstPoint;
		}
		else if (firstPoint.x < secondPoint.x && firstPoint.z < secondPoint.z){
			res[0] = new Vector3(firstPoint.x,0.0f,secondPoint.z);
			res[1] = new Vector3(secondPoint.x,0.0f,firstPoint.z);
		}		
		else if (firstPoint.x > secondPoint.x && firstPoint.z > secondPoint.z){
			res[0] = new Vector3(secondPoint.x,0.0f,firstPoint.z);
			res[1] = new Vector3(firstPoint.x,0.0f,secondPoint.z);
       	}
		return res;
	}

	void OnGUI(){
		
		GUI.Box (new Rect (10,10,200,150), "Menu");
		
		if (GUI.Button (new Rect (30, 30, 100, 30), "Go")) {
			hydra.go();
		}


	}
}






