using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace Kudan.AR
{
	/// <summary>
	/// Class that takes touch input and uses it to start tracking and rotate objects, removing the need for UI buttons when using Markerless Tracking.
	/// It is different from the TouchControl class because Touch Control contains pinch control handling, rotation along multiple axes and does not start tracking on tap input.
	/// </summary>
	public class MarkerlessTouch : MonoBehaviour 
	{
		/// <summary>
		/// Reference to the Kudan Tracker.
		/// </summary>
		public KudanTracker tracker;

		/// <summary>
		/// The object that moves with user input.
		/// </summary>
		public GameObject interactableObject;

		/// <summary>
		/// The speed at which swipe controls rotate the object.
		/// </summary>
		float moveSpeed;

		/// <summary>
		/// The distance that a finger can move across the screen before it is considered to be moving and not tapping.
		/// </summary>
		float roughDiff;

		/// <summary>
		/// Was the control a tap?
		/// </summary>
		bool tap;

		/// <summary>
		/// The position in screen coordinates (X,Y) that the finger started touching the screen.
		/// </summary>
		Vector2 startPos;

		/// <summary>
		/// The position in screen coordinates (X,Y) that the finger stopped touching the screen.
		/// </summary>
		Vector2 endPos;

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start()
		{
			moveSpeed = 2f;
			roughDiff = 3f;
			tap = false;

			startPos = new Vector2 (0, 0);
			endPos = new Vector2 (0, 0);
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update()
		{
			#if UNITY_IOS || UNITY_ANDROID
			processDrag ();
			processTap ();
			#endif
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
					Vector2 fingMove = fing.deltaPosition;

					float deltaY = (fingMove.x * moveSpeed * -1);

					interactableObject.transform.Rotate (0, deltaY, 0);
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
						tap = true;	//And if it left the screen roughly where it started, it's a tap
					}
				}
			}

			if (tap && !tracker.ArbiTrackIsTracking()) 
			{
				Vector3 floorPos;
				Quaternion floorRot;

				tracker.FloorPlaceGetPose (out floorPos, out floorRot);
				tracker.ArbiTrackStart (floorPos, floorRot);

				tap = false;
			}
		}
	}
}