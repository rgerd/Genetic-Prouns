using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This component reads the Proun's genome and builds the body
 * according to the genetic blueprint.
 */
public class ProunBuilder : MonoBehaviour {
	private const int JUDGEMENT_AGE = 10;
	private const float FREAK_HEIGHT = 50f;
	private const float FREAK_VELOCITY = 1000f;
	private const int LIFETIME_DEAD = 100000000;
	private const float FITNESS_NEVER_REPRODUCE = -10000000f;

	private GameObject proun;
	private Rigidbody[] nodes;
	private int lifetime;
	private Vector3 averagePosition;
	private int flukes = 10;

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
		Rigidbody node1 = nodes[gene.originNode];
		Rigidbody node2 = nodes [gene.connectedNode];

		switch (gene.jointType) {
		case Gene.JointType.FIXED_JOINT:
			// node1.transform.position = node2.transform.position;
			FixedJoint fixedJoint = node1.gameObject.AddComponent<FixedJoint> ();
			fixedJoint.connectedBody = node2;
			break;
		case Gene.JointType.SPRING_JOINT:
			AxisMuscle muscle = node1.gameObject.AddComponent<AxisMuscle> ();
			muscle.connectedBody = node2;
			node2.transform.position = (node1.transform.position + gene.axis * 2);
			muscle.Spawn (gene);
			break;
		}
	}
		
	void FixedUpdate () {
		if (lifetime > JUDGEMENT_AGE) {
			float averageVelocity = 0;
			for (int i = 0; i < nodes.Length; i++) {
				averageVelocity += nodes [i].velocity.sqrMagnitude;

				if (nodes [i].position.y >= FREAK_HEIGHT) {
					flukes--;
				}
			}
			averageVelocity /= nodes.Length;

			if (averageVelocity >= FREAK_VELOCITY) {
				flukes--;
			}
		}

		// We've got a freak on our hands.
		if (flukes == 0) {
			lifetime = LIFETIME_DEAD;
		}

		lifetime++;
	}

	private Gene[] GetGenes() {
		return gameObject.GetComponent<ProunGenome> ().GetGenes ();
	}

	private int GetNumNodes() {
		return gameObject.GetComponent<ProunGenome> ().GetNumNodes ();
	}

	public float getFitness() {
		if (flukes == 0) return FITNESS_NEVER_REPRODUCE;

		float totalMass = 0;
		Vector3 totalPos = new Vector3 ();
		foreach (Rigidbody node in nodes) {
			Vector3 lpos = node.gameObject.transform.localPosition;
			totalPos += new Vector3(lpos.x, 0, lpos.z) * node.mass;
			totalMass += node.mass;
		}

		return (totalPos / totalMass).magnitude;
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