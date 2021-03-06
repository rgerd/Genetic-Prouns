﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Range <T> where T : System.IConvertible {
	public readonly T min;
	public readonly T max;
	public float size {
		get {
			return (float)(max.ToDouble (null) - min.ToDouble (null));
		}
	}

	public Range (T min, T max) {
		this.min = min;
		this.max = max;
	}
}

public class Utility {
	public static int min(int a, int b) { return a < b ? a : b; }
	public static int max(int a, int b) { return a > b ? a : b; }

	public static Vector3 genAxis() {
		Vector3[] possible = {
			Vector3.up,
			Vector3.down,
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back,
		};
		return possible [genInt (possible.Length)];
	}

	public static Vector3 genVector3(float scale) {
		return new Vector3 (Random.value * scale, Random.value * 10 + 1, Random.value * scale);
	}

	public static Vector3 genVector3Box(float scale) {
		return new Vector3 (Random.value * scale, Random.value * scale, Random.value * scale);
	}

	public static Vector3 genVector3Circle(float scale) {
		float angle = Random.value * Mathf.PI * 2;
		return new Vector3 (Mathf.Sin (angle) * scale, 2, Mathf.Cos (angle) * scale);
	}

	public static float genFloat(Range<float> r) {
		return genFloat (r.min, r.max);
	}

	public static float genFloat(float a, float b) {
		return (genFloat() * (b - a)) + a; 
	}
		
	public static float nudgeFloat (float value, Range<float> range, float scale) {
		float adjustedSize = range.size * scale;
		return clampFloat((genFloat () * adjustedSize) - (adjustedSize / 2), range);
	}

	public static float genFloat() {
		return Random.value;
	}

	public static float clampFloat(float value, Range<float> range) {
		return Mathf.Max (range.min, Mathf.Min (range.max, value));
	}

	public static int genInt(Range<int> r) {
		return genInt (r.min, r.max);
	}
		
	public static int genInt(int a, int b) {
		return genInt(b - a) + a;
	}

	public static int genInt(int size) {
		return (int)(Random.value * size);
	}

	public static uint genUnsignedInt() {
		return (uint)(Random.value * 0xffffffff) + 1;
	}

	public static bool flipCoin(float probabilityHeads) {
		return genFloat () <= probabilityHeads;
	}

	public static Vector3 eProd(Vector3 a, Vector3 b) {
		return new Vector3 (
			a.x * b.x,
			a.y * b.y,
			a.z * b.z
		);
	}

	public static int axisCompare(Vector3 a, Vector3 b) {
		bool g_x = a.x >= b.x;
		bool g_y = a.y >= b.y;
		bool g_z = a.z >= b.z;

		bool l_x = a.x <= b.x;
		bool l_y = a.y <= b.y;
		bool l_z = a.z <= b.z;

		bool e_x = a.x == b.x;
		bool e_y = a.y == b.y;
		bool e_z = a.z == b.z;

		if (e_x && e_y && e_z)
			return 0;
		
		if (g_x && g_y && g_z)
			return 1;
		
		if (l_x && l_y && l_z)
			return -1;
		
		return 0;
	}

	public static T[] quickSort<T>(T[] elems, float[] values) {
		Dictionary<T, float> dict = new Dictionary<T, float> ();
		for (int i = 0; i < elems.Length; i++)
			dict.Add (elems [i], values [i]);
		T[] results = new T[elems.Length];
		quickSort<T> (dict).CopyTo (results);
		return results;
	}

	public static List<T> quickSort<T>(Dictionary<T, float> items) {
		int numItems = items.Keys.Count;
		T[] elems = new T[numItems];
		items.Keys.CopyTo(elems, 0);
		if (numItems <= 1)
			return new List<T>(elems);
		
		float[] vals = new float[numItems];
		items.Values.CopyTo (vals, 0);

		int pivotIndex = genInt (numItems);
		T pivot = elems [pivotIndex];
		float pivotVal = vals [pivotIndex];

		Dictionary<T, float> left = new Dictionary<T, float> ();
		Dictionary<T, float> right = new Dictionary<T, float> ();

		for (int i = 0; i < numItems; i++) {
			if (i == pivotIndex)
				continue;
			(vals [i] < pivotVal ? left : right).Add (elems [i], vals [i]);
		}

		List<T> result = quickSort<T> (left);
		result.Add (pivot);
		result.AddRange (quickSort<T> (right));
		return result;
	}

	/**
	 * Returns true iff Utility.quickSort is working
	 **/
	private static bool testQuickSort() {
		string[] elems = {
			"world",
			"friends",
			"how",
			"hello",
			"are",
			"you"
		};

		float[] values = {
			2.4f,
			11.2f,
			3.3f,
			1.8f,
			5.3f,
			7.2f
		};

		string[] sorted = Utility.quickSort<string>(elems, values);
		return sorted[0] == "hello" //
			&& sorted[1] == "world" //
			&& sorted[2] == "how" //
			&& sorted[3] == "are" //
			&& sorted[4] == "you" //
			&& sorted[5] == "friends"; //
	}
		
	public static bool TEST(bool verbose) {
		Assert.IsTrue (testQuickSort (), "QuickSort test failed.");
		if (verbose) {
			Debug.Log ("[√] Utility TEST passed.");
		}
		return true;
	}
}
