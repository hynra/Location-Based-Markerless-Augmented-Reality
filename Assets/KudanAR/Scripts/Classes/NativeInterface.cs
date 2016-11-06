using UnityEngine;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
#if UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX
	public class NativeInterface
	{
    #if UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_EDITOR_OSX
        private const string PLUGIN_FILE = "KudanPlugin";
	#elif UNITY_IOS
		private const string PLUGIN_FILE = "__Internal";
	#endif

	#if !NULL_PLUGIN
		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Initialises the plugin.
		/// </summary>
		public static extern bool Init();
		
		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Deinitialises the plugin.
		/// </summary>
		public static extern void Deinit();
		
		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the current version of the plugin.
		/// </summary>
		/// <returns>The plugin version.</returns>
		public static extern float GetPluginVersion();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Adds a set of Trackable Data.
		/// </summary>
		/// <returns><c>true</c>, if trackable set was added, <c>false</c> otherwise.</returns>
		/// <param name="dataPointer">Data pointer.</param>
		/// <param name="dataLength">Data length.</param>
		public static extern bool AddTrackableSet(System.IntPtr dataPointer, int dataLength);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Processes the current frame.
		/// </summary>
		/// <param name="dataPointer">Data pointer.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="padding">Padding.</param>
		public static extern void ProcessFrame(System.IntPtr dataPointer, int width, int height, int padding);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the number of loaded trackables.
		/// </summary>
		/// <returns>The number of loaded trackables as an int.</returns>
		public static extern int GetNumberOfTrackables();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the number of trackables currently being detected.
		/// </summary>
		/// <returns>The number of detected trackables.</returns>
		public static extern int GetNumberOfDetectedTrackables();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the projection matrix.
		/// </summary>
		/// <returns><c>true</c>, if projection matrix was gotten, <c>false</c> otherwise.</returns>
		/// <param name="nearPlane">Near plane.</param>
		/// <param name="farPlane">Far plane.</param>
		/// <param name="result">Result.</param>
		public static extern bool GetProjectionMatrix(float nearPlane, float farPlane, float[] result);

        #if UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_EDITOR_OSX
        [DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets a detected trackable with the specified parameters.
		/// </summary>
		/// <returns><c>true</c>, if detected trackable was gotten, <c>false</c> otherwise.</returns>
		/// <param name="index">Index.</param>
		/// <param name="result">Result.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="trackingMethod">Tracking method.</param>
		/// <param name="name">Name.</param>
		public static extern bool GetDetectedTrackable(int index, float[] result, ref int width, ref int height, ref int trackingMethod, StringBuilder name);
		#endif
		
		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Enables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was enabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		public static extern bool EnableTrackingMethod(int trackingMethodId);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Disables the given tracking method.
		/// </summary>
		/// <returns><c>true</c>, if tracking method was disabled, <c>false</c> otherwise.</returns>
		/// <param name="trackingMethodId">Tracking method identifier.</param>
		public static extern bool DisableTrackingMethod(int trackingMethodId);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the marker recovery status.
		/// </summary>
		/// <returns><c>true</c>, if marker recovery is enabled, <c>false</c> otherwise.</returns>
		public static extern bool GetMarkerRecoveryStatus();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Sets the marker recovery status.
		/// Enabling this feature allows for quicker re-detection if a marker is lost as well as making it easier to re-detect the marker from shallower angles and greater distances.
		/// This is a feature that we recommend everyone should generally enable.
		/// N.B. Enabling this feature will use a fraction more CPU power.
		/// </summary>
		/// <param name="status">Marker recovery is enabled if set to <c>true</c>, otherwise flow recovery is disabled. Default is false.</param>
		public static extern void SetMarkerRecoveryStatus(bool status);

        [DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the current position and rotation of the markerless driver being tracked.
		/// </summary>
		/// <param name="result">Result.</param>
        public static extern void ArbiTrackGetPose(float[] result);

        [DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Starts arbitrary tracking.
		/// </summary>
		/// <param name="pose">Pose.</param>
        public static extern void ArbiTrackStart(float[] pose);

        [DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Checks if arbitrary tracking is running.
		/// </summary>
		/// <returns><c>true</c>, if arbitrary tracking is running, <c>false</c> otherwise.</returns>
        public static extern bool ArbiTrackIsTracking();

        [DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the current position and orientation of the floor relative to the device.
		/// </summary>
		/// <param name="pose">Pose.</param>
		/// <param name="depth">Depth.</param>
        public static extern void FloorPlaceGetPose(float[] pose, float depth);

		#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Sets the texture ID.
		/// </summary>
		/// <param name="textureID">Texture ID.</param>
		public static extern void setTextureID(long textureID);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Sets the texture.
		/// </summary>
		/// <param name="color">Color.</param>
		public static extern void setTexture(Color32 []color);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the render event function.
		/// </summary>
		/// <returns>The render event funcion.</returns>
		public static extern System.IntPtr GetRenderEventFunc();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Initialises the webcam.
		/// </summary>
		/// <returns><c>true</c>, if camera was initialised, <c>false</c> otherwise.</returns>
		/// <param name="webcamID">Webcam I.</param>
		public static extern bool WebCamInit(int webcamID);

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Deninitialises the webcam.
		/// </summary>
		public static extern void WebCamDeinit();

		[DllImport(PLUGIN_FILE)]
		/// <summary>
		/// Gets the webcam resolution.
		/// </summary>
		/// <param name="resolution">Resolution.</param>
		public static extern void WebCamGetResolution(int[] resolution);
	
		[DllImport(PLUGIN_FILE, CharSet=CharSet.Ansi)]
		/// <summary>
		/// Checks the API key is valid.
		/// </summary>
		/// <returns><c>true</c>, if API key is valid, <c>false</c> otherwise.</returns>
		/// <param name="apiKey">API key.</param>
		/// <param name="bundleID">Bundle ID.</param>
		public static extern bool CheckAPIKeyIsValid(string apiKey, string bundleID);

		[DllImport(PLUGIN_FILE, CharSet=CharSet.Ansi)]
		/// <summary>
		/// Sets the unity editor API key.
		/// </summary>
		/// <returns><c>true</c>, if unity editor API key was set, <c>false</c> otherwise.</returns>
		/// <param name="apiKey">API key.</param>
		public static extern bool SetUnityEditorApiKey(string apiKey);
		#endif

	#elif NULL_PLUGIN
		
		public static bool Init()
		{
			return true; 
		}
		
		public static void Deinit()
		{ 
		}
		
		public static float GetPluginVersion()
		{
			return 0.0f;
		}

		public static bool AddTrackableSet(System.IntPtr dataPointer, int dataLength)
		{
			return true;
		}

		public static void ProcessFrame(System.IntPtr dataPointer, int width, int height, int padding)
		{
		}

		public static int GetNumberOfTrackables()
		{
			return 0;
		}

		public static int GetNumberOfDetectedTrackables()
		{
			return 0;
		}

		public static bool GetProjectionMatrix(float nearPlane, float farPlane, float[] result)
		{
			return false;
		}

		public static bool GetDetectedTrackable(float[] result, StringBuilder name, ref int width, ref int height)
		{
		}
	#endif
	}
#endif
};