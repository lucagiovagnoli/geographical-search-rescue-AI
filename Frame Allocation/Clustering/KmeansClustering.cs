using UnityEngine;
using System;

public class KmeansClustering{

	private int K;
	private Entity[] dataset;
	public static System.Random rnd = new System.Random();

	public class Entity{
		
		private float[] features;
		public int Dimensions {
			get {
				return features.Length;
			}
		}
		
		public Entity (int dim){
			this.features = new float[dim]; 
		}
		
		public Entity(float[] coordinates){
			this.features = coordinates;
			Debug.Log(Mathf.Pow(5,3));
		}

		public void setIthFeature(int i, float value){
			this.features [i] = value;
		}

		public void zeros(){
			for (int i=0; i<this.Dimensions; i++) {
				this.features[i] = 0.0f;
			}
		}
		
		public float computeSquareEuclideanDistance(Entity other){
			float distance=0;
			for (int i=0;i<features.Length;i++){
				distance += Mathf.Pow(this.features[i]-other.features[i],2);
			}
//			return Mathf.Sqrt(distance);
			return distance;
		}

		public static Entity operator +(Entity e1, Entity e2) {
			if (e1.Dimensions != e2.Dimensions) return null;

			Entity newE = new Entity (e1.Dimensions);
			for (int i=0; i<e1.Dimensions; i++) {
				newE.features[i] = e1.features[i]+e2.features[i];
			}
			return newE;
		}
		public static Entity operator /(Entity e1, float number){
			for (int i=0; i<e1.Dimensions; i++) {
				e1.features[i] /= number;
			}
			return e1;
		}

		public override bool Equals(System.Object other){
			// If parameter is null return false.
			if (other == null) return false;
			
			for (int i=0; i<Dimensions; i++) {
				if(this.features[i] != ((Entity)other).features[i]) return false;
			}
			return true;
		}

		public override string ToString (){

			String str =  "Dimensions: "+ Dimensions +" - features: ";
			for (int i=0;i<Dimensions;i++){
				str+= features[i]+" ";
 			}
			return str;
		}
	}
	
	public class ClusteringSolution: IComparable<ClusteringSolution>{
		
		public int[] mapDataPointToCluster{ get; set;}
		public int finalK{ get; set; }
		private float WCSS = 0;

		public ClusteringSolution(int NdataSet, int finalK){
			mapDataPointToCluster = new int[NdataSet];
			this.finalK = finalK;
		}

		/* Minimize the within-cluster sum of squares (WCSS) */
		public float computeSolutionWCSS(Entity[] dataset, Entity[] centroids){
			//compute WCSS
			this.WCSS = 0;
			for (int i=0; i<dataset.Length; i++) {
				this.WCSS += dataset [i].computeSquareEuclideanDistance (centroids [mapDataPointToCluster [i]]);
			}
			return WCSS;
		}

		#region IComparable implementation
		public int CompareTo (ClusteringSolution other)	{
			return (int) (this.WCSS - other.WCSS);
		}
		#endregion

	}

	/* Entity[] is the dataset of entities, K the number of desired clusters */
	public KmeansClustering(Entity[] dataset, int K){
	
		this.dataset = dataset;

		//handle the case when number of data points is less than K number of clusters 
		if (dataset.Length >= K) this.K = K;
		else this.K = dataset.Length;

	}

	/* run the algorithm with centroid random start  */
	public ClusteringSolution runKmeans (int randomStarts = 1, int maxIterations = 10000){

		ClusteringSolution currentSol, bestSol = null; 

		for (int i=0; i<randomStarts; i++) {

			currentSol = new ClusteringSolution(dataset.Length,this.K);
			Entity[] centroids = initCentroids ();
			//foreach (Entity e in centroids) Debug.Log ("Beginning - centroid: "+e);
			int iter=0;
			do {
				computeClusters (currentSol, centroids);
				iter++;
			} while(computeCentroids(currentSol, ref centroids)==true && iter<maxIterations);
			//Debug.Log("Iterations:"+iter);
			//foreach (Entity e in centroids) Debug.Log ("End - Centroid: "+e);

			currentSol.computeSolutionWCSS(this.dataset,centroids);

			if (i==0) bestSol = currentSol;
			else if(currentSol.CompareTo(bestSol) < 0) bestSol = currentSol;
		
		}
		return bestSol;
	}

	/* run the algorithm with user input start for centroids */
	public ClusteringSolution runKmeans (Entity[] userInputCentroids, int maxIterations = 1000){
		
		ClusteringSolution solution = new ClusteringSolution(dataset.Length,this.K);

		foreach (Entity e in userInputCentroids) Debug.Log ("Centroid: "+e);
		int iter=0;
		do {
			computeClusters (solution, userInputCentroids);
			iter++;
		} while(computeCentroids(solution, ref userInputCentroids)==true && iter<maxIterations);
						
		return solution;
	}

