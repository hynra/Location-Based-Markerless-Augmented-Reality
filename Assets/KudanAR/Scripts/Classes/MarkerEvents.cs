using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	/// <summary>
	/// Class that tracks when a marker has been found, lost or tracked.
	/// </summary>
	public class MarkerEvents : MonoBehaviour 
	{
		/// <summary>
		/// Maximum number of markers that can trigger events at any one time.
		/// </summary>
		int numMaxEventTracking;

		/// <summary>
		/// The Marker Tracking Method used to get detected trackables
		/// </summary>
		public TrackingMethodMarker trackingMethodMarker;	

		/// <summary>
		/// Array of trackables
		/// </summary>
		Trackable[] trackables;								

		/// <summary>
		/// markerStruct contains a reference to a marker object, as well as a name and bools to check whether it is active this frame or was active last frame.
		/// </summary>
		public struct markerStruct
		{
			/// <summary>
			/// Reference to the marker object
			/// </summary>
			public GameObject marker;

			/// <summary>
			/// Name of the marker.
			/// </summary>
			public string name;		

			/// <summary>
			/// Is the marker being detcted in this frame?
			/// </summary>
			public bool isActive;

			/// <summary>
			/// Was the marker being detected in the last frame?
			/// </summary>
			public bool wasActive;
		}

		/// <summary>
		/// An array of objects that the MarkerTransformDrivers can be retrieved from.
		/// </summary>
		GameObject[] markerArray;

		/// <summary>
		/// An array of markerStructs to store data.
		/// </summary>
		markerStruct[] markerStructs;

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start()
		{
			// Initialise the markerStruct array with a constant size, so that null elements can be checked. This allows marker lost events to trigger. Default is currently 10.
			numMaxEventTracking = 10;
			markerStructs = new markerStruct[numMaxEventTracking];
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update ()
		{
			// Get all GameObjects in the scene that are tagged with the "Marker" tag.
			markerArray = GameObject.FindGameObjectsWithTag ("Marker");

			// Get all detected trackables found by the plugin and store them in an array.
			trackables = trackingMethodMarker.Plugin.GetDetectedTrackablesAsArray();

			if (markerArray.Length < numMaxEventTracking)
			{
				// Make sure that the size of markerArray matches the size of markerStructs
				// This avoids any "Out of Range" exceptions
				Resize (numMaxEventTracking, ref markerArray);
			}

			for (int i = 0; i < markerStructs.Length; i++)
			{
				// Place any marker GameObjects into the markerStruct array
				markerStructs [i].marker = markerArray [i];

				for (int j = 0; j < markerArray.Length; j++)
				{
					for (int k = 0; k < trackables.Length; k++)
					{
						if (markerArray [j] != null)
						{
							// If the expected ID and the marker ID match
							if (markerArray [j].GetComponent<MarkerTransformDriver> ()._expectedId == trackables [k].name)
							{
								// And the name has not already been assigned
								if (markerStructs [k].name == null)
								{
									// Assign the name of the marker to the name of the markerStruct element
									markerStructs [k].name = trackables [k].name;
								}
							}
						}
					}
				}

				// If the marker is null, it has either been lost or has not been found yet
				if (markerStructs [i].marker == null) 
				{
					// Either way, the marker is not currently active, so set isActive to false
					markerStructs [i].isActive = false;
					// Check if the marker has been lost this frame
					checkLost (markerStructs[i]);
					// Then set wasActive to false so the marker lost event does not trigger every frame
					markerStructs [i].wasActive = false;
				}

				// If the marker is active
				else 
				{
					// Set isActive to true
					markerStructs [i].isActive = true;
					// Check whether or not it was found this frame
					checkFound (markerStructs[i]);
					// Then set wasActive to true so the marker found event does not trigger every frame
					markerStructs [i].wasActive = true;
					// If the marker is active, it is currently being tracked
					checkTrack (markerStructs[i]);
				}
			}
		}

		/// <summary>
		/// Checks if the marker was found in this frame.
		/// </summary>
		/// <param name="markerStruct">Marker struct.</param>
		public void checkFound(markerStruct markerStruct)
		{
			// If the marker is active this frame, but was not active last frame, then it must have been found this frame
			if (markerStruct.isActive && !markerStruct.wasActive)
			{
				//name = marker.name;
				Debug.LogWarning ("Found Marker: " + markerStruct.name);
			}
		}

		/// <summary>
		/// Checks if the marker has been tracked in this frame.
		/// </summary>
		/// <param name="markerStruct">Marker struct.</param>
		public void checkTrack(markerStruct markerStruct)	
		{
			// If the marker is currently active, it is being tracked
			if (markerStruct.marker.activeInHierarchy) 
			{
				Debug.LogWarning ("Tracking Marker: " + markerStruct.name);
			} 
			// If the marker is no longer active, it must have been lost, so set the marker to null, allowing the marker lost event to fire
			else
				markerStruct.marker = null;
		}

		/// <summary>
		/// Checks if the marker has been lost in this frame.
		/// </summary>
		/// <param name="markerStruct">Marker struct.</param>
		public void checkLost(markerStruct markerStruct)
		{
			// If the marker is no longer active but was active last frame, it must have been lost this frame
			if (!markerStruct.isActive && markerStruct.wasActive)
			{
				Debug.LogWarning ("Lost Marker: " + markerStruct.name);
			}
		}
			
		/// <summary>
		/// Takes a specified size and a reference to an array, then changes the array to be that size.
		/// </summary>
		/// <param name="size">Size.</param>
		/// <param name="array">Array.</param>
		void Resize(int size, ref GameObject[] array)
		{
			GameObject[] temp = new GameObject[size];
			for (int i = 0; i < Mathf.Min (size, array.Length); i++) 
			{
				temp [i] = array [i];
			}

			array = temp;
		}
	}
}