using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
	public float movementSpeed;
	public float rotationSpeed;

	void Update () {
		Vector3 displacement = new Vector3 ();
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
			displacement += Vector3.forward;
		}

		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
			displacement += Vector3.back;
		}

		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			displacement += Vector3.left;
		}

		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			displacement += Vector3.right;
		}

		if (Input.GetKey (KeyCode.Q) || Input.GetKey (KeyCode.Space)) {
			displacement += Vector3.up;
		}

		if (Input.GetKey (KeyCode.E) || Input.GetKey (KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
			displacement += Vector3.down;
		}


		Vector3 rotation = new Vector3 ();
		if (Input.GetKey (KeyCode.I)) {
			rotation.x -= 1;
		}

		if (Input.GetKey (KeyCode.K)) {
			rotation.x += 1;
		}

		if (Input.GetKey (KeyCode.J)) {
			rotation.y -= 1;
		}

		if (Input.GetKey (KeyCode.L)) {
			rotation.y += 1;
		}

		Quaternion moo = new Quaternion();
		moo.eulerAngles = (this.gameObject.transform.rotation.eulerAngles + rotation.normalized * rotationSpeed * Time.deltaTime);
		this.gameObject.transform.rotation = moo;

		Vector3 rawDisplacement = displacement.normalized * movementSpeed * Time.deltaTime;
		this.gameObject.transform.position += new Vector3 (
			rawDisplacement.x * Mathf.Cos(Mathf.Deg2Rad * moo.eulerAngles.y) + rawDisplacement.z * Mathf.Sin(Mathf.Deg2Rad * moo.eulerAngles.y),
			rawDisplacement.y,
			rawDisplacement.z * Mathf.Cos(Mathf.Deg2Rad * moo.eulerAngles.y) - rawDisplacement.x * Mathf.Sin(Mathf.Deg2Rad * moo.eulerAngles.y)
		);
	}
}
