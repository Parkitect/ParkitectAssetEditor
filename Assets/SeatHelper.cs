using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SeatHelper : MonoBehaviour {

	// Use this for initialization
    void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.05f);
		Vector3 leftKnee = transform.position - transform.up * 0.02f + transform.forward * 0.078f - transform.right * 0.045f;
		Gizmos.DrawSphere(leftKnee, 0.03f);

		Vector3 rightKnee = transform.position - transform.up * 0.02f + transform.forward * 0.078f + transform.right * 0.045f;
		Gizmos.DrawSphere(rightKnee, 0.03f);

		Vector3 head = transform.position + transform.up * 0.305f + transform.forward * 0.03f;
		Gizmos.DrawSphere(head, 0.1f);

		Vector3 leftFoot = leftKnee + transform.forward * 0.015f - transform.up * 0.07f;
		Gizmos.DrawSphere(leftFoot, 0.02f);

		Vector3 rightFoot = rightKnee + transform.forward * 0.015f - transform.up * 0.07f;
		Gizmos.DrawSphere(rightFoot, 0.02f);
	}
}
