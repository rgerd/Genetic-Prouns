using UnityEngine;
using UnityEngine.Assertions;
using System;

[Serializable]
public class MutationParams {
	public MuscleGene.MutationParams muscleGeneMutationParams;
}

[Serializable]
public struct FloatMutationSettings {
	[Range(0.0f, 1.0f)]
	public float probability;
	[Range(0.0f, 1.0f)]
	public float amount;
}

[Serializable]
public struct FlagMutationSettings {
	[Range(0.0f, 1.0f)]
	public float probability;
}

public abstract class Gene {
	public enum JointType {
		Spring,
		NumJoints, // Always keep at end, everything underneath is hidden
		Fixed,
		Hinge,
	};

	public enum EnableMode {
		Enabled,
		Limp,
		Disabled,
		NumEnableModes
	}

	abstract public int GetGIN();
}

[Serializable]
public class MuscleGene : Gene {
	public int originNode;
	public int connectedNode;

	public JointType jointType;

	public float heartBeat;
	public float contractTime;
	public float contractedLength;
	public float extensionDistance;
	public Vector3 axis;
	public EnableMode enableMode;

	[Serializable]
	public struct MutationParams {
		public FlagMutationSettings shouldMutate;
		public FloatMutationSettings heartBeat;
		public FloatMutationSettings contractTime;
		public FloatMutationSettings extensionDistance;
		public FlagMutationSettings axis;
		public FlagMutationSettings enableMode;
	}

	private static Range<float> heartBeatRange = new Range<float>(0.5f, 2.5f);
	private static Range<float> contractTimeRange = new Range<float>(0.25f, 0.75f);
	private static Range<float> extensionDistanceRange = new Range<float>(0.5f, 4.5f);

	public MuscleGene(int _originNode, int _connectedNode) {
		Assert.IsTrue (_originNode < _connectedNode, "Cannot connect a muscle from " + _originNode + " to " + _connectedNode + "!");

		originNode = _originNode;
		connectedNode = _connectedNode;

		heartBeat = Utility.genFloat(heartBeatRange);
		contractTime = Utility.genFloat(contractTimeRange);
		contractedLength = 0.0f;
		extensionDistance = Utility.genFloat(extensionDistanceRange);
		jointType = (JointType) Utility.genInt ((int) JointType.NumJoints);
		axis = Utility.genAxis ();
		enableMode = EnableMode.Enabled;
	}

	public MuscleGene Enable() {
		enableMode = EnableMode.Enabled;
		return this;
	}

	public MuscleGene Limp() {
		enableMode = EnableMode.Limp;
		return this;
	}

	public MuscleGene Disable() {
		enableMode = EnableMode.Disabled;
		return this;
	}

	public Gene Mutate(MutationParams mutationParams) {
		if (!Utility.flipCoin (mutationParams.shouldMutate.probability)) {
			return this;
		}

		if (Utility.flipCoin (mutationParams.axis.probability)) {
			this.axis = Utility.genAxis ();
		}

		if (Utility.flipCoin (mutationParams.enableMode.probability)) {
			this.enableMode = (EnableMode)Utility.genInt ((int) EnableMode.NumEnableModes);
		}

		if (Utility.flipCoin (mutationParams.heartBeat.probability)) {
			this.heartBeat = Utility.nudgeFloat (this.heartBeat, heartBeatRange, mutationParams.heartBeat.amount);
		}

		if (Utility.flipCoin (mutationParams.contractTime.probability)) {
			this.contractTime = Utility.nudgeFloat (this.contractTime, contractTimeRange, mutationParams.contractTime.amount);
		}

		if (Utility.flipCoin (mutationParams.extensionDistance.probability)) {
			this.extensionDistance = Utility.nudgeFloat (this.extensionDistance, extensionDistanceRange, mutationParams.extensionDistance.amount);
		}

		return this;
	}

	public override int GetGIN() {
		return 0;
	}
}

[Serializable]
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
		scale = new Vector3(Utility.genFloat() + 0.5f, Utility.genFloat() + 0.5f, Utility.genFloat() + 0.5f) * 6; 
		material = Utility.genInt(3);
	}

	public Gene Mutate(MutationParams mutationParams) {
		return this;
	}

	public override int GetGIN() {
		return 0;
	}
}