using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This component reads the Proun's genome and builds the body
 * according to the genetic blueprint.
 */
public class ProunBuilder : MonoBehaviour {
	private const int JUDGEMENT_AGE = 200;
	private const float FREAK_HEIGHT = 50f;
	private const float FREAK_DEPTH = -50f;
	private const float FREAK_VELOCITY = 1000f;
	private const int LIFETIME_DEAD = 100000000;
	private const float FITNESS_NEVER_REPRODUCE = -1f;

	private GameObject proun;
	private Rigidbody[] nodes;
	private int lifetime;
	private Vector3 startPosition = Vector3.zero;
	private int flukes = 10;
	private float muscleDensity = 0;

	void Start () {
		lifetime = 0;
		Gene[] genes = this.GetGenes ();
		int numNodes = this.GetNumNodes ();

		nodes = new Rigidbody[numNodes];
		foreach (Gene gene in genes) {
			if (gene is NodeGene) {
				BuildNode ((NodeGene) gene);
			} else if(gene is MuscleGene) {
				BuildMuscle ((MuscleGene) gene);
			}
		}
	}

	/*
	 * Nothing super fancy here. Just takes the parameters from the gene and applies them to the node object.
	 */
	private void BuildNode(NodeGene gene) {
		int index = gene.index;
		int x = index % 5;
		int y = index / 5;
		Vector3 position = 
			Vector3.forward * x * 1.5f +
			Vector3.left * y * 1.5f +
			Vector3.up * 10 + Vector3.up * Utility.genFloat ();

		Rigidbody newNode = Instantiate<Rigidbody> (ProunGenome.nodeBodies[gene.body_type], gameObject.transform);
		newNode.gameObject.name = "NODE_" + index;

		newNode.transform.localPosition = position;

		Vector3 scale = newNode.transform.localScale;
		scale.x *= gene.mass * gene.scale.x;
		scale.y *= gene.mass * gene.scale.y;
		scale.z *= gene.mass * gene.scale.z;
		newNode.transform.localScale = scale;
	
		Renderer newNodeRend = newNode.gameObject.GetComponent<Renderer> ();
		Material newNodeMat = ProunGenome.materials[gene.material];
		newNodeRend.material = newNodeMat;

		PhysicMaterial newNodePhysMat = newNode.GetComponent<Collider> ().material;
		newNodePhysMat.dynamicFriction = gene.d_friction;
		newNodePhysMat.staticFriction  = gene.s_friction;

		newNode.mass = gene.mass;
		newNode.collisionDetectionMode = CollisionDetectionMode.Discrete;

		nodes [gene.index] = newNode;
	}

	/*
	 * Same thing here. Builds the right joint connecting two nodes and applies the gene parameters.
	 */
	private void BuildMuscle(MuscleGene gene) {
		if (gene.enableMode == Gene.EnableMode.Disabled)
			return;

		muscleDensity += 2f / nodes.Length;

		Rigidbody node1 = nodes[gene.originNode];
		Rigidbody node2 = nodes [gene.connectedNode];

		switch (gene.jointType) {
		case Gene.JointType.Fixed:
			// node1.transform.position = node2.transform.position;
			FixedJoint fixedJoint = node1.gameObject.AddComponent<FixedJoint> ();
			fixedJoint.connectedBody = node2;
			break;
		case Gene.JointType.Spring:
			AxisMuscle muscle = node1.gameObject.AddComponent<AxisMuscle> ();
			muscle.connectedBody = node2;
			node2.transform.position = (node1.transform.position + gene.axis * 2);
			muscle.Spawn (gene);
			break;
		}
	}

	Vector3 GetPosition() {
		Vector3 position = new Vector3 ();
		foreach (Rigidbody node in nodes) {
			position += node.position / nodes.Length;
		}

		return position;
	}
		
	void FixedUpdate () {
		if (lifetime >= ProunGenerator.prounMaximumLifetime) {
			return;
		}

		if (lifetime > JUDGEMENT_AGE) {
			float averageVelocity = 0;
			for (int i = 0; i < nodes.Length; i++) {
				averageVelocity += nodes [i].velocity.sqrMagnitude;

				if (nodes [i].position.y >= FREAK_HEIGHT 
				 || nodes [i].position.y <= FREAK_DEPTH) {
					flukes--;
				}
			}
			averageVelocity /= nodes.Length;

			if (averageVelocity >= FREAK_VELOCITY) {
				flukes--;
			}

			if (startPosition == Vector3.zero) {
				startPosition = GetPosition ();
			}
		}

		// We've got a freak on our hands.
		if (flukes <= 0) {
			lifetime = LIFETIME_DEAD;
		} else {
			lifetime++;
		}
	}

	private Gene[] GetGenes() {
		return gameObject.GetComponent<ProunGenome> ().GetGenes ();
	}

	private int GetNumNodes() {
		return gameObject.GetComponent<ProunGenome> ().GetNumNodes ();
	}

	public float getFitness() {
		if (flukes <= 0) return FITNESS_NEVER_REPRODUCE;

		Vector3 finalPosition = GetPosition ();
		float distance = (finalPosition - startPosition).sqrMagnitude;

		float sizeFitness = (float)Math.Pow (nodes.Length, 3);
		float muscleFitness = (float)Math.Pow (muscleDensity, 3);
		float travelFitness = (distance / lifetime) * 50;
		float stabilityFitness = flukes / 10f;

		float totalFitness = (sizeFitness * muscleFitness * travelFitness * stabilityFitness) / 1000f;

		return totalFitness;
	}

	public int getLifetime() {
		return lifetime;
	}
}

/*
 * 
 switch (gene.jointType) {
		case ProunGenome.JointType.SPRING_JOINT:

			MuscleController muscle = Instantiate (ProunGenome.muscle).GetComponent<MuscleController> ();

			muscle.source = node1;
			muscle.dest = node2;
			muscle.push = gene.push;
			muscle.pullMin = gene.pullMin;
			muscle.pullMax = gene.pullMax;
			muscle.spawn ();

			break;
		case ProunGenome.JointType.HINGE_JOINT:

			if (node1.GetComponent<HingeController> () != null)
				return;
			HingeController hingeJoint = node1.gameObject.AddComponent<HingeController> ();
			hingeJoint.connectedBody = node2;
			hingeJoint.maxVelocity = gene.push;
			hingeJoint.activate ();

			break;
		case ProunGenome.JointType.FIXED_JOINT:

			node1.transform.position = node2.transform.position;
			FixedJoint fixedJoint = node2.gameObject.AddComponent<FixedJoint> ();
			fixedJoint.connectedBody = node1;

			break;
		}
 */