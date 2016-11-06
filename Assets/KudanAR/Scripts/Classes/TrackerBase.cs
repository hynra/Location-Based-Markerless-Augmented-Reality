using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	/// <summary>
	/// Base class for the tracker plugin.  This abstracts the native plugin for each operating system.
	/// </summary>
	public abstract class TrackerBase : ITracker
	{
		/// <summary>
		/// The version of the plugin (scripts etc).  This is different to the version of the NATIVE plugin.
		/// </summary>
		private const string PluginVersionNumber = "1.3";

		/// <summary>
		/// List of trackables the user has loaded.
		/// </summary>
		protected List<Trackable> _trackables = new List<Trackable>(8);

		/// <summary>
		/// The default camera near plane value.
		/// </summary>
		protected float _cameraNearPlane = 0.3f;

		/// <summary>
		/// The default camera far plane value.
		/// </summary>
		protected float _cameraFarPlane = 1000f;

		/// <summary>
		/// ArbiTracker default floor height.
		/// </summary>
		protected float _floorHeight = 200.0f;


		/// <summary>
		/// The tracking thread.
		/// </summary>
		protected System.Threading.Thread _trackingThread;

		/// <summary>
		/// Is tracking currently running?
		/// </summary>
		protected bool _isTrackingRunning;

		/// <summary>
		/// The texture used to render to the camera.
		/// </summary>
		protected Texture _finalTexture;

		/// <summary>
		/// The projection matrix.
		/// </summary>
		protected Matrix4x4 _projectionMatrix;

		/// <summary>
		/// List of detected trackables.
		/// </summary>
		protected List<Trackable> _detected = new List<Trackable>(8);

		/// <summary>
		/// The camera rate, number of times the camera feed refreshes each second.
		/// </summary>
		protected float _cameraRate;

		/// <summary>
		/// The tracker rate, the number of times the tracker updates each second.
		/// </summary>
		protected float _trackerRate;

		/// <summary>
		/// The app rate, the number of times the app updates each second.
		/// </summary>
		protected float _appRate;

		/// <summary>
		/// The cloned texture.
		/// </summary>
		protected Texture _clonedTexture;

		/// <summary>
		/// Gets the camera frame rate.
		/// </summary>
		/// <value>The camera frame rate.</value>
		public float CameraFrameRate
		{
			get { return _cameraRate; }
		}

		/// <summary>
		/// Gets the tracker frame rate.
		/// </summary>
		/// <value>The tracker frame rate.</value>
		public float TrackerFrameRate
		{
			get { return _trackerRate; }
		}

		/// <summary>
		/// Gets the app frame rate.
		/// </summary>
		/// <value>The app frame rate.</value>
		public float AppFrameRate
		{
			get { return _appRate; }
		}

		/// <summary>
		/// Gets the current plugin version.
		/// </summary>
		/// <returns>The plugin version.</returns>
		public string GetPluginVersion()
		{
			return PluginVersionNumber;
		}

		/// <summary>
		/// Gets the number of trackables.
		/// </summary>
		/// <returns>The number of trackables.</returns>
		public int GetNumTrackables()
		{
			return _trackables.Count;
		}

		/// <summary>
		/// Gets a trackable at given index.
		/// </summary>
		/// <returns>A trackable.</returns>
		/// <param name="index">Index.</param>
		public Trackable GetTrackable(int index)
		{
			return _trackables[index];
		}

		/// <summary>
		/// Determines whether tracking is running.
		/// </summary>
		/// <returns><c>true</c> if tracking is running; otherwise, <c>false</c>.</returns>
		public bool IsTrackingRunning()
		{
			return _isTrackingRunning;
		}

		/// <summary>
		/// Removes a trackable with a given name.
		/// </summary>
		/// <param name="name">Name.</param>
		public void RemoveTrackable(string name)
		{
			// TODO: remove from plugin
		}

		/// <summary>
		/// Clears trackables.
		/// </summary>
		public void ClearTrackables()
		{
			// TODO:
		}

		/// <summary>
		/// Initialisess the plugin.
		/// </summary>
		/// <returns><c>true</c>, if plugin was inited, <c>false</c> otherwise.</returns>
		public abstract bool InitPlugin();

		/// <summary>
		/// Deinitialises the plugin.
		/// </summary>
		public abstract void DeinitPlugin();

		/// <summary>
		/// Gets the native plugin version.
		/// </summary>
		/// <returns>The native plugin version.</returns>
		public abstract float GetNativePluginVersion();

		/// <summary>
		/// Raises the application focus event.
		/// </summary>
		/// <param name="focusStatus">If set to <c>true</c> focus status.</param>
		public abstract void OnApplicationFocus(bool focusStatus);

		/// <summary>
		/// Raises the application pause event.
		/// </summary>
		/// <param name="pauseStatus">If set to <c>true</c> application is paused.</param>
		public abstract void OnApplicationPause(bool pauseStatus);

		/// <summary>
		/// Gets the number of cameras.
		/// </summary>
		/// <returns>The number of cameras.</returns>
		public abstract int GetNumCameras();

		/// <summary>
		/// Starts input from an image.
		/// </summary>
		/// <returns><c>true</c>, if input from image was started, <c>false</c> otherwise.</returns>
		/// <param name="image">Image.</param>
		public abstract bool StartInputFromImage(Texture2D image);

		/// <summary>
		/// Starts input from a given camera device.
		/// </summary>
		/// <returns><c>true</c>, if input from camera was started, <c>false</c> otherwise.</returns>
		/// <param name="deviceIndex">Device index.</param>
		/// <param name="targetWidth">Target width.</param>
		/// <param name="targetHeight">Target height.</param>
		public abstract bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight);

		/// <summary>
		/// Stops all input.
		/// </summary>
		public abstract void StopInput();

		/// <summary>
		/// Adds a trackable with a given set of data and ID.
		/// </summary>
		/// <returns><c>true</c>, if trackable was added, <c>false</c> otherwise.</returns>
		/// <param name="data">Data.</param>
		/// <param name="id">Identifier.</param>
		public abstract bool AddTrackable(byte[] data, string id);

		/// <summary>
		/// Updates tracking.
		/// </summary>
		public abstract void UpdateTracking();

		/// <summary>
		/// Function called just after the current frame has been drawn.
		/// </summary>
		public abstract void PostRender();
		
		/// <summary>
		/// Starts tracking.
		/// </summary>
		public abstract void StartTracking();

		/// <summary>
		/// Stops tracking.
		/// </summary>
		public abstract void StopTracking();

		/// <summary>
		/// Enables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was enabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		public abstract bool EnableTrackingMethod(int trackingMethodId);

		/// <summary>
		/// Disables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was disabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		public abstract bool DisableTrackingMethod(int trackingMethodId);

		/// <summary>
		/// Gets the marker recovery status.
		/// </summary>
		/// <returns><c>true</c>, if marker recovery is enabled, <c>false</c> otherwise.</returns>
		public abstract bool GetMarkerRecoveryStatus ();

		/// <summary>
		/// Sets the marker recovery status.
		/// Enabling this feature allows for quicker re-detection if a marker is lost as well as making it easier to re-detect the marker from shallower angles and greater distances.
		/// This is a feature that we recommend everyone should generally enable.
		/// N.B. Enabling this feature will use a fraction more CPU power.
		/// </summary>
		/// <param name="status">Marker recovery is enabled if set to <c>true</c>, otherwise flow recovery is disabled. Default is false.</param>
		public abstract void SetMarkerRecoveryStatus (bool status);

		/// <summary>
		/// Sets the API key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="bundleId">Bundle identifier.</param>
		public abstract void SetApiKey(string key, string bundleId);

		/// <summary>
		/// Starts arbitrary tracking using a given position and orientation.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public abstract void ArbiTrackStart (Vector3 position, Quaternion orientation);

		/// <summary>
		/// Checks if arbitrary tracking is currently running.
		/// </summary>
		/// <returns><c>true</c>, if arbitrary tracking is running, <c>false</c> otherwise.</returns>
		public abstract bool ArbiTrackIsTracking ();

		/// <summary>
		/// Gets the current position and orientation of the floor, relative to the device.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public abstract void FloorPlaceGetPose (out Vector3 position, out Quaternion orientation);

		/// <summary>
		/// Gets the current position and rotation of the markerless driver being tracked.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
		public abstract void ArbiTrackGetPose (out Vector3 position, out Quaternion orientation);

		/// <summary>
		/// Sets up the rendering camera.
		/// </summary>
		/// <param name="cameraNearPlane">Camera near plane.</param>
		/// <param name="cameraFarPlane">Camera far plane.</param>
		public void SetupRenderingCamera(float cameraNearPlane, float cameraFarPlane)
		{
			_cameraNearPlane = cameraNearPlane;
			_cameraFarPlane = cameraFarPlane;
		}

		/// <summary>
		/// Adds the trackable from a given path with a given ID.
		/// </summary>
		/// <returns><c>true</c>, if trackable was added, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="id">Identifier.</param>
		public bool AddTrackable(string path, string id)
		{
			bool result = false;
			if (System.IO.File.Exists(path))
			{
				byte[] data = System.IO.File.ReadAllBytes(path);
				result = AddTrackable(data, id);
			}
			else
			{
				Debug.LogError("[KudanAR] Missing file " + path);
			}

			return result;
		}

		/// <summary>
		/// Gets the tracking texture.
		/// </summary>
		/// <returns>The tracking texture.</returns>
		public Texture GetTrackingTexture()
		{
			if (_clonedTexture != null) {
				return _clonedTexture;
			}
			return _finalTexture;
		}

		/// <summary>
		/// Gets the number of detected trackables.
		/// </summary>
		/// <returns>The number of detected trackables.</returns>
		public int GetNumDetectedTrackables()
		{
			return _detected.Count;
		}

		/// <summary>
		/// Gets detected trackable at the given index.
		/// </summary>
		/// <returns>The detected trackable.</returns>
		/// <param name="index">Index.</param>
		public Trackable GetDetectedTrackable(int index)
		{
			return _detected[index];
		}

		/// <summary>
		/// Gets the projection matrix.
		/// </summary>
		/// <returns>The projection matrix.</returns>
		public Matrix4x4 GetProjectionMatrix()
		{
			return _projectionMatrix;
		}

		/// <summary>
		/// Gets the detected trackables as array.
		/// </summary>
		/// <returns>The detected trackables as array.</returns>
		public Trackable[] GetDetectedTrackablesAsArray()
		{
			return _detected.ToArray();
		}

		public void SetArbiTrackFloorHeight(float floorHeight)
		{
			_floorHeight = floorHeight;
		}

		public virtual void  updateCam()
		{
			
		}
	}
};
