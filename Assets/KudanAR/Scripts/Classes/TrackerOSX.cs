#if UNITY_EDITOR_OSX
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public class TrackerOSX : TrackerBase
	{
		private int _width, _height;
		private List<Trackable> _currentDetected = new List<Trackable> (8);

		public override bool InitPlugin ()
		{
			return NativeInterface.Init ();
		}

		public override void DeinitPlugin ()
		{
			NativeInterface.Deinit ();
		}

		public override float GetNativePluginVersion ()
		{
			return NativeInterface.GetPluginVersion ();
		}

		public override void OnApplicationFocus (bool focusStatus)
		{
		}

		public override void OnApplicationPause (bool pauseStatus)
		{
		}

		public override int GetNumCameras ()
		{
			return 0;
		}

		public override bool StartInputFromImage (Texture2D image)
		{
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput ();

			// Start new input
			if (wasTracking) {
				StartTracking ();
			}

			return true;
		}

		public override bool StartInputFromCamera (int deviceIndex, int targetWidth, int targetHeight)
		{
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput ();

			// Initialise the webcam. Change the argument to specify your webcam ID. Unfortunately this isn't very predictable
			// on OSX so trial and error is required.
			if (NativeInterface.WebCamInit (deviceIndex) == false) 
			{
				Debug.Log ("Couldn't open webcam");
				return false;
			}

			// Get webcam texture resolution.
			int[] resolution = new int[2];
			NativeInterface.WebCamGetResolution (resolution);

			_width = resolution [0];
			_height = resolution [1];

			// Create a new texture to hold it.
			_clonedTexture = new Texture2D (_width, _height, TextureFormat.RGBA32, false);

			if (wasTracking) {
				StartTracking ();
			}

			return true;			
		}

		public override void StopInput ()
		{
			StopTracking ();

			NativeInterface.WebCamDeinit ();
		}

		public override void SetApiKey (string key, string bundleId)
		{
			// Not implemented
		}

		// Trackables
		public override bool AddTrackable (byte[] data, string id)
		{
			GCHandle handle = GCHandle.Alloc (data, GCHandleType.Pinned);
			bool result = NativeInterface.AddTrackableSet (handle.AddrOfPinnedObject (), data.Length);
			handle.Free ();

			if (result) {
				Trackable trackable = new Trackable ();
				trackable.name = id;
				_trackables.Add (trackable);
			}

			return result;
		}

		public override bool GetMarkerRecoveryStatus() 
		{
			return NativeInterface.GetMarkerRecoveryStatus ();
		}

		public override void SetMarkerRecoveryStatus (bool status)
		{
			NativeInterface.SetMarkerRecoveryStatus (status);
		}

		private int _numFramesGrabbedLast;
		private int _numFramesTrackedLast;
		private int _numFramesProcessedLast;
		private int _numFramesRenderedLast;

		private int _numFramesGrabbed;
		private int _numFramesTracked;
		private int _numFramesProcessed;
		private int _numFramesRendered;
		private float _rateTimer;

		private void UpdateFrameRates ()
		{
			_rateTimer += Time.deltaTime;
			_numFramesRendered++;
			if (_rateTimer >= 1.0f) {
				_cameraRate = (float)(_numFramesGrabbed - _numFramesGrabbedLast) / _rateTimer;
				_trackerRate = (float)(_numFramesTracked - _numFramesTrackedLast) / _rateTimer;
				_appRate = (float)(_numFramesRendered - _numFramesRenderedLast) / _rateTimer;

				_numFramesGrabbedLast = _numFramesGrabbed;
				_numFramesTrackedLast = _numFramesTracked;
				_numFramesRenderedLast = _numFramesRendered;
				_rateTimer = 0f;
			}
		}

		public override void StartTracking ()
		{
			if (_isTrackingRunning) {
				Debug.LogWarning ("[KudanAR] Trying to start tracking when it's already running");
				return;
			}

			_isTrackingRunning = true;
		}

		public override void StopTracking ()
		{
			_isTrackingRunning = false;
		}

		public override bool EnableTrackingMethod (int trackingMethodId)
		{
			return NativeInterface.EnableTrackingMethod (trackingMethodId);
		}

		public override bool DisableTrackingMethod (int trackingMethodId)
		{
			return NativeInterface.DisableTrackingMethod (trackingMethodId);
		}


		public override void UpdateTracking ()
		{
			_numFramesGrabbed++;

			if (_numFramesGrabbed > _numFramesTracked) {
				System.IntPtr intPtr = new System.IntPtr ();
				NativeInterface.ProcessFrame (intPtr, _width, _height, 0);

				// Process the detected markers
				_currentDetected = GetDetected ();

				_numFramesTracked++;
			}

			// Only process the markers if a new tracking has completed
			if (_numFramesProcessed != _numFramesTracked) {
				_numFramesProcessed = _numFramesTracked;

				// Copy the list of detected objects, or make a new list of it's empty
				if (_currentDetected != null)
					_detected = _currentDetected;
				else
					_detected = new List<Trackable> (8);
				_currentDetected = new List<Trackable> (8);

				// Update projection matrix
				float[] projectionFloats = new float[16];

				NativeInterface.GetProjectionMatrix (_cameraNearPlane, _cameraFarPlane, projectionFloats);
				_projectionMatrix = ConvertNativeFloatsToMatrix (projectionFloats, (float)_width / (float)_height);
			}

			// Update our frame rates
			UpdateFrameRates ();
		}

		public override void PostRender ()
		{
		}

		private List<Trackable> GetDetected ()
		{
			// Grab detected trackables from nativeland into C# Unity land
			int num = NativeInterface.GetNumberOfDetectedTrackables ();
			List<Trackable> result = new List<Trackable> (num);
			for (int i = 0; i < num; i++) {
				Trackable trackable = new Trackable ();
				StringBuilder sbName = new StringBuilder (512);
				int width = 0;
				int height = 0;
				float[] p = new float[7];
				int trackingMethod = 0;
				if (NativeInterface.GetDetectedTrackable (i, p, ref width, ref height, ref trackingMethod, sbName)) {
					trackable.name = sbName.ToString ();
					trackable.width = width;
					trackable.height = height;
					trackable.position = ConvertNativeFloatsToVector3 (p [0], p [1], p [2]);
					trackable.orientation = ConvertNativeFloatsToQuaternion (p [3], p [4], p [5], p [6]);
					trackable.trackingMethod = trackingMethod;
					result.Add (trackable);
				}
			}
			return result;
		}
			

		public override void updateCam ()
		{
			System.IntPtr texturePtr = _clonedTexture.GetNativeTexturePtr ();
			long textureID = (long)texturePtr;

			NativeInterface.setTextureID (textureID);

			GL.IssuePluginEvent (NativeInterface.GetRenderEventFunc (), 0);
		}

		// Utility functions for converting native data into Unity data
		public Matrix4x4 ConvertNativeFloatsToMatrix (float[] r, float cameraAspect)
		{
			Matrix4x4 m = new Matrix4x4 ();
			m.SetRow (0, new Vector4 (r [0], r [1], r [2], r [3]));
			m.SetRow (1, new Vector4 (r [4], r [5], r [6], r [7]));
			m.SetRow (2, new Vector4 (r [8], r [9], r [10], r [11]));
			m.SetRow (3, new Vector4 (r [12], r [13], r [14], r [15]));

			// Scale the aspect ratio based on camera vs screen ratios
			float screenAspect = ((float)Screen.width / (float)Screen.height);

			cameraAspect = _width / (float)_height;

			float scale = cameraAspect / screenAspect;

			if (scale > 1f)
				m.m00 *= scale;
			else
				m.m11 /= scale;

			m = m.transpose;

			return m;
		}

		protected static Vector3 ConvertNativeFloatsToVector3 (float x, float y, float z)
		{
			return new Vector3 (x, y, -z);
		}

		protected static Quaternion ConvertNativeFloatsToQuaternion (float x, float y, float z, float w)
		{
			Quaternion q = new Quaternion (-x, -y, z, w) * Quaternion.AngleAxis (90f, Vector3.forward) * Quaternion.AngleAxis (90f, Vector3.left);
			return q;
		}

		public override void ArbiTrackStart (Vector3 position, Quaternion orientation)
		{
			
		}

		public override bool ArbiTrackIsTracking ()
		{
			return false;
		}


		public override void FloorPlaceGetPose (out Vector3 position, out Quaternion orientation)
		{
			position = new Vector3 ();
			orientation = new Quaternion ();
		}

		public override void ArbiTrackGetPose (out Vector3 position, out Quaternion orientation)
		{
			position = new Vector3 ();
			orientation = new Quaternion ();
		}
	}
};
#endif