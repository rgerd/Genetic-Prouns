using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProunGenerator : MonoBehaviour {
	public GameObject prounBlueprint;
	public bool speedUp;
	public int prounsPerGeneration = 5;
	public int maximumLifetime;
	public int elitism = 2;

	private ProunGenome[] currentGenomes;
	private ProunBuilder[] currentProuns;

	private ProunGenome[] lastGenomes;
	private float[] lastGenFitness;

	private int genNumber;
	private bool generating;
	private int alive;

	void Start () {
		lastGenomes		= new  ProunGenome [prounsPerGeneration];
		lastGenFitness	= new        float [prounsPerGeneration];
		currentGenomes	= new  ProunGenome [prounsPerGeneration];
		currentProuns	= new ProunBuilder [prounsPerGeneration];
		genNumber = 0;
		alive = prounsPerGeneration;

		for (int i = 0; i < prounsPerGeneration; i++) {
			GenGenome (i, Utility.genVector3Circle (Utility.genInt(50) + 50));
		}
	}

	private Vector3 genProunPosition() {
		return Utility.genVector3Circle (Utility.genInt (50) + 50);
	}
		
	/*
	 * Basically lays an egg with a baby Proun in it. It's like
	 * an egg because we later need to call spawn() to 'hatch' it.
	 * Note: the 'eggs' are invisible. And baby Prouns are the same
	 * as adult Prouns.
	 */
	private ProunGenome GenGenome(int index) {
		return GenGenome (index, genProunPosition ());
	}
		
	private ProunGenome GenGenome(int index, Vector3 position) {
		ProunGenome genome = InstantiateGenome (index, position);
		currentGenomes [index] = genome;
		return genome;
	}

	private ProunGenome PassGenome(int srcIndex, int destIndex) {
		return PassGenome (srcIndex, destIndex, genProunPosition ());
	}

	private ProunGenome PassGenome(int srcIndex, int destIndex, Vector3 position) {
		ProunGenome genome = InstantiateGenome (destIndex, position);
		genome.cloneGenome (lastGenomes [srcIndex]);
		currentGenomes [destIndex] = genome;
		return genome;
	}

	private ProunGenome InstantiateGenome(int index, Vector3 position) {
		GameObject obj = Instantiate (
			prounBlueprint, 
			position,
			Quaternion.identity
		);
		obj.name = "PROUN_" + genNumber + "_" + index;
		ProunGenome genome = obj.GetComponent<ProunGenome> ();
		return genome;
	}

	/*
	 * Calls spawn and registers the 'hatched' Proun as a
	 * current Proun in the generation.
	 */
	private void SpawnGenome(int index) {
		currentProuns [index] = currentGenomes [index].spawn ();
	}

	void Update () {
		if (generating) { return; }
		if(speedUp)
			Time.timeScale = 8;

		for (int i = 0; i < currentGenomes.Length; i++) {
			if (currentGenomes [i] == null)
				continue;
			
			if(!currentGenomes[i].isSpawned())
				SpawnGenome (i); // Release the proun!
			
			ProunBuilder currentProun = currentProuns [i];
			// Check if a Proun is ready to die. If they are,
			// measure their fitness and add them to the 
			// last generation.
			if (currentProun.getLifetime () >= maximumLifetime) {
				float fitness = currentProun.getFitness ();

				lastGenFitness [i] = fitness;
				lastGenomes [i] = currentGenomes[i];

				// Clean up the remains
				Destroy (currentProun.gameObject);
				currentGenomes [i] = null;
				alive--;
			}
		}

		if (alive == 0)
			NewGeneration ();
	}

	/*
	 * Selects a genome's index by tournament-style
	 * selection on the genome's fitness.
	 */
	static int ShotgunSelect(float[] fitnesses, int blastSize) {
		int maxIndex = Utility.genInt(fitnesses.Length);
		float maxFitness = fitnesses[maxIndex];

		for (int i = 1; i < blastSize; i++) {
			int randIndex = Utility.genInt(fitnesses.Length);
			float randFitness = fitnesses [randIndex];
			if (randFitness > maxFitness) {
				maxIndex = randIndex;
				maxFitness = randFitness;
			}
		}

		return maxIndex;
	}

	/*
	 * Selects the top `numTop` genome indices from
	 * a list of the genome fitnesses.
	 */
	static int[] SelectTopGenomes(float[] fitnesses, int numTop) {
		int[] top = new int[numTop];
		float[] topFit = new float[numTop];
		for(int t = 0; t < numTop; t++) {
			top [t] = -1;
			topFit [t] = -1;
			for (int i = 0; i < fitnesses.Length; i++) {
				if((t == 0) || (topFit[t - 1] > fitnesses[i])) {
					if (fitnesses [i] > topFit [t]) {
						top [t] = i;
						topFit [t] = fitnesses [i];
					}
				}
			}
		}

		return top;
	}

	/*
	 * Mates organisms from the previous generation 
	 * to create the next generation.
	 */
	private void NewGeneration() {
		Time.timeScale = 1;
		generating = true;  // Lock the update loop
		genNumber++;
		print ("Generating generation " + genNumber);

		int genomeIndex = 0;
		alive += DoElitism (lastGenFitness, ref genomeIndex);
		alive += DoMating (lastGenFitness, ref genomeIndex);

		generating = false; // Unlock the update loop
	}

	private int DoElitism(float[] lastGenFitness, ref int genomeIndex) {
		int[] topGenomeIndices = SelectTopGenomes (lastGenFitness, elitism);
		for (int i = 0; i < elitism; i++) {
			int srcIndex = topGenomeIndices [i];
			int destIndex = genomeIndex + i;
			// print ("Top fitness #" + (i + 1) + ": " + lastGenFitness [srcIndex]);
			PassGenome (srcIndex, destIndex);
		}
		genomeIndex += elitism;

		return elitism;
	}

	private int DoMating(float[] lastGenFitness, ref int genomeIndex) {
//		for (; genomeIndex < prounsPerGeneration; genomeIndex++) {
//			int suitorIndex = ShotgunSelect (lastGenFitness, 3);
//
//			int mateIndex;
//			do { mateIndex = ShotgunSelect(lastGenFitness, 3); } while (mateIndex == suitorIndex);
//
//			ProunGenome suitor = lastGenomes [suitorIndex];
//			ProunGenome mate = lastGenomes[mateIndex];
//
//			int suitorLength = suitor.getGenome ().Length;
//			int mateLength	 = mate.getGenome ().Length;
//
//			// Doing single-point crossover by splicing between 0.25 and 0.75 of the genome's length.
//			int suitorSplice = Utility.genInt (suitorLength >> 1) + (suitorLength >> 3);
//			int mateSplice	 = Utility.genInt (mateLength >> 1) + (mateLength >> 3);
//
//			// Splice the genomes to prepare for mating
//			ProunGenome.NodeGene[] splicedSuitor = suitor.spliceGenome (suitorSplice);
//			ProunGenome.NodeGene[] splicedMate	 = mate.spliceGenome (mateSplice);
//
//			// Mate the genomes to create two children. This allows the next generation to be as large as
//			// the previous one.
//			int child1Index = genomeIndex;
//			ProunGenome child1 = GenGenome (child1Index);
//			child1.setParents (splicedSuitor, -suitorSplice, splicedMate, mateSplice);
//
//			int child2Index = ++genomeIndex;
//			ProunGenome child2 = GenGenome (child2Index);
//			child2.setParents (splicedSuitor, suitorSplice, splicedMate, -mateSplice);
//
//			// Is it hot in here or is it just me?
//		}
//		// Note that we don't spawn the genomes just yet. They need to fully initialize (kind of like gestation!),
//		// so we initialize them in the update loop when they're ready.
		return 0;
	}
}
