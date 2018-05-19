using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuscleController : MonoBehaviour {
	/*
	 * The muscle acts on each body symmetrically,
	 * so from a physical standpoint [source] and
	 * [dest] can be chose arbitrarily. However,
	 * due to the way both genomes and Unity's
	 * physics are programmed, it is more convenient
	 * to have an explicit source and destination for 
	 * the muscle.
	 */
	public Rigidbody source;
	public Rigidbody dest;

	/*
	 * The ratio between [pullMin, pullMax] and [push]
	 * dictates the [maximum, minimum] lengths of this
	 * muscle.
	 */
	public float pullMin = 20;
	public float pullMax = 45;
	public float push = 100;

	/*
	 * Every [heartBeat] seconds, the muscle will contract once
	 * for [contractTime] seconds. [contractOffs] offsets the 
	 * contraction within this rhythm so that not all muscles
	 * with the same heart beat are contracting at the same time.
	 */
	public int heartBeat = 3;
	public int contractTime = 2;
	private int contractOffs;

	/*
	 * This should be set to true if and only if the muscle
	 * is not being defined programatically. This will
	 * spawn the muscle as soon as it is defined.
	 */
	public bool spawnAutomatically = false;

	private bool spawned = false;

	private SpringJoint springJoint;

	void Start () {
		if (spawnAutomatically)
			spawn ();
	}

	/*
	 * This puts the muscle between [source] and [dest].
	 * The muscle is basically a spring between this GameObject
	 * and [dest]. This GameObject is attached to [src] by a 
	 * fixed joint. The reason we do this is to allow the nodes
	 * to have multiple muscles attached to them. Unity only likes
	 * to have one spring joint per object, or else the physics
	 * gets scary, so this object acts as a proxy.
	 */
	public void spawn() {
		contractOffs = Utility.genInt (heartBeat - contractTime);

		springJoint = gameObject.GetComponent<SpringJoint> ();

		this.transform.position = source.transform.position;
//		this.transform.parent = source.transform;
//		this.transform.localPosition = Vector3.zero;
//
		gameObject.GetComponent<FixedJoint> ().connectedBody = source;

		gameObject.GetComponent<SpringJoint> ().connectedBody = dest;

		spawned = true;
	}

	/*
	 * Here we expand and contract the 'muscle' by increasing and 
	 * decreasing the strength of the spring joint while simultaneously
	 * pushing [source] and [dest] away from each other. This serves
	 * to exploit the spring's tension to keep the objects at a stable
	 * (yet varying) distance from each other.
	 */
	void FixedUpdate () {
		if (!spawned)
			return;

		Vector3 pushForce = (dest.position - source.position).normalized * push;
		source.AddForce (-pushForce);
		dest.AddForce (pushForce);

		float spring = pullMin;
		int hbTime = (int) Time.time % heartBeat; // Convert to 'heart beat time'
		if (hbTime > contractOffs && hbTime < contractOffs + contractTime)
			spring = pullMax; // Contract!

		springJoint.spring = spring;
	}
}
