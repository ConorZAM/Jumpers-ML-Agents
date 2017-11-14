using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperAgent : Agent {

	// Joints in the model
	// Linear spring
	ConfigurableJoint confJoint;
	// Hinge joint
	HingeJoint hinge;
	float hingeTargetAngle;	// This is the angle the joint will aim for but not immediately change to
	// Spring inside hinge joint
	JointSpring jsHinge = new JointSpring();
	float maxDeltaAngle;


	// Rigidbodies
	Rigidbody body;
	Rigidbody foot;

	// Transforms
	Transform bodyTr;
	Transform bodyAnchor;

	// Reset store
	Vector3[] partPositions;
	Quaternion[] partRotations;



	void Start(){
		maxDeltaAngle = 15f;
		hinge = GetComponentInChildren<HingeJoint> ();
		confJoint = GetComponentInChildren<ConfigurableJoint> ();
		jsHinge = hinge.spring;

		body = transform.Find ("Body").GetComponent<Rigidbody> ();
		foot = transform.Find ("Foot").GetComponent<Rigidbody> ();
		bodyAnchor = transform.Find ("Body anchor");

		bodyTr = body.transform;

		partPositions = new Vector3[transform.childCount];
		partRotations = new Quaternion[transform.childCount];

		for(int i = 0; i < transform.childCount; i++){
			partPositions [i] = transform.GetChild (i).position;
			partRotations [i] = transform.GetChild (i).rotation;
		}

		confJoint.targetPosition = Vector3.up * Random.Range(-0.2f, 0.2f);
		hingeTargetAngle = Random.Range (-180f, 180f);
	}





	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		// Position and velocities
		state.Add(body.velocity.z / 5f);
		state.Add(body.velocity.y / 5f);
		state.Add (bodyAnchor.eulerAngles.x / 360f);
		state.Add (bodyTr.position.y - 0.175f);

		// Joint info
		state.Add(confJoint.targetPosition.y);
		state.Add (hinge.angle / 180f);

		return state;
	}

	public override void AgentStep(float[] act)
	{
		// hinge target position = act[0]
		// linear spring target position = act[1]
		hingeTargetAngle = Mathf.Clamp(act[0], -180, 180);

		float targetPos = Mathf.Clamp (act [1] / 10f, -0.2f, 0.2f);
		confJoint.targetPosition = Vector3.up * targetPos;
	}

	void FixedUpdate(){

		// Joint motion
		jsHinge.targetPosition = Mathf.MoveTowards (jsHinge.targetPosition, hingeTargetAngle, maxDeltaAngle);
		hinge.spring = jsHinge;

		// Rewards
		reward += foot.velocity.z * Time.fixedDeltaTime;
		reward -= Mathf.Abs (hinge.currentTorque.x * 0.001f) + Mathf.Abs (confJoint.currentForce.y * 0.00001f);
	
		// Done or reset condition
		if(bodyTr.position.y < 0.04f){
			done = true;
		}
	}

	public override void AgentReset()
	{
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			child.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			child.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			child.position = partPositions [i];
			child.rotation = partRotations [i];
		}

		// Add some randomness for the model
		confJoint.targetPosition = Vector3.up * Random.Range(-0.2f, 0.2f);
		hingeTargetAngle = Random.Range (-180f, 180f);
	}

	public override void AgentOnDone()
	{

	}
}
