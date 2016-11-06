using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]	// The object this script is attached to needs to have a rigidbody to work
/// <summary>
/// One method of movement used in the flying sample. This method applies velocities to rigidbodies.
/// </summary>
public class FlyBehaviour_RigidbodyVelocity : MonoBehaviour 
{
	/// <summary>
	/// The distance between this object and its brother.
	/// </summary>
	float distance;					

	/// <summary>
	/// Minimum distance the objects must be apart.
	/// </summary>
	public float minThresh = 150f;

	/// <summary>
	/// Maximum distance the objects can be apart.
	/// </summary>
	public float maxThresh = 250f;

	/// <summary>
	/// Time passed since the last switch
	/// </summary>
	/// 
	float currentTime;				
	/// <summary>
	/// Amount of time that needs to pass before the objects change direction.
	/// </summary>
	public float switchTime = 0.5f;	

	/// <summary>
	/// The current velocity being applied to the rigidbody.
	/// </summary>
	Vector3 currentVel;			

	/// <summary>
	/// The maximum value each component of the vector can be.
	/// </summary>
	public float topSpeed = 20f;

	/// <summary>
	/// Reference to the GameObject that the object this script is attached to must move around.
	/// </summary>
	public GameObject brother;	

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		distance = 1f;
		currentTime = 0f;
		currentVel = new Vector3 (0, 0, 0);
		SetVelocity (distance);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (currentTime < switchTime) 
		{
			currentTime += Time.deltaTime;	// If it is not time to switch, add the amount of time that has passed since the last frame
		}
		else
		{
			distance = Vector3.Distance (this.transform.position, brother.transform.position);	// If it is time to switch, calculate the distance between this object and its "Brother"

			SetVelocity (distance);	// Set the velocity vector of this object, based upon the distance between this object and its "Brother"

			this.GetComponent<Rigidbody> ().velocity = currentVel;	// Apply the velocity vector to the rigidbody of this object

			currentTime = 0f;		// Reset the timer until the next switch
		}
	}

	/// <summary>
	/// Set a new velocity for this object.
	/// </summary>
	/// <param name="distance">Distance.</param>
	void SetVelocity(float distance)
	{
		if (minThresh < distance && distance < maxThresh)			// If the distance is between the minimum and maximum thresholds
		{
			if (Random.value > 0.5) {
				currentVel.x = Random.Range (0, topSpeed);			// If a random number between 0 and 1 is greater than 0.5, assign a component of the velocity vector a random number between 0 and the top speed
			} else
				currentVel.x = Random.Range ((topSpeed * -1), 0);	// If the random number between 0 and 1 is not greater than 0.5, assign the component a random number between 0 and the top speed in the opposite direction 
		
			if (Random.value > 0.5) {
				currentVel.y = Random.Range (0, topSpeed);			 
			} else
				currentVel.y = Random.Range ((topSpeed * -1), 0);
		
			if (Random.value > 0.5) {
				currentVel.z = Random.Range (0, topSpeed);
			} else
				currentVel.z = Random.Range ((topSpeed * -1), 0);
		}
		else if (distance > maxThresh) 
		{
			currentVel = Vector3.MoveTowards (this.transform.position, brother.transform.position, topSpeed * Time.deltaTime);			// If the two objects are too far away, make them move towards one another
		} 
		else if (distance < minThresh) 
		{
			currentVel = Vector3.MoveTowards (this.transform.position, brother.transform.position, topSpeed * Time.deltaTime * -1);		// If the objects are too close to one another, make them move apart
		}
	}
}