using UnityEngine;
using System.Text;
using System.Collections;

namespace Kudan.AR
{
	/// <summary>
	/// The Marker Transform Driver, which is moved by the tracker when using the Marker Tracking Method.
	/// </summary>
	[AddComponentMenu("Kudan AR/Transform Drivers/Marker Based Driver")]
	public class MarkerTransformDriver : TransformDriverBase
	{
		/// <summary>
		/// Constant scale factor for resizing markers.
		/// </summary>
		private const float UnityScaleFactor = 10f;

		[Tooltip("Optional ID")]
		/// <summary>
		/// The ID of the marker needed to activate this transform driver.
		/// </summary>
		public string _expectedId;

		/// <summary>
		/// Whether or not to resize child objects of this transform driver.
		/// </summary>
		public bool _applyMarkerScale;

		[Header("Plane Drawing")]
		/// <summary>
		/// Whether or not to draw a box showing the space the marker takes up in the virtual world.
		/// </summary>
		public bool _alwaysDrawMarkerPlane = true;

		/// <summary>
		/// The width of the marker plane.
		/// </summary>
		public int _markerPlaneWidth;

		/// <summary>
		/// The height of the marker plane.
		/// </summary>
		public int _markerPlaneHeight;

		/// <summary>
		/// The ID of a detected trackable.
		/// </summary>
		private string _trackableId;

		/// <summary>
		/// Reference to the Marker Tracking Method.
		/// </summary>
		private TrackingMethodMarker _tracker;

		/// <summary>
		/// Finds the tracker.
		/// </summary>
		protected override void FindTracker()
		{
			_trackerBase = _tracker = (TrackingMethodMarker)Object.FindObjectOfType(typeof(TrackingMethodMarker));
		}

		/// <summary>
		/// Register this instance with the Tracking Method class for event handling.
		/// </summary>
		protected override void Register()
		{
			if (_tracker != null)
			{
				_tracker._foundMarkerEvent.AddListener(OnTrackingFound);
				_tracker._lostMarkerEvent.AddListener(OnTrackingLost);
				_tracker._updateMarkerEvent.AddListener(OnTrackingUpdate);

				this.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Unregister this instance with the Tracking Method class for event handling.
		/// </summary>
		protected override void Unregister()
		{
			if (_tracker != null)
			{
				_tracker._foundMarkerEvent.RemoveListener(OnTrackingFound);
				_tracker._lostMarkerEvent.RemoveListener(OnTrackingLost);
				_tracker._updateMarkerEvent.RemoveListener(OnTrackingUpdate);
			}
		}

		/// <summary>
		/// Raises the tracking found event.
		/// </summary>
		/// <param name="trackable">Trackable.</param>
		public void OnTrackingFound(Trackable trackable)
		{
			bool matches = false;
			if (_expectedId == trackable.name)
			{
				matches = true;
			}

			if (matches)
			{
				_trackableId = trackable.name;
				this.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Raises the tracking lost event.
		/// </summary>
		/// <param name="trackable">Trackable.</param>
		public void OnTrackingLost(Trackable trackable)
		{
			if (_trackableId == trackable.name)
			{
				this.gameObject.SetActive(false);
				_trackableId = string.Empty;
			}
		}

		/// <summary>
		/// Raises the tracking update event.
		/// </summary>
		/// <param name="trackable">Trackable.</param>
		public void OnTrackingUpdate(Trackable trackable)
		{
			if (_trackableId == trackable.name)
			{
				this.transform.localPosition = trackable.position;
				this.transform.localRotation = trackable.orientation;

				if (_applyMarkerScale)
				{
					this.transform.localScale = new Vector3(trackable.height / UnityScaleFactor, 1f, trackable.width / UnityScaleFactor);
				}
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Sets the scale of child objects using the marker size.
		/// </summary>
		public void SetScaleFromMarkerSize()
		{
			if (_markerPlaneWidth > 0 && _markerPlaneHeight > 0)
			{
				this.transform.localScale = new Vector3(_markerPlaneHeight / UnityScaleFactor, 1f, _markerPlaneWidth / UnityScaleFactor);
			}
		}

		/// <summary>
		/// Raises the draw gizmos selected event.
		/// </summary>
		void OnDrawGizmosSelected()
		{
			DrawPlane();
		}

		/// <summary>
		/// Raises the draw gizmos event.
		/// </summary>
		void OnDrawGizmos()
		{
			if (_alwaysDrawMarkerPlane)
			{
				DrawPlane();
			}
		}

		/// <summary>
		/// Draws the marker preview plane.
		/// </summary>
		private void DrawPlane()
		{
			Gizmos.matrix = this.transform.localToWorldMatrix;

			Vector3 size = Vector3.one * UnityScaleFactor;

			// In the editor mode use the user entered size values
			if (!Application.isPlaying)
			{
				if (_markerPlaneWidth > 0 && _markerPlaneHeight > 0)
				{
					size = new Vector3(_markerPlaneHeight, 0.01f, _markerPlaneWidth);
				}
				else
				{
					return;
				}
			}

			// Draw a flat cube to represent the area the marker would take up
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(Vector3.zero, size);
		}
#endif
	}
};