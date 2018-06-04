using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AdjacencyMatrix<T> : AdjacencyMatrix {
	private int size;
	private T[] elements;

	public AdjacencyMatrix(int size) {
		this.elements = new T[(size * (size - 1)) >> 1];
		this.size = size;
	}

	private int ToIndex(int row, int col) {
		return ((row * (row - 1)) >> 1) + col;
	}

	public T GetNeighbor(int a, int b) {
		if (a == b || a < 0 || b < 0 || a >= size || b >= size)
			return default(T);
		return elements [ToIndex(Utility.max(a, b), Utility.min(a, b))];
	}

	public void SetNeighbor(int a, int b, T connection) {
		if (a == b || a < 0 || b < 0 || a >= size || b >= size)
			return;
		elements [ToIndex (Utility.max (a, b), Utility.min (a, b))] = connection;
	}

	private bool ElementDefined(int index) {
		return elements [index] != null && !elements [index].Equals (default(T));
	}

	public T[] GetNeighbors(int i) {
		if (i < 0 || i >= size)
			return new T[]{};

		T[] neighbors = new T[size - 1];
		int neighborId = 0;

		// Row Scan
		int rowStart = ToIndex (i, 0);
		int rowEnd = rowStart + i;
		for (int ri = rowStart; ri < rowEnd; ri++) {
			if (ElementDefined(ri))
				neighbors[neighborId++] = elements[ri];
		}

		// Column Scan
		int row = i + 1;
		int neighborIndex = ToIndex (row, i);
		while (neighborIndex < elements.Length) {
			if (ElementDefined(neighborIndex))
				neighbors[neighborId++] = elements[neighborIndex];
			neighborIndex += row++;
		}

		T[] neighborsResized = new T[neighborId];
		Array.Copy (neighbors, neighborsResized, neighborId);

		return neighborsResized;
	}

	public T[] GetContents() {
		T[] neighbors = new T[elements.Length];
		int numInitialized = 0;
		for (int i = 0; i < elements.Length; i++) {
			if (ElementDefined(i))
				neighbors [numInitialized++] = elements [i];
		}

		T[] neighborsResized = new T[numInitialized];
		Array.Copy (neighbors, neighborsResized, numInitialized);

		return neighborsResized;
	}
}

public class AdjacencyMatrix {
	// Test Elegantly, Swiftly, and Thoroughly
	public static bool TEST(bool verbose) {
		AdjacencyMatrix<int> testMat = new AdjacencyMatrix<int> (8);

		// Test NULL
		Assert.AreEqual(testMat.GetNeighbor(0, 1), 0, "Matrix has non-default initial value.");

		testMat.SetNeighbor (-1, 2, 1);
		Assert.AreEqual(testMat.GetNeighbor(-1, 2), 0, "Matrix set a neighbor for a negative index.");

		testMat.SetNeighbor (0, 8, 1);
		Assert.AreEqual(testMat.GetNeighbor(0, 8), 0, "Matrix set a neighbor for an out-of-bounds index.");

		testMat.SetNeighbor(0, 0, 1);
		Assert.AreEqual(testMat.GetNeighbor(0, 0), 0, "Matrix set a neighbor for an index to itself.");

		// Test set
		testMat.SetNeighbor(0, 1, 2);
		testMat.SetNeighbor(0, 2, 4);
		testMat.SetNeighbor(0, 3, 6);
		testMat.SetNeighbor(0, 4, 8);
		testMat.SetNeighbor(0, 5, 10);
		testMat.SetNeighbor(0, 6, 12);
		testMat.SetNeighbor(0, 7, 14);

		Assert.AreEqual (testMat.GetNeighbor (0, 5), 10, "Matrix did not properly set neighbor value.");

		testMat.SetNeighbor (0, 5, 0);

		int[] expectedNeighbors = new int[] { 2, 4, 6, 8, 12, 14 };
		int[] actualNeighbors = testMat.GetNeighbors (0);

		Assert.AreEqual (expectedNeighbors.Length, actualNeighbors.Length, "Number of expected neighbors does not match actual number.");

		for (int i = 0; i < expectedNeighbors.Length; i++) {
			Assert.AreEqual (expectedNeighbors [i], actualNeighbors [i], "Expected neighbor does not match actual neighbor.");
		}
			
		Debug.Log ("[√] AdjacencyMatrix TEST passed.");
		return true;
	}
}

// [ * - - - ]
// [ 1 * - - ]
// [ 0 1 * - ]
// [ 1 2 3 * ]
// ((size) * (size - 1)) >> 1
