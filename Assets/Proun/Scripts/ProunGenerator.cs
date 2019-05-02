using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ProunGenerator : MonoBehaviour
{
    public GameObject prounBlueprint;
    public bool speedUp;
    public float speedUpAmount = 8;
    public new Camera camera;
    public int prounsPerGeneration = 5;
    public int maximumGenerations = 0; // 0 = unlimited
    public int maximumLifetime;
    public int elitism = 2;
    public bool shouldLogElites;
    public float saveFitnessCutoff = 1200;
    public string saveFileName;
    public bool shouldSaveElites;
    public bool shouldLoadFromFile;
    public MutationParams mutationParameters;

    private ProunGenome[] currentGenomes;
    private ProunBuilder[] currentProuns;

    private ProunGenome[] lastGenomes;
    private float[] lastGenFitness;
    private int[] lastGenFlukes;

    private int genNumber = 1;
    private bool generating;
    private int alive;

    private float eliteFitnessAverage;

    public static int prounMaximumLifetime;

    void Start()
    {
        prounMaximumLifetime = maximumLifetime;

        lastGenomes = new ProunGenome[prounsPerGeneration];
        lastGenFitness = new float[prounsPerGeneration];
        lastGenFlukes = new int[prounsPerGeneration];
        currentGenomes = new ProunGenome[prounsPerGeneration];
        currentProuns = new ProunBuilder[prounsPerGeneration];
        alive = prounsPerGeneration;

        eliteFitnessAverage = 0;

        if (shouldLoadFromFile)
        {
            GenomeData[] genomeData = DataController.LoadProunGenomeData(saveFileName);
            for (int i = 0; i < prounsPerGeneration; i++)
            {
                GenGenome(i, genomeData[i]);
            }
        }
        else
        {
            for (int i = 0; i < prounsPerGeneration; i++)
            {
                GenGenome(i);
            }
        }
    }

    private float GenProunCircleRadius()
    {
        return Utility.genInt(50) + 50;
    }

    private Vector3 GenProunPosition()
    {
        return Utility.genVector3Circle(GenProunCircleRadius());
    }

    /*
	 * Basically lays an egg with a baby Proun in it. It's like
	 * an egg because we later need to call spawn() to 'hatch' it.
	 * Note: the 'eggs' are invisible. And baby Prouns are the same
	 * as adult Prouns.
	 */
    private ProunGenome GenGenome(int index)
    {
        return GenGenome(index, GenProunPosition());
    }

    private ProunGenome GenGenome(int index, GenomeData data)
    {
        return GenGenome(index, data, GenProunPosition());
    }

    private ProunGenome GenGenome(int index, GenomeData data, Vector3 position)
    {
        ProunGenome newGenome = GenGenome(index, position);
        newGenome.SetData(data);
        return newGenome;
    }

    private ProunGenome GenGenome(int index, Vector3 position)
    {
        ProunGenome genome = InstantiateGenome(index, position);
        currentGenomes[index] = genome;
        return genome;
    }

    private ProunGenome PassGenome(int srcIndex, int destIndex)
    {
        return PassGenome(srcIndex, destIndex, GenProunPosition());
    }

    private ProunGenome PassGenome(int srcIndex, int destIndex, Vector3 position)
    {
        ProunGenome genome = InstantiateGenome(destIndex, position);
        genome.CloneGenome(lastGenomes[srcIndex]);
        currentGenomes[destIndex] = genome;
        return genome;
    }

    private ProunGenome InstantiateGenome(int index, Vector3 position)
    {
        GameObject obj = Instantiate(
            prounBlueprint,
            position,
            Quaternion.identity
        );
        obj.name = "PROUN_" + genNumber + "_" + index;
        ProunGenome genome = obj.GetComponent<ProunGenome>();
        return genome;
    }

    /*
	 * Calls spawn and registers the 'hatched' Proun as a
	 * current Proun in the generation.
	 */
    private void SpawnGenome(int index)
    {
        currentProuns[index] = currentGenomes[index].Spawn();
    }

    void Update()
    {
        if (generating) { return; }
        if (speedUp)
        {
            Time.timeScale = speedUpAmount;
            camera.enabled = false;
        }

        for (int i = 0; i < currentGenomes.Length; i++)
        {
            if (currentGenomes[i] == null)
                continue;

            if (!currentGenomes[i].IsSpawned())
                SpawnGenome(i); // Release the proun!

            ProunBuilder currentProun = currentProuns[i];
            // Check if a Proun is ready to die. If they are,
            // measure their fitness and add them to the 
            // last generation.
            if (currentProun.getLifetime() >= maximumLifetime)
            {
                float fitness = currentProun.getFitness();

                lastGenFitness[i] = fitness;
                lastGenomes[i] = currentGenomes[i];
                lastGenFlukes[i] = currentProun.GetNumFlukes();

                // Clean up the remains
                Destroy(currentProun.gameObject);
                currentGenomes[i] = null;
                alive--;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Saving generation...");
            DataController.SaveProunGenomes(currentGenomes, "ProunGenomes.json");
        }

        if (alive == 0)
            NewGeneration();
    }

    /*
	 * Selects a genome's index by tournament-style
	 * selection on the genome's fitness.
	 */
    static int ShotgunSelect(float[] fitnesses, int blastSize)
    {
        int maxIndex = Utility.genInt(fitnesses.Length);
        float maxFitness = fitnesses[maxIndex];

        for (int i = 1; i < blastSize; i++)
        {
            int randIndex = Utility.genInt(fitnesses.Length);
            float randFitness = fitnesses[randIndex];
            if (randFitness > maxFitness)
            {
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
    static int[] SelectTopGenomes(float[] fitnesses, int numTop)
    {
        int[] top = new int[numTop];
        float[] topFit = new float[numTop];
        for (int t = 0; t < numTop; t++)
        {
            top[t] = -1;
            topFit[t] = -1;
            for (int i = 0; i < fitnesses.Length; i++)
            {
                if ((t == 0) || (topFit[t - 1] > fitnesses[i]))
                {
                    if (fitnesses[i] > topFit[t])
                    {
                        top[t] = i;
                        topFit[t] = fitnesses[i];
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
    private int stableCount = 0;
    private void NewGeneration()
    {
        float averageGenFitness = 0f;
        float bestGenFitness = -1f;
        float aveComplexity = 0f;
        float aveMindSize = 0f;
        float aveBodySize = 0f;
        int numNormals = 0;
        int numFreaks = 0;
        float complexityFlukeRatio = 0;
        for (int i = 0; i < lastGenFitness.Length; i++)
        {

            float bodySize = lastGenomes[i].GetNumNodes() * lastGenomes[i].GetMindSize();
            float flukes = lastGenFlukes[i];

            string generation_data = "";
            generation_data += bodySize + ", ";
            generation_data += flukes + "\n";
            Debug.Log(generation_data);

            string filePath = Path.Combine(Application.streamingAssetsPath, "fluke_results.csv");

            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, generation_data);
            }
            else
            {
                File.WriteAllText(filePath, generation_data);
            }
            /*
            if (lastGenFitness[i] > 0)
            {
                bestGenFitness = bestGenFitness < lastGenFitness[i] ? lastGenFitness[i] : bestGenFitness;
                averageGenFitness += lastGenFitness[i];
                float complexity = (lastGenomes[i].GetMindSize() * 2f) / lastGenomes[i].GetNumNodes();


                aveComplexity += complexity;
                aveMindSize += lastGenomes[i].GetMindSize();
                aveBodySize += lastGenomes[i].GetNumNodes();
                numNormals++;
                if (lastGenFitness[i] > 1200 && lastGenFlukes[i] < 3)
                {
                    stableCount++;
                }
            }
            else
            {
                numFreaks++;
            }
            */
        }
        averageGenFitness /= numNormals;
        complexityFlukeRatio /= numNormals;
        aveComplexity /= numNormals;
        aveMindSize /= numNormals;
        aveBodySize /= numNormals;


        // string generation_data = "";
        // generation_data += genNumber + ", ";
        // // generation_data += numNormals + ", ";
        // // generation_data += stableCount + ", ";
        // // generation_data += aveComplexity + ", ";
        // // generation_data += aveMindSize + ", ";
        // // generation_data += aveBodySize + ", ";
        // // generation_data += averageGenFitness + ", ";
        // // generation_data += bestGenFitness + "\n";
        // generation_data += complexityFlukeRatio + "\n";
        // if (File.Exists(filePath))
        // {
        //     File.AppendAllText(filePath, generation_data);
        // }
        // else
        // {
        //     File.WriteAllText(filePath, generation_data);
        // }

        if (genNumber == maximumGenerations)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit ();
#endif
        }

        Time.timeScale = 1;
        generating = true;  // Disconnect the update loop
        genNumber++;

        int genomeIndex = 0;
        alive += DoElitism(lastGenFitness, ref genomeIndex);
        alive += DoMating(lastGenFitness, lastGenomes, ref genomeIndex);

        generating = false; // Reconnect the update loop
    }

    private int DoElitism(float[] lastGenFitnes, ref int genomeIndex)
    {
        int[] topGenomeIndices = SelectTopGenomes(lastGenFitness, elitism);
        for (int i = 0; i < elitism; i++)
        {
            int srcIndex = topGenomeIndices[i];
            int destIndex = genomeIndex + i;

            float eliteFitness = lastGenFitnes[srcIndex];

            if (shouldLogElites)
            {
                print("Elite #" + (i + 1) + " fitness: " + eliteFitness);

                if (i < 2)
                {
                    eliteFitnessAverage = (eliteFitnessAverage * (genNumber - 1) + eliteFitness) / genNumber;
                }
            }

            if (shouldSaveElites && eliteFitness >= saveFitnessCutoff)
            {
                DataController.SaveProunGenome(lastGenomes[srcIndex], saveFileName, false);
            }

            PassGenome(srcIndex, destIndex);
        }
        genomeIndex += elitism;

        if (shouldLogElites)
        {
            print("Average top elite fitness: " + eliteFitnessAverage);
        }

        return elitism;
    }

    private int DoMating(float[] lastGenFitness, ProunGenome[] lastGenomes, ref int genomeIndex)
    {
        int contribution = 0;

        for (; genomeIndex < prounsPerGeneration; genomeIndex++, contribution++)
        {
            int suitorIndex = ShotgunSelect(lastGenFitness, 3);

            int mateIndex;
            do { mateIndex = ShotgunSelect(lastGenFitness, 3); } while (mateIndex == suitorIndex);

            bool suitorBetter = lastGenFitness[suitorIndex] > lastGenFitness[mateIndex];

            ProunGenome worse = suitorBetter ? lastGenomes[mateIndex] : lastGenomes[suitorIndex];
            ProunGenome better = suitorBetter ? lastGenomes[suitorIndex] : lastGenomes[mateIndex];
            float worseFitness = suitorBetter ? lastGenFitness[mateIndex] : lastGenFitness[suitorIndex];
            float betterFitness = suitorBetter ? lastGenFitness[suitorIndex] : lastGenFitness[mateIndex];

            ProunGenome child = GenGenome(genomeIndex);
            child.SetParents(worse, better, worseFitness, betterFitness, mutationParameters);
            // Is it hot in here or is it just me?
        }
        // Note that we don't spawn the genomes just yet. They need to fully initialize (kind of like gestation!),
        // so we initialize them in the update loop when they're ready.
        return contribution;
    }
}