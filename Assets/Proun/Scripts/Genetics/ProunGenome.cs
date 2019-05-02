using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProunGenome : MonoBehaviour {
	/* Public variables */
	public int minProunSize = 4;
	public int maxProunSize = 10;

	/* Public assets */
	private static bool builderStaticsSet = false;

	public Rigidbody[] _nodeBodies;
	public static Rigidbody[] nodeBodies;

	public GameObject _muscle;
	public static GameObject muscle;

	public Material[] _materials;
	public static Material[] materials;

	/* Object variables */
	public bool spawnAutomatically = false;
	private bool spawned = false;

	private bool empty = true;

	private string genomeID;
	private NodeGene[] body;
	private AdjacencyMatrix<MuscleGene> mind;

	private static string GenerateGID() {
		return (Utility.genUnsignedInt ()).ToString ("X");
	}

	public void SetData (GenomeData data) {
		body = data.body;

		mind = new AdjacencyMatrix<MuscleGene> (body.Length);
		MuscleGene[] muscles = data.mind;
		foreach (MuscleGene muscle in muscles) {
			mind.SetNeighbor (muscle.originNode, muscle.connectedNode, muscle);
		}

		empty = false;
	}

	public GenomeData ToGenomeData() {
		GenomeData data = new GenomeData ();
		data.genomeID = genomeID;
		data.body = body;
		data.mind = mind.GetContents ();
		return data;
	}

	void Start() {
		if (spawnAutomatically)
			Spawn ();
	}

	private void GenerateRand() {
		int size = Utility.genInt (minProunSize, maxProunSize);

		body = new NodeGene[size];
		for (int i = 0; i < size; i++)
			body [i] = new NodeGene (i);

		mind = new AdjacencyMatrix<MuscleGene> (size);
		for (int i = 0; i < size; i++) {
			int numPotentialNeighbors = size - (i + 1);
			List<int> randNeighbors = new List<int> (numPotentialNeighbors);
			for (int j = 0; j < numPotentialNeighbors; j++)
				randNeighbors.Add(j + i + 1);

			int numNeighbors = 0;
			while (randNeighbors.Count > 0 && numNeighbors < 3) {
				if (Utility.genFloat () < (4f - numNeighbors) / 4f) {
					int randIndex = Utility.genInt (randNeighbors.Count);
					int neighbor = randNeighbors [randIndex];
					randNeighbors.RemoveAt (randIndex);
					mind.SetNeighbor (i, neighbor, new MuscleGene (i, neighbor));
					numNeighbors++;
				} else {
					break;
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

    public int GetMindSize() {
        return mind.GetContents().Length;
    }

	public bool IsSpawned() {
		return spawned;
	}

	public ProunBuilder Spawn() {
		if (!ProunGenome.builderStaticsSet) {
			ProunGenome.nodeBodies = _nodeBodies;
			ProunGenome.muscle = _muscle;
			ProunGenome.materials = _materials;
		}

		spawned = true;
		if (empty) GenerateRand();

		if (genomeID == null)
			genomeID = GenerateGID ();
		return gameObject.AddComponent<ProunBuilder> ();
	}

	public void CloneGenome(ProunGenome src) {
		this.body = src.body;
		this.mind = src.mind;
		empty = false;
	}


	/* p2 fitness > p1fitness */
	public void SetParents(
		ProunGenome p1, 
		ProunGenome p2, 
		float p1Fitness, 
		float p2Fitness, 
		MutationParams mutationParams) 
	{
		int p1Size = p1.body.Length;
		int p2Size = p2.body.Length;

		int smallerSize = p2Size > p1Size ? p1Size : p2Size;
		int largerSize  = p1Size > p2Size ? p1Size : p2Size;

		float fitnessRatio = p2Fitness / (p1Fitness + p2Fitness);

		int newSize = 0;

		NodeGene[] _newBody = new NodeGene[largerSize];

		for (int i = 0; i < smallerSize; i++, newSize++)
			_newBody [i] = Utility.genFloat () < fitnessRatio ? p2.body [i] : p1.body [i];

		if (p2Size > p1Size) {
			for (int i = smallerSize; i < largerSize; i++, newSize++)
				_newBody [i] = p2.body [i];
		}

		NodeGene[] newBody = new NodeGene[newSize];
		for (int i = 0; i < newSize; i++) {
			newBody [i] = _newBody [i];
			newBody [i].index = i;
		}

		AdjacencyMatrix<MuscleGene> newMind = new AdjacencyMatrix<MuscleGene> (newSize);

		for (int i = 0; i < smallerSize; i++) {
			for (int j = 0; j < smallerSize; j++) {
				MuscleGene muscleGene = p1.mind.GetNeighbor (i, j);
				if (muscleGene != null) {
					newMind.SetNeighbor (i, j, muscleGene.Disable());
				}
			}
		}

		for (int i = 0; i < p2Size; i++) {
			for (int j = 0; j < p2Size; j++) {
				MuscleGene muscleGene = p2.mind.GetNeighbor (i, j);
				if (muscleGene != null) {
					newMind.SetNeighbor (i, j, (MuscleGene) muscleGene.Mutate(mutationParams.muscleGeneMutationParams));
				}
			}
		}

		this.body = newBody;
		this.mind = newMind;
	}
}