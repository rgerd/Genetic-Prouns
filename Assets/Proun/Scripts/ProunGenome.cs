using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Need to define genome in a way that I can split it into partial/whole solutions
// Graph representation that can be sliced cleanly
// Node = (index, mass, dynamic fric, static fric)
// Connection = (push, pull, timing, connecting node) => lower always connects to higher

public class ProunGenome : MonoBehaviour {
	public enum JointType {
		SPRING_JOINT,
		NUM_JOINTS, // Always keep at end, everything underneath is hidden
		FIXED_JOINT,
		HINGE_JOINT,
	};

	public struct MuscleGene {
		public int originNode;
		public int connectedNode;

		public JointType jointType;

		public float heartBeat;
		public float contractTime;
		public float contractedLength;
		public float extensionDistance;

		public MuscleGene(int _originNode, int _connectedNode) {
			if(_originNode >= _connectedNode)
				throw new Exception("Cannot connect a muscle from " + _originNode + " to " + _connectedNode + "!");

			originNode = _originNode;
			connectedNode = _connectedNode;

			heartBeat = Utility.genFloat(0.8f, 2f);
			contractTime = Utility.genFloat(0.25f, 0.75f);
			contractedLength = 0.0f;
			extensionDistance = Utility.genFloat(0.5f, 1.5f);
			jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
		}
	}

	public struct NodeGene {
		public int index;
		public int body_type;
		public float mass;
		public float d_friction;
		public float s_friction;
		public int material;
		public Vector3 scale;
		public List<MuscleGene> muscles;

		public NodeGene(int _index) {
			index = _index;
			body_type = Utility.genInt(ProunGenome.nodeBodies.Length);
			mass = Utility.genFloat(0.3f, 0.9f);
			d_friction = Utility.genFloat(0.2f, 0.8f);
			s_friction = Utility.genFloat(0.2f, 0.8f);
			scale = new Vector3(Utility.genFloat(), Utility.genFloat(), Utility.genFloat()) * 5; 
			material = Utility.genInt(3);
			muscles = new List<MuscleGene> ();
		}
	}

	public int maxProunSize = 100;

	public Rigidbody[] _nodeBodies;
	public static Rigidbody[] nodeBodies;

	public GameObject _muscle;
	public static GameObject muscle;

	public Material[] _materials;
	public static Material[] materials;

	public bool spawnAutomatically = false;
	private bool spawned = false;

	private NodeGene[] genome;

	void Start() {
		ProunGenome.nodeBodies = _nodeBodies;
		ProunGenome.muscle = _muscle;
		ProunGenome.materials = _materials;

		if (spawnAutomatically) {
			spawn ();
		}
	}

	private NodeGene[] generateWorm(int size) {
		NodeGene[] genome = new NodeGene[size];


		for (int i = 0; i < size; i++) {
			genome [i] = new NodeGene (i);
			if (i == 0) {
				for (int j = 1; j < size; j++) {
					genome [i].muscles.Add (new MuscleGene (i, j));
				}
			}
		}

		return genome;
	}

	private NodeGene[] generateWheel(int size) {
		NodeGene[] genome = new NodeGene[size];
		return genome;
	}

	private NodeGene[] generateSkirt(int size) {
		NodeGene[] genome = new NodeGene[size];
		return genome;
	}

	private NodeGene[] generateClique(int size) {
		NodeGene[] genome = new NodeGene[size];

		for (int i = 0; i < size; i++)
			genome [i] = new NodeGene (i);

		for (int i = 0; i < size; i++) {
			for (int j = i + 1; j < size && j < i + 4; j++) {
				genome [i].muscles.Add (new MuscleGene (i, j));
			}
		}

		return genome;
	}

	public NodeGene[] getGenome() {
		return this.genome;
	}

	public string getGenomeString() {
		string s = "";
		for (int i = 0; i < genome.Length; i++) {
			s += genome [i].mass + ", ";
		}
		return s;
	}

