using UnityEngine;
using UnityEngine.Assertions;

public abstract class Gene {
	public enum JointType {
		SPRING_JOINT,
		NUM_JOINTS, // Always keep at end, everything underneath is hidden
		FIXED_JOINT,
		HINGE_JOINT,
	};

	abstract public void Mutate();
	abstract public int GetGIN();
}

public class MuscleGene : Gene {
	public int originNode;
	public int connectedNode;

	public JointType jointType;

	public float heartBeat;
	public float contractTime;
	public float contractedLength;
	public float extensionDistance;
	public Vector3 axis;

	public MuscleGene(int _originNode, int _connectedNode) {
		Assert.IsTrue (_originNode < _connectedNode, "Cannot connect a muscle from " + _originNode + " to " + _connectedNode + "!");

		originNode = _originNode;
		connectedNode = _connectedNode;

		heartBeat = Utility.genFloat(0.8f, 2f);
		contractTime = Utility.genFloat(0.25f, 0.75f);
		contractedLength = 0.0f;
		extensionDistance = Utility.genFloat(0.5f, 1.5f);
		jointType = (JointType) Utility.genInt ((int) JointType.NUM_JOINTS);
		axis = Utility.genAxis ();
	}

	public override void Mutate() {
		return;
	}

	public override int GetGIN() {
		return 0;
	}
}

public class NodeGene : Gene {
	public int index;
	public int body_type;
	public float mass;
	public float d_friction;
	public float s_friction;
	public int material;
	public Vector3 scale;

	public NodeGene(int _index) {
		index = _index;
		body_type = Utility.genInt(ProunGenome.nodeBodies.Length);
		mass = Utility.genFloat(0.3f, 0.9f);
		d_friction = Utility.genFloat(0.2f, 0.8f);
		s_friction = Utility.genFloat(0.2f, 0.8f);
		scale = new Vector3(Utility.genFloat(), Utility.genFloat(), Utility.genFloat()) * 5; 
		material = Utility.genInt(3);
	}

	public override void Mutate() {
		return;
	}

	public override int GetGIN() {
		return 0;
	}
}