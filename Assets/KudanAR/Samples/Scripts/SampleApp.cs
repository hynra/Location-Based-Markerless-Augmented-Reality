using UnityEngine;
using System.Collections;

namespace Kudan.AR.Samples
{
	/// <summary>
	/// Script used in the Kudan Samples. Provides functions that switch between different tracking methods and start abitrary tracking.
	/// </summary>
	public class SampleApp : MonoBehaviour
	{
        public KudanTracker _kudanTracker;	// The tracker to be referenced in the inspector. This is the Kudan Camera object.
        public TrackingMethodMarker _markerTracking;	// The reference to the marker tracking method that lets the tracker know which method it is using
        public TrackingMethodMarkerless _markerlessTracking;	// The reference to the markerless tracking method that lets the tracker know which method it is using

        public void MarkerClicked()
        {
            _kudanTracker.ChangeTrackingMethod(_markerTracking);	// Change the current tracking method to marker tracking
        }

        public void MarkerlessClicked()
        {
            _kudanTracker.ChangeTrackingMethod(_markerlessTracking);	// Change the current tracking method to markerless tracking
        }

        public void StartClicked()
        {
            // from the floor placer.
            Vector3 floorPosition;			// The current position in 3D space of the floor
            Quaternion floorOrientation;	// The current orientation of the floor in 3D space, relative to the device

            _kudanTracker.FloorPlaceGetPose(out floorPosition, out floorOrientation);	// Gets the position and orientation of the floor and assigns the referenced Vector3 and Quaternion those values
            _kudanTracker.ArbiTrackStart(floorPosition, floorOrientation);				// Starts markerless tracking based upon the given floor position and orientations
        }
	}
}
