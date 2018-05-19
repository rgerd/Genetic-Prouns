using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringContraction : MonoBehaviour {
	public float pullMin;
	public float pullMax;


	void FixedUpdate() {
		if ((int) Time.time % 2 == 0) {
			gameObject.GetComponent<SpringJoint> ().spring = pullMax;
		} else {
			gameObject.GetComponent<SpringJoint> ().spring = pullMin;
		}
	}
}
