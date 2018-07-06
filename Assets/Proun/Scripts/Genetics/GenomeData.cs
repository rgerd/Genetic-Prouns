using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class GenomeData {
	public string genomeID;
	public NodeGene[] body;
	public MuscleGene[] mind;

	public string ToJson(bool prettyPrint = true) {
		return JsonUtility.ToJson (this, prettyPrint);
	}

	public static string ArrayToJson(ProunGenome[] genomes, bool prettyPrint = true) {
		GenomeData[] data = new GenomeData[genomes.Length];
		for (int i = 0; i < genomes.Length; i++) {
			if (genomes [i] != null) {
				data [i] = genomes [i].ToGenomeData ();
			}
		}
		return ArrayToJson (data, prettyPrint);
	}

	public static string ArrayToJson(GenomeData[] genomes, bool prettyPrint = true) {
		string[] jsonObjects = new string[genomes.Length];
		for (int i = 0; i < genomes.Length; i++) {
			if (genomes [i] != null) {
				jsonObjects [i] = genomes [i].ToJson (prettyPrint);
			}
		}
		return "***\n" + string.Join (prettyPrint ? "\n***\n" : "***", jsonObjects) + "\n";
	}

	public static GenomeData[] GenomeDataFromArray(string json) {
		Char[] delimeter = new Char[] { '*', '*', '*' };
		string[] jsonObjects = json.Split (delimeter);
		int numObjects = jsonObjects.Length;
		List<GenomeData> data = new List<GenomeData> ();
		for (int i = 0; i < numObjects; i++) {
			if (jsonObjects [i].Length == 0)
				continue;
			data.Add (JsonUtility.FromJson<GenomeData> (jsonObjects [i]));
		}

		return data.ToArray ();
	}
}
