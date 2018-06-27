using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderJoint : MonoBehaviour {
	public Rigidbody connectedBody;
	public bool activateAutomatically = false;
	public float maxVelocity = 300;
	public float wiggleSpeed = 2;
	private GameObject harpoon;
	private GameObject grip;
	private Vector3 axis;

	void Start() {
		if (activateAutomatically)
			activate ();
	}

	public void activate() {
		grip = createEmptyRigidbody ("Grip");
		glueHandle (grip, gameObject.GetComponent<Rigidbody>());
		grip.transform.parent = gameObject.transform;
		grip.transform.localPosition = Vector3.zero;

		harpoon = createEmptyRigidbody ("Harpoon");
		glueHandle (harpoon, connectedBody);
		harpoon.transform.position = grip.transform.position + Utility.genVector3Box (1);
		connectedBody.transform.position = harpoon.transform.position;
		harpoon.transform.parent = connectedBody.transform;

		axis = new Vector3 ();
		int rand = Utility.genInt (3);
		switch (rand) {
		case 0:
			axis.x = 1;
			break;
		case 1:
			axis.y = 1;
			break;
		case 2:
			axis.z = 1;
			break;
		}
	}

	GameObject createEmptyRigidbody(string name) {
		GameObject obj = new GameObject ();
		obj.name = name;
		Rigidbody body = obj.AddComponent<Rigidbody> ();
		// body.useGravity = false;
		// body.isKinematic = true;
		body.mass = 0.001f;
		return obj;
	}

	void glueHandle(GameObject handle, Rigidbody dest) {
		FixedJoint handleGlue = handle.AddComponent<FixedJoint> ();
		handleGlue.breakForce = 1000000000000;
		handleGlue.connectedBody = dest;
		handleGlue.anchor = Vector3.zero;
		handleGlue.autoConfigureConnectedAnchor = false;
		handleGlue.connectedAnchor = Vector3.zero;
	}

	void FixedUpdate() {
		Vector3 gp = Utility.eProd(grip.transform.position, axis);
		Vector3 hp = Utility.eProd(harpoon.transform.position, axis);

		float dist = Vector3.Distance(gp, hp);
		int sign = Mathf.Sin (Time.time * 4) > 0 ? 1 : -1;
		bool attract = Utility.axisCompare(hp, gp) == sign;
//		print (hp.x + ", " + hp.y + ", " + hp.z + " | " + gp.x + ", " + gp.y + ", " + gp.z + " | " + Utility.axisCompare (hp, gp));

		if ((dist < 0.5 || attract) && (dist > 0.01 || !attract)) {
			grip.GetComponent<Rigidbody> ().AddForce (axis * (sign * 1000), ForceMode.VelocityChange);
			harpoon.GetComponent<Rigidbody> ().AddForce (axis * (-sign * 1000), ForceMode.VelocityChange);
		} else {
			grip.transform.parent.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			grip.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			harpoon.transform.parent.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			harpoon.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}
	}
}
