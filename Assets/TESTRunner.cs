using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TESTRunner : MonoBehaviour {
	public bool verbose;

	void Start () {
		Utility.TEST (verbose);
		AdjacencyMatrix.TEST (verbose);
	}
}
