﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisMuscle : MonoBehaviour {
	public bool spawnAutomatically = false;
	public Rigidbody connectedBody;
	public Vector3 movementAxis;

	private ConfigurableJoint joint;
	// Made public only so editor can see
	public float heartBeat;
	public float contractTime;
	public float contractedLength;
	public float extensionDistance;
	public Gene.EnableMode enableMode;

	void Start () {
		if (this.spawnAutomatically)
			this.Spawn (null);
	}

	private void RandomizeParams() {
		this.heartBeat = Utility.genFloat (0.5f, 2f);
		this.contractedLength = 0.0f;
		this.extensionDistance = Utility.genFloat () + 0.5f;
		this.contractTime = Utility.genFloat ();
		this.movementAxis = Utility.genAxis ();
		this.enableMode = (Gene.EnableMode)Utility.genInt ((int)Gene.EnableMode.NumEnableModes);
	}

	private void UseGene(MuscleGene gene) {
		this.heartBeat = gene.heartBeat;
		this.contractTime = gene.contractTime;
		this.contractedLength = gene.contractedLength;
		this.extensionDistance = gene.extensionDistance;
		this.movementAxis = gene.axis;
		this.enableMode = gene.enableMode;
	}

	public void Spawn (MuscleGene gene) {
		if (gene != null) {
			this.UseGene (gene);
		} else {
			this.RandomizeParams ();
		}
		this.Spawn ();
	}

	public void Spawn () {
		this.joint = this.gameObject.AddComponent<ConfigurableJoint> ();
		this.joint.connectedBody = this.connectedBody;

		this.joint.xMotion = ConfigurableJointMotion.Limited;
		this.joint.yMotion = ConfigurableJointMotion.Limited;
		this.joint.zMotion = ConfigurableJointMotion.Limited;

		this.joint.angularXMotion = ConfigurableJointMotion.Limited;
		this.joint.angularYMotion = ConfigurableJointMotion.Limited;
		this.joint.angularZMotion = ConfigurableJointMotion.Limited;

		this.joint.xDrive = InitAxisDrive(this.joint.xDrive);
		this.joint.yDrive = InitAxisDrive(this.joint.yDrive);
		this.joint.zDrive = InitAxisDrive(this.joint.zDrive);

		if (this.movementAxis.x != 0) {
			this.joint.xMotion = ConfigurableJointMotion.Free;
			this.joint.xDrive = ConfigureAxisDrive(this.joint.xDrive);
		}

		if (this.movementAxis.y != 0) {
			this.joint.yMotion = ConfigurableJointMotion.Free;
			this.joint.yDrive = ConfigureAxisDrive(this.joint.yDrive);
		}

		if (this.movementAxis.z != 0) {
			this.joint.zMotion = ConfigurableJointMotion.Free;
			this.joint.zDrive = ConfigureAxisDrive(this.joint.zDrive);
		}

		this.joint.enablePreprocessing = false;
		this.joint.enableCollision = false;
	}

	JointDrive InitAxisDrive(JointDrive drive) {
		drive.positionSpring = 100;
		drive.positionDamper = 5000;
		drive.maximumForce = 0;

		return drive;
	}

	JointDrive ConfigureAxisDrive(JointDrive drive) {
		drive.positionSpring = 100;
		drive.positionDamper = 5;
		drive.maximumForce = 100;
		return drive;
	}
		
	void FixedUpdate () {
		if (enableMode != Gene.EnableMode.Enabled) {
			return;
		}

		Vector3 position = this.joint.targetPosition;
		float radTime = Time.time * Mathf.PI * 2; // Time converted to radians. 1 second = 0.5, 1, 0.5, 0
		radTime /= heartBeat;
		float rawWave = (Mathf.Sin(radTime) + 1) / 2; // 0-1
		float targetPosition = (rawWave * extensionDistance) + contractedLength;
		position = this.movementAxis * targetPosition;
		this.joint.targetPosition = position;
	}
}
