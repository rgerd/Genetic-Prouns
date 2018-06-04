using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Need to define genome in a way that I can split it into partial/whole solutions
// Graph representation that can be sliced cleanly
// Node = (index, mass, dynamic fric, static fric)
// Connection = (push, pull, timing, connecting node) => lower always connects to higher

public class ProunGenome : MonoBehaviour {
	public int maxProunSize = 100;

	public Rigidbody[] _nodeBodies;
	public static Rigidbody[] nodeBodies;

	public GameObject _muscle;
	public static GameObject muscle;

	public Material[] _materials;
	public static Material[] materials;

	public bool spawnAutomatically = false;
	private bool spawned = false;

	private bool empty = true;
	private NodeGene[] body;
	private AdjacencyMatrix<MuscleGene> mind;

	void Start() {
		ProunGenome.nodeBodies = _nodeBodies;
		ProunGenome.muscle = _muscle;
		ProunGenome.materials = _materials;

		if (spawnAutomatically)
			spawn ();
	}

	private void GenerateRand(int maxProunSize) {
		int size = Utility.genInt (maxProunSize * 2);
		body = new NodeGene[size];
		for (int i = 0; i < size; i++)
			body [i] = new NodeGene (i);
		mind = new AdjacencyMatrix<MuscleGene> (size);
		for (int i = 0; i < size; i++) {
			for (int j = i + 1; j < size; j++) {
				if (Utility.genFloat () < 0.8) {
					mind.SetNeighbor (i, j, new MuscleGene (i, j));
				}
			}
		}
		empty = false;
	}

	public Gene[] GetGenes() {
		NodeGene[] nodeGenes = body;
		MuscleGene[] mindGenes = mind.GetContents (); // Tell me Proun, what's on your mind?
		Gene[] allGenes = new Gene[nodeGenes.Length + mindGenes.Length];
		for (int i = 0; i < nodeGenes.Length; i++)
			allGenes [i] = nodeGenes [i];
		for (int i = 0; i < mindGenes.Length; i++)
			allGenes [i + nodeGenes.Length] = mindGenes [i];

		return allGenes;
	}

	public int GetNumNodes() {
		return body.Length;
	}

	public bool isSpawned() {
		return spawned;
	}

	public ProunBuilder spawn() {
		spawned = true;
		if (empty) GenerateRand(maxProunSize);
		return gameObject.AddComponent<ProunBuilder> ();
	}

