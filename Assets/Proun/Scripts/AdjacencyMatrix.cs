using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMatrix<T> : AdjacencyMatrix {
	private int size;
	private T[] elements;

	public AdjacencyMatrix(int size) {
		this.elements = new T[(size * (size - 1)) >> 1];
		this.size = size;
	}

	private int toIndex(int row, int col) {
		return ((row * (row - 1)) >> 1) + col;
	}

	public T getNeighbor(int a, int b) {
		if (a == b || a < 0 || b < 0 || a >= size || b >= size)
			return default(T);
		return elements [toIndex(Utility.max(a, b), Utility.min(a, b))];
	}

	public void setNeighbor(int a, int b, T connection) {
		if (a == b || a < 0 || b < 0 || a >= size || b >= size)
			return;
		elements [toIndex (Utility.max (a, b), Utility.min (a, b))] = connection;
	}

	public T[] getNeighbors(int i) {
		if (i < 0 || i >= size)
			return new T[]{};

		T[] neighbors = new T[size - 1];
		int neighborId = 0;

		// Row Scan
		int rowStart = toIndex (i, 0);
		int rowEnd = rowStart + i;
		for (int ri = rowStart; ri < rowEnd; ri++) {
			if (!elements [ri].Equals(default(T)))
				neighbors[neighborId++] = elements[ri];
		}

		// Column Scan
		int row = i + 1;
		int neighborIndex = toIndex (row, i);
		while (neighborIndex < elements.Length) {
			if (!elements [neighborIndex].Equals(default(T)))
				neighbors[neighborId++] = elements[neighborIndex];
			neighborIndex += row++;
		}

		T[] _neighbors = new T[neighborId];
		Array.Copy (neighbors, _neighbors, neighborId);

		return _neighbors;
	}


}

public class AdjacencyMatrix {
	// Test Elegantly, Swiftly, and Thoroughly
	public static bool TEST(bool failHard, bool verbose) {
		AdjacencyMatrix<int> testMat = new AdjacencyMatrix<int> (8);

		// Test NULL
		if (testMat.getNeighbor(0, 1) != 0) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: testMat.getNeighbor(0, 1) = " + testMat.getNeighbor(0, 1));
			return false;
		}

		testMat.setNeighbor (-1, 2, 1);
		if (testMat.getNeighbor (-1, 2) != 0) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: testMat.getNeighbor (-1, 2) = " + testMat.getNeighbor(-1, 2));
			return false;
		}

		testMat.setNeighbor (0, 8, 1);
		if (testMat.getNeighbor (0, 8) != 0) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: testMat.getNeighbor (0, 8) = " + testMat.getNeighbor(0, 8));
			return false;
		}

		testMat.setNeighbor(0, 0, 1);
		if (testMat.getNeighbor (0, 0) != 0) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: testMat.getNeighbor (0, 0) = " + testMat.getNeighbor(0, 0));
			return false;
		}

		// Test set
		testMat.setNeighbor(0, 1, 2);
		testMat.setNeighbor(0, 2, 4);
		testMat.setNeighbor(0, 3, 6);
		testMat.setNeighbor(0, 4, 8);
		testMat.setNeighbor(0, 5, 10);
		testMat.setNeighbor(0, 6, 12);
		testMat.setNeighbor(0, 7, 14);

		if (testMat.getNeighbor (0, 5) != 10) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: testMat.getNeighbor (0, 5) = " + testMat.getNeighbor(0, 1));
			return false;
		}

		testMat.setNeighbor (0, 5, 0);

		int[] expectedNeighbors = new int[] { 2, 4, 6, 8, 12, 14 };
		int[] actualNeighbors = testMat.getNeighbors (0);

		if (expectedNeighbors.Length != actualNeighbors.Length) {
			if (failHard)
				throw new Exception ("AdjacencyMatrix TEST FAILED: expectedNeighbors.Length (" + expectedNeighbors.Length + ") != actualNeighbors.Length (" + actualNeighbors.Length + ")");
			return false;
		}
		for (int i = 0; i < expectedNeighbors.Length; i++) {
			if (expectedNeighbors [i] != actualNeighbors [i]) {
				if (failHard)
					throw new Exception ("AdjacencyMatrix TEST FAILED: expectedNeighbors.Length != actualNeighbors.Length");
				return false;
			}
		}

		if (verbose)
			Debug.Log ("AdjacencyMatrix TEST passed.");
		return true;
	}
}

// [ * - - - ]
// [ 1 * - - ]
// [ 0 1 * - ]
// [ 1 2 3 * ]
// ((size) * (size - 1)) >> 1
