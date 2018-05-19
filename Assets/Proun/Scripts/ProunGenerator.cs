using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProunGenerator : MonoBehaviour {
	public GameObject proun;
	public int prounsPerGeneration;
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
			genGenome (i, Utility.genVector3Circle (Utility.genInt(50) + 50));
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
	private ProunGenome genGenome(int index) {
		return genGenome (index, genProunPosition ());
	}
		
	private ProunGenome genGenome(int index, Vector3 position) {
		GameObject obj = Instantiate (
			proun, 
			position,
			Quaternion.identity
		);
		obj.name = "PROUN_" + genNumber + "_" + index;
		ProunGenome genome = obj.GetComponent<ProunGenome> ();
		currentGenomes [index] = genome;
		return genome;
	}

	private ProunGenome passGenome(int srcIndex, int destIndex) {
		return passGenome (srcIndex, destIndex, genProunPosition ());
	}

	private ProunGenome passGenome(int srcIndex, int destIndex, Vector3 position) {
		GameObject obj = Instantiate (
			proun, 
			position,
			Quaternion.identity
		);
		obj.name = "PROUN_" + genNumber + "_" + destIndex;
		ProunGenome genome = obj.GetComponent<ProunGenome> ();
		genome.cloneGenome (lastGenomes [srcIndex]);
		currentGenomes [destIndex] = genome;
		return genome;
	}

	/*
	 * Calls spawn and registers the 'hatched' Proun as a
	 * current Proun in the generation.
	 */
	private void spawnGenome(int index) {
		currentProuns [index] = currentGenomes [index].spawn ();
	}

	void Update () {
		if (generating) { return; }

		for (int i = 0; i < currentGenomes.Length; i++) {
			if (currentGenomes [i] == null)
				continue;
			
			if(!currentGenomes[i].isSpawned())
				spawnGenome (i); // Release the proun!
			
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
			newGeneration ();
	}

	/*
	 * Selects a genome's index by tournament-style
	 * selection on the genome's fitness.
	 */
	int shotgunSelect(float[] fitnesses, int blastSize) {
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

	int[] selectTopGenomes(float[] fitnesses, int numTop) {
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
	void newGeneration() {
//		Time.timeScale = 1;
		generating = true;  // Lock the update loop
		genNumber++;
		alive = prounsPerGeneration;
		print ("Generating generation " + genNumber);

		int[] topGenomeIndices = selectTopGenomes (lastGenFitness, elitism);
		for (int i = 0; i < elitism; i++) {
			print ("Top fitness " + i + ": " + lastGenFitness [i]);
			passGenome (topGenomeIndices [i], i);
		}
			
		for (int genomeIndex = elitism; genomeIndex < prounsPerGeneration; genomeIndex++) {
			int suitorIndex = shotgunSelect (lastGenFitness, 3);
			ProunGenome suitor = lastGenomes [suitorIndex];

			int mateIndex;
			do { mateIndex = shotgunSelect(lastGenFitness, 3); } while (mateIndex == suitorIndex);
			ProunGenome mate = lastGenomes[mateIndex];

			int suitorLength = suitor.getGenome ().Length;
			int mateLength	 = mate.getGenome ().Length;

			// Doing single-point crossover by splicing between 0.25 and 0.75 of the genome's length.
			int suitorSplice = Utility.genInt (suitorLength >> 1) + (suitorLength >> 3);
			int mateSplice	 = Utility.genInt (mateLength >> 1) + (mateLength >> 3);

			// Splice the genomes to prepare for mating
			ProunGenome.NodeGene[] splicedSuitor = suitor.spliceGenome (suitorSplice);
			ProunGenome.NodeGene[] splicedMate	 = mate.spliceGenome (mateSplice);

			// Mate the genomes to create two children. This allows the next generation to be as large as
			// the previous one.
			int child1Index = genomeIndex;
			ProunGenome child1 = genGenome (child1Index);
			child1.setParents (splicedSuitor, -suitorSplice, splicedMate, mateSplice);

			int child2Index = ++genomeIndex;
			ProunGenome child2 = genGenome (child2Index);
			child2.setParents (splicedSuitor, suitorSplice, splicedMate, -mateSplice);

			// Is it hot in here or is it just me?
		}
		// Note that we don't spawn the genomes just yet. They need to fully initialize (kind of like gestation!),
		// so we initialize them in the update loop when they're ready.

		generating = false; // Unlock the update loop
//		Time.timeScale = 8;
	}
}