	public void cloneGenome(ProunGenome src) {
		this.body = src.body;
		this.mind = src.mind;
		empty = false;
	}
//
//	/*
//	 * Returns a spliced version of this Proun's genome 
//	 * such that there are no muscles connecting
//	 * any two nodes across the index parameter.
//	 * For example, if index is 4, and there is a muscle
//	 * connecting the node gene #1 to node gene #6,
//	 * the resulting spliced version will not include that
//	 * muscle.
//	 * The spliced genome can then more easily be used in 
//	 * reproduction with other spliced genomes.
//	 */
//	public NodeGene[] spliceGenome(int index) {
//		NodeGene[] spliced = new NodeGene[genome.Length];
//		for (int i = 0; i < genome.Length; i++) {
//			spliced [i] = genome [i];
//			spliced [i].muscles = new List<MuscleGene> ();
//			for (int j = 0; j < genome [i].muscles.Count; j++) {
//				if (genome [i].muscles [j].connectedNode < index != i < index)
//					continue; // If the muscle crosses the index, slice it
//				spliced [i].muscles.Add(genome[i].muscles[j]);
//			}
//		}
//		return spliced;
//	}
//		
//	public int maxMuscleCrossover = 5;
//	public float mutationVolatility = 0.1f;
//
//	/*
//	 * This function is responsible for mating two genomes
//	 * and populating this genome with their "child".
//	 * There are three stages: genome concatenation,
//	 * joint connection, and mutation.
//	 */
//	public void setParents(NodeGene[] p1, int s1, NodeGene[] p2, int s2) {
//		// BEGIN CONCATENTATION
//		genome = new NodeGene[p1.Length + p2.Length];
//
//		// Insert the parent genomes and get index translation tables
//		int[] index_transition_1 = insertGenome(p1, s1, genome, 0);
//		int splice_1_size = index_transition_1.Length;
//
//		int[] index_transition_2 = insertGenome(p2, s2, genome, splice_1_size);
//		int splice_2_size = index_transition_2.Length;
//
//		// Resize the child genome
//		NodeGene[] _genome = genome;
//		genome = new NodeGene[splice_1_size + splice_2_size];
//		print ("[ProunGenome.setParents] Genome length: " + (splice_1_size + splice_2_size));
//
//		// Reindex the genes
//		for(int i = 0; i < genome.Length; i++) {
//			genome [i] = _genome [i]; // Complete resize
//			NodeGene nodeGene = genome[i];
//
//			int[] translation_table = i < splice_1_size ? index_transition_1 : index_transition_2;
//
//			// Reindex node gene
//			nodeGene.index = translation_table [nodeGene.index % translation_table.Length];
//
//			// Do same for all muscles
//			for (int m = 0; m < genome [i].muscles.Count; m++) {
//				MuscleGene muscleGene = nodeGene.muscles [m];
//				int node1Index = nodeGene.index;
//				int node2Index = translation_table [muscleGene.connectedNode % translation_table.Length];
//
//				if (node1Index < node2Index) { // Totally fine
//					muscleGene.originNode = node1Index;
//					muscleGene.connectedNode = node2Index;
//				} else { // Gonna have to do a switcheroo, pass the muscle back to the lower index
//					muscleGene.originNode = node2Index;
//					muscleGene.connectedNode = node1Index;
//					nodeGene.muscles.RemoveAt(m--);
//					genome[node2Index].muscles.Add(muscleGene); // Probably doesn't work
//					continue;
//				}
//
//				nodeGene.muscles [m] = muscleGene;
//			}
//
//			genome [i] = nodeGene;
//		}
//		// END CONCATENTATION
//		if(!verifyGenome(genome, true)) 
//			return;
//		
//		// BEGIN JOINT CONNECTION
//		for (int i = 0; i < maxMuscleCrossover; i++) {
//			int node1Index = Utility.genInt (splice_1_size);
//			NodeGene node1 = genome [node1Index];
//			NodeGene node2 = genome [Utility.genInt (splice_2_size) + splice_1_size];
//
//			MuscleGene newMuscle = new MuscleGene (node1.index, node2.index);
//			newMuscle.jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
//			// node1.muscles.Add (newMuscle); // THIS IS THE LINE OF CODE THAT MESSES IT ALL UP
//			print("YOOOOO");
//			print(node1.muscles);
//
//			genome [node1Index] = node1;
//		}
//		// END JOINT CONNECTION
//		if(!verifyGenome(genome, true)) 
//			return;
//
//		// BEGIN MUTATION
//		for (int i = 0; i < genome.Length; i++) {
//			if (Utility.genFloat () < mutationVolatility) {
//				NodeGene node = genome [i];
//				if (Utility.genFloat () < 0.75) { // Overwrite a node
//					NodeGene mutNode = new NodeGene (i);
//					mutNode.muscles = node.muscles;
//					genome [i] = mutNode;
//				} else { // Overwrite a muscle
//					int muscleIndex = Utility.genInt (node.muscles.Count);
//					if (muscleIndex >= node.muscles.Count)
//						continue;
//					MuscleGene muscle = node.muscles [muscleIndex];
//					MuscleGene mutMuscle = new MuscleGene (node.index, muscle.connectedNode);
//					mutMuscle.jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
//					node.muscles [muscleIndex] = mutMuscle;
//				}
//			}
//		}
//		// END MUTATION
//		if(!verifyGenome(genome, true)) 
//			return;
//	}
//
//	/*
//	 * Inserts the spliced genome src (spliced with spliceIndex)
//	 * into the genome dest starting at index insertIndex.
//	 * Note that this function overwrites genes already in the
//	 * dest genome.
//	 * Returns a table that translates src indices to dest indices.
//	 */
//	private static int[] insertGenome(NodeGene[] src, int spliceIndex, NodeGene[] dest, int insertIndex) {
//		int start = spliceIndex < 0 ? -spliceIndex - 1 : spliceIndex;
//		int end = spliceIndex < 0 ? -1 : src.Length;
//		int inc = spliceIndex < 0 ? -1 : 1;
//		int size = Math.Abs(end - start);
//		int[] translation_table = new int[size];
//
//		for(int i = start; i != end; i += inc, insertIndex++) {
//			dest[insertIndex] = src[i];
//			translation_table[i % size] = insertIndex;
//		}
//
//		return translation_table;
//	}
//
//	/*
//	 * Looks at all of the node and muscle indices to make sure
//	 * everything is perfect for use in the real world.
//	 * If this fails, we could have big problems down the road,
//	 * so we should use this whenever we make big changes to 
//	 * a genome, like in setParents().
//	 */
//	private static bool verifyGenome(NodeGene[] g, bool failHard) {
//		for (int i = 0; i < g.Length; i++) {
//			NodeGene ng = g [i];
//			if (ng.index != i) {
//				if (failHard)
//					throw new Exception ("NodeGene index error: g[" + i + "].index = " + ng.index);
//				return false;
//			}
//			for (int m = 0; m < ng.muscles.Count; m++) {
//				MuscleGene mg = ng.muscles [m];
//				if (mg.originNode != i) {
//					if (failHard)
//						throw new Exception ("MuscleGene origin index error: g[" + i + "].mg[" + m + "].originNode = " + mg.originNode);
//					return false;
//				}
//
//				if (mg.connectedNode <= mg.originNode) {
//					if (failHard)
//						throw new Exception ("MuscleGene connection index error: g[" + i + "].mg[" + m + "].originNode = " + mg.originNode + " >= " + mg.connectedNode);
//					return false;
//				}
//
//				if (mg.connectedNode >= g.Length) {
//					if (failHard)
//						throw new Exception ("MuscleGene connection index error: g[" + i + "].mg[" + m + "].connectedNode = " + mg.connectedNode + " >= " + g.Length);
//					return false;
//				}
//			}
//		}
//		return true;
//	}
}
