using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushAway : MonoBehaviour {
	public Vector3 vec;

	void FixedUpdate() {
		this.gameObject.GetComponent<Rigidbody> ().AddForce (vec);
	}
}
