using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataController : MonoBehaviour {
	void Start () {
		// Possibly look for file and load prouns from previous run
	}

	public static void SaveProunGenome(ProunGenome genome, string dataFileName, bool prettyPrint = true) {
		SaveProunGenomes (new ProunGenome[] { genome }, dataFileName, prettyPrint);
	}

	public static void SaveProunGenomes(ProunGenome[] genomes, string dataFileName, bool prettyPrint = true) {
		string filePath = Path.Combine (Application.streamingAssetsPath, dataFileName);
		string dataAsJson = GenomeData.ArrayToJson (genomes, prettyPrint);
		if (File.Exists (filePath)) {
			File.AppendAllText (filePath, dataAsJson);
		} else {
			File.WriteAllText (filePath, dataAsJson);
		}
	}

	public static GenomeData[] LoadProunGenomeData(string dataFileName) {
		string filePath = Path.Combine (Application.streamingAssetsPath, dataFileName);

		if (File.Exists (filePath)) {
			string dataAsJson = File.ReadAllText (filePath);
			return GenomeData.GenomeDataFromArray (dataAsJson);
		}

		Debug.LogError ("Could not find data file!");

		return null;
	}
		
}
