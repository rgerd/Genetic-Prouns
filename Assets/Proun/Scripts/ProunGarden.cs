using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProunGarden : MonoBehaviour {
	public GameObject prounBlueprint;
	public string loadFileName;

	void Start () {
		GenomeData[] genomeData = DataController.LoadProunGenomeData (loadFileName);
		for (int i = 0; i < genomeData.Length; i++) {
			InstantiateGenome (i, genomeData[i]).Spawn ();
		}
	}

	private ProunGenome InstantiateGenome(int index, GenomeData data) {
		GameObject obj = Instantiate (
			prounBlueprint, 
			GenProunPosition(),
			Quaternion.identity
		);
		obj.name = "GARDEN_PROUN_" + index;
		ProunGenome genome = obj.GetComponent<ProunGenome> ();
		genome.SetData (data);
		return genome;
	}

	private Vector3 GenProunPosition() {
		return Utility.genVector3Circle (Utility.genInt (50) + 50);
	}
}
