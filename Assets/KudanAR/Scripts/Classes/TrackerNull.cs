using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	/// <summary>
	/// Tracker for null platform
	/// Used on platforms where tracker isn't supported
	/// Also used as a reference for implementing tracker support on other platforms
	/// </summary>
	public class TrackerNull : TrackerBase
	{
		public override bool InitPlugin()
		{
			// Return true/false whether native plugin could initialise
			return true;
		}

		public override void DeinitPlugin()
		{
			// When this is called the input adn tracking will already have been stopped.
			// Remember to Texture2D.Destroy() any created textures here.
		}

		public override float GetNativePluginVersion()
		{
			// Return the version code from the native plugin
			// The native plugin version could be different to the overall plugin
			return 0.0f;
		}

		// Application
		public override void OnApplicationFocus( bool focusStatus )
		{
		}

		public override void OnApplicationPause( bool pauseStatus )
		{
		}

		public override void SetApiKey(string key, string bundleId)
		{
			// Pass these values to the native plugin to handle
		}
		
		public override int GetNumCameras()
		{
			// Return the number of usable cameras
			return 0;
		}

		public override bool AddTrackable(byte[] data, string id)
		{
			// Add the trackable data to the native plugin
			// Also add a Trackable object to the _trackables List<Trackable>()
			return false;
		}

		public override bool GetMarkerRecoveryStatus() 
		{
			return false;
		}

		public override void SetMarkerRecoveryStatus (bool status)
		{

		}

		public override bool StartInputFromImage(Texture2D image)
		{
			// Doesn't need to be implemented, just for testing using a static image
			return false;
		}

		public override bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight)
		{
			// Device index can be ignored on mobile, presumably they would alway use the forward camera
			return false;
		}

		public override void StopInput()
		{
			// Turn off the camera
			// An app may want to StopTracking() but still keep the camera feed displaying, this is why it is controlled separately
		}

		public override void StartTracking()
		{
			// Start tracking, including any thread creation
			throw new System.NotImplementedException();
		}

		public override void StopTracking()
		{
			// Stop all heavy tracking processing
			// Tracking can be resumed by calling StartTracking()
			throw new System.NotImplementedException();
		}

		public override bool EnableTrackingMethod(int trackingMethodId)
		{
			return false;
		}

		public override bool DisableTrackingMethod(int trackingMethodId)
		{
			return false;
		}

		public override void UpdateTracking()
		{
			// This is called once per frame

			// This function doesn't do anything if the native frame processor hasn't completed since the last call
			// At the end of this function _detected and _projectionMatrix should be updated
			// This function should also update the frame rate variables, eg _cameraRate, _appRate.
		}
		
		public override void PostRender()
		{
		}

		public override void ArbiTrackStart(Vector3 position, Quaternion orientation)
		{
		}
		
		public override bool ArbiTrackIsTracking()
		{
			return false;
		}
		
		
		public override void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
		{
			position = new Vector3 ();
			orientation = new Quaternion ();
		}
		
		public override void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
		{
			position = new Vector3 ();
			orientation = new Quaternion ();
		}
	}
};