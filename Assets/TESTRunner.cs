using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTRunner : MonoBehaviour {
	public bool failHard;
	public bool verbose;

	void Start () {
		if (failHard && !AdjacencyMatrix.TEST (failHard, verbose)) {
			throw new UnityException ("AdjacencyMatrix TEST passed.");
		}
	}
}
