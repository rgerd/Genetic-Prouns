using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeController : MonoBehaviour {
	public Rigidbody connectedBody;
	public bool activateAutomatically = false;
	public float maxVelocity = 300;
	public float wiggleSpeed = 2;
	private HingeJoint joint;

	void Start() {
		if (activateAutomatically)
			activate ();
	}

	public void activate() {
		joint = gameObject.AddComponent<HingeJoint> ();
		joint.useMotor = true;
		joint.enablePreprocessing = false;
		Vector3 dist = connectedBody.transform.position - gameObject.transform.position;
		joint.anchor = dist;
		joint.autoConfigureConnectedAnchor = false;
		Vector3 axis = Utility.genVector3Circle (1);
		axis.y = 0;
		joint.axis = Utility.genFloat() < 0.5 ? Vector3.forward : Vector3.up;
		JointMotor motor = joint.motor;
		motor.force = 1000;
		motor.freeSpin = true;
		joint.motor = motor;

		joint.connectedBody = connectedBody;
		joint.connectedAnchor = -dist;
	}

	void FixedUpdate() {
		JointMotor motor = joint.motor;
		motor.targetVelocity = Mathf.Sin (Time.time * wiggleSpeed) * maxVelocity;
		joint.motor = motor;
	}
}