	private static void printGenome(NodeGene[] g) {
		print("--------------");
		for (int i = 0; i < g.Length; i++) {
			print (g[i].index + ":\t" + g [i].mass);
			for(int m = 0; m < g[i].muscles.Count; m++) {
				print ("\t" + g[i].muscles[m].originNode + " -> " + g[i].muscles[m].connectedNode);
			}
		}
		print("--------------");
	}

	public bool isSpawned() {
		return spawned;
	}

	public ProunBuilder spawn() {
		spawned = true;
		if (genome == null)
			genome = generateWorm(maxProunSize);
		return gameObject.AddComponent<ProunBuilder> ();
	}

	public void cloneGenome(ProunGenome src) {
		this.genome = src.genome;
	}

	/*
	 * Returns a spliced version of this Proun's genome 
	 * such that there are no muscles connecting
	 * any two nodes across the index parameter.
	 * For example, if index is 4, and there is a muscle
	 * connecting the node gene #1 to node gene #6,
	 * the resulting spliced version will not include that
	 * muscle.
	 * The spliced genome can then more easily be used in 
	 * reproduction with other spliced genomes.
	 */
	public NodeGene[] spliceGenome(int index) {
		NodeGene[] spliced = new NodeGene[genome.Length];
		for (int i = 0; i < genome.Length; i++) {
			spliced [i] = genome [i];
			spliced [i].muscles = new List<MuscleGene> ();
			for (int j = 0; j < genome [i].muscles.Count; j++) {
				if (genome [i].muscles [j].connectedNode < index != i < index)
					continue; // If the muscle crosses the index, slice it
				spliced [i].muscles.Add(genome[i].muscles[j]);
			}
		}
		return spliced;
	}
		
	public int maxMuscleCrossover = 5;
	public float mutationVolatility = 0.1f;

	/*
	 * This function is responsible for mating two genomes
	 * and populating this genome with their "child".
	 * There are three stages: genome concatenation,
	 * joint connection, and mutation.
	 */
	public void setParents(NodeGene[] p1, int s1, NodeGene[] p2, int s2) {
		// BEGIN CONCATENTATION
		genome = new NodeGene[p1.Length + p2.Length];

		// Insert the parent genomes and get index translation tables
		int[] index_transition_1 = insertGenome(p1, s1, genome, 0);
		int splice_1_size = index_transition_1.Length;

		int[] index_transition_2 = insertGenome(p2, s2, genome, splice_1_size);
		int splice_2_size = index_transition_2.Length;

		// Resize the child genome
		NodeGene[] _genome = genome;
		genome = new NodeGene[splice_1_size + splice_2_size];
		print ("[ProunGenome.setParents] Genome length: " + (splice_1_size + splice_2_size));

		// Reindex the genes
		for(int i = 0; i < genome.Length; i++) {
			genome [i] = _genome [i]; // Complete resize
			NodeGene nodeGene = genome[i];

			int[] translation_table = i < splice_1_size ? index_transition_1 : index_transition_2;

			// Reindex node gene
			nodeGene.index = translation_table [nodeGene.index % translation_table.Length];

			// Do same for all muscles
			for (int m = 0; m < genome [i].muscles.Count; m++) {
				MuscleGene muscleGene = nodeGene.muscles [m];
				int node1Index = nodeGene.index;
				int node2Index = translation_table [muscleGene.connectedNode % translation_table.Length];

				if (node1Index < node2Index) { // Totally fine
					muscleGene.originNode = node1Index;
					muscleGene.connectedNode = node2Index;
				} else { // Gonna have to do a switcheroo, pass the muscle back to the lower index
					muscleGene.originNode = node2Index;
					muscleGene.connectedNode = node1Index;
					nodeGene.muscles.RemoveAt(m--);
					genome[node2Index].muscles.Add(muscleGene); // Probably doesn't work
					continue;
				}

				nodeGene.muscles [m] = muscleGene;
			}

			genome [i] = nodeGene;
		}
		// END CONCATENTATION
		if(!verifyGenome(genome, true)) 
			return;
		
		// BEGIN JOINT CONNECTION
		for (int i = 0; i < maxMuscleCrossover; i++) {
			int node1Index = Utility.genInt (splice_1_size);
			NodeGene node1 = genome [node1Index];
			NodeGene node2 = genome [Utility.genInt (splice_2_size) + splice_1_size];

			MuscleGene newMuscle = new MuscleGene (node1.index, node2.index);
			newMuscle.jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
			// node1.muscles.Add (newMuscle); // THIS IS THE LINE OF CODE THAT MESSES IT ALL UP
			print("YOOOOO");
			print(node1.muscles);

			genome [node1Index] = node1;
		}
		// END JOINT CONNECTION
		if(!verifyGenome(genome, true)) 
			return;

		// BEGIN MUTATION
		for (int i = 0; i < genome.Length; i++) {
			if (Utility.genFloat () < mutationVolatility) {
				NodeGene node = genome [i];
				if (Utility.genFloat () < 0.75) { // Overwrite a node
					NodeGene mutNode = new NodeGene (i);
					mutNode.muscles = node.muscles;
					genome [i] = mutNode;
				} else { // Overwrite a muscle
					int muscleIndex = Utility.genInt (node.muscles.Count);
					if (muscleIndex >= node.muscles.Count)
						continue;
					MuscleGene muscle = node.muscles [muscleIndex];
					MuscleGene mutMuscle = new MuscleGene (node.index, muscle.connectedNode);
					mutMuscle.jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
					node.muscles [muscleIndex] = mutMuscle;
				}
			}
		}
		// END MUTATION
		if(!verifyGenome(genome, true)) 
			return;
	}

