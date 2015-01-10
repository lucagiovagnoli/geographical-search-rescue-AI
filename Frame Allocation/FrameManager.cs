using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrameManager  {

	/* Singleton Pattern */
	private static FrameManager instance=null;
	//private constructor
	private FrameManager(){}
	public static FrameManager Instance { //get the singleton instance
		get {
			if (instance == null) instance = new FrameManager();
			return instance;
		}
	}

	//UAV variables
	public UAV[] UAVs { get; set;}
	private LinkedList<Frame> areas = new LinkedList<Frame>(); 

	public Grid grid{ get;set;}

	public void addArea(Frame area){
		areas.AddLast(area);
		grid.addFrameProfits (area);
	}

	// debug variables
	private static Color[] colorList = new Color[] {Color.red,Color.cyan,Color.green,Color.magenta,Color.yellow};
	private static int currentColor = 0;

	public List<Frame> computeSearchFrames(){
		List<Frame> framesToSearch = grid.getLikelyFrames (0);
	//	framesToSearch.Sort (new frameComparatorByProfit());
		foreach (Frame f in areas) f.destroyDebug();
		return framesToSearch;
	}
	
	/* K MEANS CLUSTERING ALGORITHM - ALGLIB */
	public Cluster[] computeClustersWithAlgLib(List<Frame> framesToSearch){

		int i, K = this.UAVs.Length;
		Cluster[] clustersArray = new Cluster[K];
		for (i=0; i<K; i++)
			clustersArray [i] = new Cluster ();
		
		// array of points (frames' centers used in the clustering algorithm)
		double[,] dataset = new double[framesToSearch.Count, 2]; //{{1,1},{1,2},{4,1},{2,3},{4,1.5}};

		i = 0;
		foreach (Frame f in framesToSearch) {
			dataset[i,0] = f.center.x;
			dataset[i,1] = f.center.z; i++;
		}

		alglib.kmeansreport rep = KmeansClustering.computeClusters (dataset, K);

		i=0;
		foreach (Frame f in framesToSearch) {
		//	Debug.Log(dataset[i,0] + "," + dataset[i,1] + " - cluster:" + rep.cidx[i]);
			clustersArray[rep.cidx[i]].addFrame(f); 
			i++;
		}

		foreach (Cluster c in clustersArray) {
			c.debugCentroid();
			foreach (Frame f in c.frames){
				f.displayDebug (colorList[currentColor], f.profitDensity);
			}
			currentColor++;currentColor%=colorList.Length;
		}

		return clustersArray;
	}

	/* K MEANS CLUSTERING ALGORITHM - My implementation */
	public Cluster[] computeClusters(List<Frame> framesToSearch){

		KmeansClustering.Entity[] dataset = new KmeansClustering.Entity[framesToSearch.Count];
		int i, K = this.UAVs.Length;

		i = 0;
		foreach (Frame f in framesToSearch) {
			dataset[i] = new KmeansClustering.Entity(2);
			dataset[i].setIthFeature(0,f.center.x);
			dataset[i].setIthFeature(1,f.center.z);
			//	Debug.Log(dataset[i]);
			i++;
		}

		/* dataset = center of frames
		 * K = number of UAVS */
		KmeansClustering clusterer = new KmeansClustering (dataset, this.UAVs.Length);
		KmeansClustering.ClusteringSolution sol = clusterer.runKmeans (5); // 10 random starts

		//final number of clusters K could be less than number of UAV if entities are less than K. 
		Cluster[] clustersArray = new Cluster[sol.finalK]; 
		for (i=0; i<sol.finalK; i++)
			clustersArray [i] = new Cluster ();
		
		i=0;
		foreach (Frame f in framesToSearch) {
			//Debug.Log ("Point: "+dataset[i]+" - Cluster: " +sol.mapDataPointToCluster[i]);
			clustersArray[sol.mapDataPointToCluster[i]].addFrame(f); 
			i++;
		}
		
		foreach (Cluster c in clustersArray) {
			c.debugCentroid();
			foreach (Frame f in c.frames){
				f.displayDebug (colorList[currentColor], f.profitDensity);
			}
			currentColor++;currentColor%=colorList.Length;
		}
		
		return clustersArray;
	}

	/* Assignment problem - HUNGARIAN ALGORITHM */
	public void assignClustersToDrones(Cluster[] clusters) {
		
		/* compute matrix dimensions with dummy rows and columns */
		int N = UAVs.Length;
		int[,] costs = new int[N,N]; //{{20,22,14,24},{20,19,12,20},{13,10,18,16},{22,23,9,28}};
		
		/*compute matrix costs*/
		for (int m=0; m<N; m++) {
			for (int n=0; n<N; n++) {
				if(m<UAVs.Length && n<clusters.Length) 
					costs[m,n] = (int) Mathf.Floor(Vector3.Magnitude(UAVs[m].transform.position-(clusters[n]).Centroid));
				else costs[m,n] = int.MaxValue;
			}
		}
		
		/*Munkres - Hungarian Algorithm*/
		int [] res = HungarianAlgorithm.FindAssignments (costs);
		
		/*Assign clusters to UAVs*/
		for (int i=0;i<UAVs.Length;i++) {
			if(res[i]<clusters.Length) UAVs[i].assignCluster(clusters[res[i]]);
		}
	}

	public void go(){

		List<Frame> framesToSearch = computeSearchFrames ();
		Cluster[] clustersArray = computeClusters (framesToSearch);
		//Cluster[] clustersArray = computeClustersWithAlgLib (framesToSearch);
		assignClustersToDrones (clustersArray);	
	}
}
