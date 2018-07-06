using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProunGarden : MonoBehaviour {
	public GameObject prounBlueprint;
	public string loadFileName;
	public int numProunsToLoad;

	void Start () {
		GenomeData[] genomeData = DataController.LoadProunGenomeData (loadFileName);

		print ("Loading " + numProunsToLoad + " / " + genomeData.Length + " prouns.");

		for (int i = 0; i < numProunsToLoad; i++) {
			InstantiateGenome (i, genomeData[Utility.genInt(genomeData.Length)]).Spawn ();
		}
	}

	private ProunGenome InstantiateGenome(int index, GenomeData data) {
		GameObject obj = Instantiate (
			prounBlueprint, 
			GenProunPosition(),
			Quaternion.identity
		);
		obj.name = "GARDEN_PROUN_" + data.genomeID;
		ProunGenome genome = obj.GetComponent<ProunGenome> ();
		genome.SetData (data);
		return genome;
	}

	private Vector3 GenProunPosition() {
		return Utility.genVector3Circle (Utility.genInt (50) + 50);
	}
}
