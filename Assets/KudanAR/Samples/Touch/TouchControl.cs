using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Class that takes touch input and uses it to rotate and scale objects and activate a script.
/// </summary>
public class TouchControl : MonoBehaviour 
{
	#if UNITY_ANDROID || UNITY_IOS
	/// <summary>
	/// The rate at which the pinch control scales the object.
	/// </summary>
	float zoomSpeed;

	/// <summary>
	/// The rate at which the swipe control rotates the object.
	/// </summary>
	float moveSpeed;

	/// <summary>
	/// The amount that the finger can move on the screen before it is considered to be a movement and not a tap.
	/// </summary>
	float roughDiff;

	/// <summary>
	/// The position in screen coordinates (X,Y) that the finger started touching the screen.
	/// </summary>
	Vector2 startPos;

	/// <summary>
	/// The position in screen coordinates (X,Y) that the finger stopped touching teh screen.
	/// </summary>
	Vector2 endPos;

	/// <summary>
	/// The script that activates when the sreen is tapped.
	/// </summary>
	ActivatedScript activate;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		zoomSpeed = 0.5f;
		moveSpeed = 2f;
		roughDiff = 3f;

		startPos = new Vector2 (0, 0);
		endPos = new Vector2 (0, 0);

		activate = this.GetComponent<ActivatedScript>();
		activate.enabled = false;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		processDrag ();
		processPinch ();
		processTap ();
	}

	/// <summary>
	/// Checks for pinch controls.
	/// </summary>
	void processPinch ()
	{
		if (Input.touchCount == 2)
		{
			//Store inputs
			Touch fing1 = Input.GetTouch (0);
			Touch fing2 = Input.GetTouch (1);

			if (fing1.phase == TouchPhase.Moved || fing2.phase == TouchPhase.Moved)	//If either finger has moved since the last frame
			{
				//Get previous positions
				Vector2 fing1Prev = fing1.position - fing1.deltaPosition;
				Vector2 fing2Prev = fing2.position - fing2.deltaPosition;

				//Find vector magnitude between touches in each frame
				float prevTouchDeltaMag = (fing1Prev - fing2Prev).magnitude;		
				float touchDeltaMag = (fing1.position - fing2.position).magnitude;

				//Find difference in distances
				float deltaDistance = prevTouchDeltaMag - touchDeltaMag;

				//Create appropriate vector
				float scaleChange = ((this.transform.localScale.x - deltaDistance) * zoomSpeed);

				//Scale object
				this.transform.localScale = new Vector3 (scaleChange, scaleChange, scaleChange);
			}
		}
	}

	/// <summary>
	/// Checks for drag controls.
	/// </summary>
	void processDrag()
	{
		if (Input.touchCount == 1) 
		{
			//Store input
			Touch fing = Input.GetTouch (0);

			if(fing.phase == TouchPhase.Moved)	//If the finger has moved since the last frame
			{
				//Find the amount the finger has moved, and apply a rotation to this gameobject based on that amount
				Vector3 rotation = this.transform.rotation.eulerAngles;
				Vector2 fingMove = fing.deltaPosition;
				float deltaX = fingMove.y * moveSpeed;
				float deltaY = fingMove.x * moveSpeed;

				rotation.x -= deltaX;
				rotation.y -= deltaY;

				this.transform.rotation = Quaternion.Euler (rotation);
			}
		}
	}

	/// <summary>
	/// Checks for tap controls.
	/// </summary>
	void processTap()
	{
		if (Input.touchCount == 1) 
		{
			//Store input
			Touch fing = Input.GetTouch (0);
		
			if (fing.phase == TouchPhase.Began)	//If the finger started touching the screen this frame
			{
				if(!EventSystem.current.IsPointerOverGameObject(fing.fingerId))	//And the finger on the screen is not currently touching an object
					startPos = fing.position;	//Get the screen position of the finger when it hit the screen
			} 
			else if (fing.phase == TouchPhase.Ended)	//If the finger stopped touching the screen this frame
			{
				endPos = fing.position;			//Get the screen position of the finger when it left the screen

				if (Mathf.Abs(endPos.magnitude - startPos.magnitude) < roughDiff)	//Calculate how far away the finger was from its starting point when it left the screen
				{
					activate.enabled = !activate.enabled;	//And if it left the screen roughly where it started, it's a tap
				}
			}
		}
	}
	#endif
}