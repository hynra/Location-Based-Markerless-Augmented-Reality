using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	/// <summary>
	/// The tracker class that TrackerBase inherits from
	/// </summary>
	public interface ITracker
	{
		/// <summary>
		/// Initialisess the plugin.
		/// </summary>
		/// <returns><c>true</c>, if plugin was initialised, <c>false</c> otherwise.</returns>
		bool InitPlugin();

		/// <summary>
		/// Deinitialises the plugin.
		/// </summary>
		void DeinitPlugin();

		/// <summary>
		/// Gets the current plugin version.
		/// </summary>
		/// <returns>The plugin version.</returns>
		string GetPluginVersion();

		/// <summary>
		/// Gets the native plugin version.
		/// </summary>
		/// <returns>The native plugin version.</returns>
		float GetNativePluginVersion();

		/// <summary>
		/// Gets the number of cameras.
		/// </summary>
		/// <returns>The number of cameras.</returns>
		int GetNumCameras();

		/// <summary>
		/// Sets up the rendering camera.
		/// </summary>
		/// <param name="cameraNearPlane">Camera near plane.</param>
		/// <param name="cameraFarPlane">Camera far plane.</param>
		void SetupRenderingCamera(float cameraNearPlane, float cameraFarPlane);

		/// <summary>
		/// Starts the input from a given image.
		/// </summary>
		/// <returns><c>true</c>, if input from image was started, <c>false</c> otherwise.</returns>
		/// <param name="image">Image.</param>
		bool StartInputFromImage(Texture2D image);

		/// <summary>
		/// Starts the input from a  given camera device.
		/// </summary>
		/// <returns><c>true</c>, if input from camera was started, <c>false</c> otherwise.</returns>
		/// <param name="deviceIndex">Device index.</param>
		/// <param name="targetWidth">Target width.</param>
		/// <param name="targetHeight">Target height.</param>
		bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight);

		/// <summary>
		/// Stops all input.
		/// </summary>
		void StopInput();

		/// <summary>
		/// Adds a trackable at a given path with a given ID.
		/// </summary>
		/// <returns><c>true</c>, if trackable was added, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="id">Identifier.</param>
		bool AddTrackable(string path, string id);

		/// <summary>
		/// Adds a trackable with a given data set and ID.
		/// </summary>
		/// <returns><c>true</c>, if trackable was added, <c>false</c> otherwise.</returns>
		/// <param name="data">Data.</param>
		/// <param name="id">Identifier.</param>
		bool AddTrackable(byte[] data, string id);

		/// <summary>
		/// Gets the number of trackables.
		/// </summary>
		/// <returns>The number of trackables.</returns>
		int GetNumTrackables();

		/// <summary>
		/// Gets a trackable at given index.
		/// </summary>
		/// <returns>A trackable.</returns>
		/// <param name="index">Index.</param>
		Trackable GetTrackable(int index);

		/// <summary>
		/// Removes a trackable with a given name.
		/// </summary>
		/// <param name="name">Name.</param>
		void RemoveTrackable(string name);

		/// <summary>
		/// Clears trackables.
		/// </summary>
		void ClearTrackables();

		/// <summary>
		/// Updates tracking.
		/// </summary>
		void UpdateTracking();
		
		/// <summary>
		/// Starts tracking.
		/// </summary>
		void StartTracking();

		/// <summary>
		/// Determines whether tracking is running.
		/// </summary>
		/// <returns><c>true</c> if tracking is running; otherwise, <c>false</c>.</returns>
		bool IsTrackingRunning();

		/// <summary>
		/// Stops tracking.
		/// </summary>
		void StopTracking();

		/// <summary>
		/// Enables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was enabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		bool EnableTrackingMethod(int trackingMethodId);

		/// <summary>
		/// Disables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was disabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		bool DisableTrackingMethod(int trackingMethodId);
		
		/// <summary>
		/// Gets the tracking texture.
		/// </summary>
		/// <returns>The tracking texture.</returns>
		Texture GetTrackingTexture();
		
		/// <summary>
		/// Gets the number of detected trackables.
		/// </summary>
		/// <returns>The number of detected trackables.</returns>
		int GetNumDetectedTrackables();

		/// <summary>
		/// Gets detected trackable at the given index.
		/// </summary>
		/// <returns>The detected trackable.</returns>
		/// <param name="index">Index.</param>
		Trackable GetDetectedTrackable(int index);

		/// <summary>
		/// Gets the projection matrix.
		/// </summary>
		/// <returns>The projection matrix.</returns>
		Matrix4x4 GetProjectionMatrix();

		/// <summary>
		/// Updates the camera.
		/// </summary>
		void updateCam ();


		bool GetMarkerRecoveryStatus();

		void SetMarkerRecoveryStatus(bool status);
	}
};