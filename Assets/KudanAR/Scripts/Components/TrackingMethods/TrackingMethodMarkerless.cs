using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Kudan AR/Tracking Methods/Markerless Tracking")]
	/// <summary>
	/// The Markerless Tracking Method. This method tracks objects using arbitrary tracking.
	/// </summary>
	public class TrackingMethodMarkerless : TrackingMethodBase
	{
		/// <summary>
		/// The name of this tracking method.
		/// </summary>
		/// <value>The name.</value>
		public override string Name
		{
			get { return "Markerless"; }
		}

		/// <summary>
		/// The ID of this tracking method.
		/// </summary>
		/// <value>The tracking method identifier.</value>
		public override int TrackingMethodId
		{
			get { return 1; }
		}

		/// <summary>
		/// The update marker event.
		/// </summary>
		public MarkerUpdateEvent _updateMarkerEvent;

		/// <summary>
		/// The ArbiTrack floor depth. Default value of 200.f
		/// </summary>
		public float _floorDepth = 200.0f;

		/// <summary>
		/// Processes the current frame.
		/// </summary>
		public override void ProcessFrame()
		{
            Vector3 position;
            Quaternion orientation;

            _kudanTracker.ArbiTrackGetPose(out position, out orientation);

            Trackable trackable = new Trackable();
            trackable.position = position;
            trackable.orientation = orientation;

            trackable.isDetected = _kudanTracker.ArbiTrackIsTracking();

            _updateMarkerEvent.Invoke(trackable);
		}

		public override void StartTracking()
		{
			_kudanTracker.SetArbiTrackFloorHeight (_floorDepth);

			base.StartTracking ();
		}

        /// <summary>
		/// Stops tracking.
		/// </summary>
        public override void StopTracking()
        {
            base.StopTracking();
            Trackable trackable = new Trackable();
            trackable.isDetected = false;

            _updateMarkerEvent.Invoke(trackable);
        }
	}
}