	/* Define the initial groups' centroids. This step can be done using different strategies. 
	 * A very common one is to assign random values for the centroids of all groups. 
	 * Another approach is to use the values of K different entities as being the centroids. */
	private Entity[] initCentroids(){

		Entity[] centroids = new Entity[this.K]; // K clusters, K centroids
		bool[] chosen = new bool[dataset.Length];

		for (int i=0; i<K; i++) {
			int randomNumber = rnd.Next(0,dataset.Length);

			while(chosen[randomNumber] == true) {
				randomNumber = rnd.Next(0,dataset.Length);
			}
			chosen[randomNumber] = true;
			centroids[i]=this.dataset[randomNumber]; 
		}
		return centroids;
	}
	
	public void computeClusters(ClusteringSolution sol, Entity[] centroids){

		float currentDistance, minDistance;

		// scan through the dataset
		for (int i=0;i<dataset.Length;i++){
			minDistance=float.MaxValue;
			// scan through the centroids
			for (int z=0;z<centroids.Length;z++){
				currentDistance = dataset[i].computeSquareEuclideanDistance(centroids[z]);	
				if (currentDistance < minDistance) {
					minDistance = currentDistance;
					sol.mapDataPointToCluster[i] = z;
				}
			}
		}
	}

	/* returns true if the centroids changed */
	private bool computeCentroids(ClusteringSolution sol, ref Entity[] oldCentroids){

		int[] counters = new int[K];
		Entity[] temp = new Entity[K]; // temp variable for new centroids

		for (int i=0;i<K;i++) {
			temp[i] = new Entity(oldCentroids[0].Dimensions);		
		}

		for(int i=0;i<dataset.Length;i++){
			temp[sol.mapDataPointToCluster[i]] = temp[sol.mapDataPointToCluster[i]] + dataset[i]; // clear
			counters[sol.mapDataPointToCluster[i]]++;
		}
		for(int i=0;i<dataset.Length;i++){
			temp[sol.mapDataPointToCluster[i]] = temp[sol.mapDataPointToCluster[i]] / counters[sol.mapDataPointToCluster[i]];
		}

		for (int i=0;i<K;i++) {
			if(oldCentroids[i].Equals(temp[i]) == false) { //one at least changed
				oldCentroids = temp; // update centroids
				return true;
			}
		}
		//nothing changed, return false
		return false;
	}
				

	public static alglib.kmeansreport computeClusters(double[,] xy, int K){

		//
		// The very simple clusterization example
		//
		// We have a set of points in 2D space:
		//     (P0,P1,P2,P3,P4) = ((1,1),(1,2),(4,1),(2,3),(4,1.5))
		//
		//  |
		//  |     P3
		//  |
		//  | P1          
		//  |             P4
		//  | P0          P2
		//  |-------------------------
		//
		// We want to perform k-means++ clustering with K=2.
		//
		// In order to do that, we:
		// * create clusterizer with clusterizercreate()
		// * set points XY and metric (must be Euclidean, distype=2) with clusterizersetpoints()
		// * (optional) set number of restarts from random positions to 5
		// * run k-means algorithm with clusterizerrunkmeans()
		//
		// You may see that clusterization itself is a minor part of the example,
		// most of which is dominated by comments :)
		//
		alglib.clusterizerstate s;
		alglib.kmeansreport rep;
		
		alglib.clusterizercreate(out s);
		alglib.clusterizersetpoints(s, xy, 2);
		alglib.clusterizersetkmeanslimits(s, 5, 0);
		alglib.clusterizerrunkmeans(s, K, out rep);

		//
		// We've performed clusterization, and it succeeded (completion code is +1).
		//
		// Now first center is stored in the first row of rep.c, second one is stored
		// in the second row. rep.cidx can be used to determine which center is
		// closest to some specific point of the dataset.
		//
		System.Console.WriteLine("{0}", rep.terminationtype); // EXPECTED: 1
		
		return rep;

		// We called clusterizersetpoints() with disttype=2 because k-means++
		// algorithm does NOT support metrics other than Euclidean. But what if we
		// try to use some other metric?
		//
		// We change metric type by calling clusterizersetpoints() one more time,
		// and try to run k-means algo again. It fails.
		//
//		alglib.clusterizersetpoints(s, xy, 0);
//		alglib.clusterizerrunkmeans(s, 2, out rep);
//		System.Console.WriteLine("{0}", rep.terminationtype); // EXPECTED: -5
//		System.Console.ReadLine();
			
	}
}
	
	