	/*
	 * Inserts the spliced genome src (spliced with spliceIndex)
	 * into the genome dest starting at index insertIndex.
	 * Note that this function overwrites genes already in the
	 * dest genome.
	 * Returns a table that translates src indices to dest indices.
	 */
	private static int[] insertGenome(NodeGene[] src, int spliceIndex, NodeGene[] dest, int insertIndex) {
		int start = spliceIndex < 0 ? -spliceIndex - 1 : spliceIndex;
		int end = spliceIndex < 0 ? -1 : src.Length;
		int inc = spliceIndex < 0 ? -1 : 1;
		int size = Math.Abs(end - start);
		int[] translation_table = new int[size];

		for(int i = start; i != end; i += inc, insertIndex++) {
			dest[insertIndex] = src[i];
			translation_table[i % size] = insertIndex;
		}

		return translation_table;
	}

	/*
	 * Looks at all of the node and muscle indices to make sure
	 * everything is perfect for use in the real world.
	 * If this fails, we could have big problems down the road,
	 * so we should use this whenever we make big changes to 
	 * a genome, like in setParents().
	 */
	private static bool verifyGenome(NodeGene[] g, bool failHard) {
		for (int i = 0; i < g.Length; i++) {
			NodeGene ng = g [i];
			if (ng.index != i) {
				if (failHard)
					throw new Exception ("NodeGene index error: g[" + i + "].index = " + ng.index);
				return false;
			}
			for (int m = 0; m < ng.muscles.Count; m++) {
				MuscleGene mg = ng.muscles [m];
				if (mg.originNode != i) {
					if (failHard)
						throw new Exception ("MuscleGene origin index error: g[" + i + "].mg[" + m + "].originNode = " + mg.originNode);
					return false;
				}

				if (mg.connectedNode <= mg.originNode) {
					if (failHard)
						throw new Exception ("MuscleGene connection index error: g[" + i + "].mg[" + m + "].originNode = " + mg.originNode + " >= " + mg.connectedNode);
					return false;
				}

				if (mg.connectedNode >= g.Length) {
					if (failHard)
						throw new Exception ("MuscleGene connection index error: g[" + i + "].mg[" + m + "].connectedNode = " + mg.connectedNode + " >= " + g.Length);
					return false;
				}
			}
		}
		return true;
	}
}
