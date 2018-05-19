using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This component reads the Proun's genome and builds the body
 * according to the genetic blueprint.
 */
public class ProunBuilder : MonoBehaviour {
	private GameObject proun;
	private Rigidbody[] nodes;
	private int lifetime;

	void Start () {
		lifetime = 0;
		ProunGenome.NodeGene[] genome = this.getGenome();
		int numNodes = genome.Length;

		nodes = new Rigidbody[numNodes];
		foreach(ProunGenome.NodeGene nodeGene in genome) {
			int index = nodeGene.index;
			int x = index % 5;
			int y = index / 5;
			nodes [index] = buildNode (nodeGene, Vector3.forward * x * 1.5f + Vector3.left * y * 1.5f + Vector3.up * 10 + Vector3.up * Utility.genFloat ());
			nodes[index].gameObject.name = "NODE_" + index;
		}
			
		foreach (ProunGenome.NodeGene nodeGene in genome) {
			foreach (ProunGenome.MuscleGene muscleGene in nodeGene.muscles) {
				Rigidbody srcNode = nodes [nodeGene.index];
				Rigidbody destNode = nodes [muscleGene.connectedNode];
				connectNodes (muscleGene, srcNode, destNode);
			}
		}
	}

	/*
	 * Nothing super fancy here. Just takes the parameters from the gene and applies them to the node object.
	 */
	Rigidbody buildNode(ProunGenome.NodeGene gene, Vector3 position) {
		Rigidbody newNode = Instantiate<Rigidbody> (ProunGenome.nodeBodies[gene.body_type], gameObject.transform);
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

		return newNode;
	}

	/*
	 * Same thing here. Builds the right joint connecting two nodes and applies the gene parameters.
	 */
	void connectNodes(ProunGenome.MuscleGene gene, Rigidbody node1, Rigidbody node2) {
		switch (gene.jointType) {
		case ProunGenome.JointType.FIXED_JOINT:
			// node1.transform.position = node2.transform.position;
			FixedJoint fixedJoint = node1.gameObject.AddComponent<FixedJoint> ();
			fixedJoint.connectedBody = node2;
			break;
		case ProunGenome.JointType.SPRING_JOINT:
			AxisMuscle muscle = node1.gameObject.AddComponent<AxisMuscle> ();
			muscle.connectedBody = node2;
			int sign;
			switch (Utility.genInt (3)) {
			case 0:
				sign = node1.transform.position.x - node2.transform.position.x < 0 ? -1 : 1;
				muscle.movementAxis = Vector3.left * sign;
				break;
			case 1:
				sign = node1.transform.position.y - node2.transform.position.y < 0 ? -1 : 1;
				muscle.movementAxis = Vector3.up * sign;
				break;
			case 2:
				sign = node1.transform.position.z - node2.transform.position.z < 0 ? -1 : 1;
				muscle.movementAxis = Vector3.forward * sign;
				break;	
			}
			node2.transform.position = node1.transform.position + muscle.movementAxis * 0.5f;

			muscle.Spawn (gene);
			break;
		}
	}
		
	void FixedUpdate () {
		lifetime++;
	}

	public ProunGenome.NodeGene[] getGenome() {
		return gameObject.GetComponent<ProunGenome> ().getGenome ();
	}

	public string getGenomeString() {
		return gameObject.GetComponent<ProunGenome> ().getGenomeString ();
	}

	public ProunGenome.NodeGene[] spliceGenome(int geneIndex) {
		return gameObject.GetComponent<ProunGenome> ().spliceGenome (geneIndex);
	}

	public float getFitness() {
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