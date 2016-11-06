using UnityEngine;
using System.Collections;

/// <summary>
/// Method of movement used in the flying sample. This method changes an object's transform to move it towards a target point in 3D space.
/// </summary>
public class FlyBehaviour_TargetTransform : MonoBehaviour 
{
	/// <summary>
	/// The distance between this object and its brother.
	/// </summary>
	float distance;

	/// <summary>
	/// The minimum distance the this object must be from its brother.
	/// </summary>
	public float minThresh = 100f;

	/// <summary>
	/// The maximum distance this object can be from its brother.
	/// </summary>
	public float maxThresh = 500f;

	/// <summary>
	/// If the distance between this object and its target point is less than this threshold, it is considered to have reached its target.
	/// </summary>
	public float moveThresh = 10f;

	/// <summary>
	/// The target point in 3D space to move towards.
	/// </summary>
	Vector3 targetVec;

	/// <summary>
	/// The amount that this object moves each frame.
	/// </summary>
	public float moveStep = 1.5f;

	/// <summary>
	/// The maximum distance each component of the target point can be away from this object.
	/// </summary>
	public float maxMove = 20f;

	/// <summary>
	/// Reference to the GameObject that this object must move around.
	/// </summary>
	public GameObject brother;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		distance = 0f;
		targetVec = new Vector3 (0, 0, 0);
		SetTarget ();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (isTargetReached ())		// If the point that was set last has been reached
		{
			distance = Vector3.Distance (this.transform.position, brother.transform.position);	// Calculate the distance between this object and its "Brother"
			SetTarget ();																		// Set a new target
		} 
		else 
		{
			this.transform.position = Vector3.MoveTowards(this.transform.position,targetVec, moveStep);	// If the target point has not been reached, move this object towards the target
		}
	}

	/// <summary>
	/// Sets a new target point.
	/// </summary>
	void SetTarget()
	{
		if (minThresh < distance && distance < maxThresh)										// If the distance is between the minimum and maximum thresholds
		{
			if (Random.value > 0.5) {
				targetVec.x = (this.transform.position.x + Random.Range (0, maxMove));			// If a random number between 0 and 1 is greater than 0.5, set a component of the target vector to be a random distance away from this object, between 0 and the maximum move distance
			} else
				targetVec.x = (this.transform.position.x + Random.Range ((maxMove * -1), 0));	// If the random number between 0 and 1 is not greater than 0.5, set the component a random number between 0 and the maximum move distance in the opposite direction

			if (Random.value > 0.5) {
				targetVec.y = (this.transform.position.y + Random.Range (0, maxMove));
			} else
				targetVec.y = (this.transform.position.y + Random.Range ((maxMove * -1), 0));

			if (Random.value > 0.5) {
				targetVec.z = (this.transform.position.z + Random.Range (0, maxMove));
			} else
				targetVec.z = (this.transform.position.z + Random.Range ((maxMove * -1), 0));
		}
		else if (distance > maxThresh) 
		{
			targetVec = Vector3.MoveTowards (this.transform.position, brother.transform.position, Random.Range(0, maxMove));		// If the two objects are too far away, make the target vector somewhere between this and the other object
		} 
		else if (distance < minThresh) 
		{
			targetVec = Vector3.MoveTowards (this.transform.position, brother.transform.position, Random.Range(maxMove * -1, 0));	// If the two objects are too close, make the target vector somewhere away from the other object
		}
	}

	/// <summary>
	/// Has the target been reached?
	/// </summary>
	/// <returns><c>true</c>, if target has been reached, <c>false</c> otherwise.</returns>
	bool isTargetReached()
	{
		bool reached;	// Has the target been reached?

		reached = (Vector3.Distance (this.transform.position, targetVec) < moveThresh) ? true : false;	// If the distance between this object and the target point is within the move threshold, the target has been reached

		return reached;
	}
}