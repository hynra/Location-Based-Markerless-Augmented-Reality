using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kudan.AR
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kudan AR/Kudan Tracker")]
	public class KudanTracker : MonoBehaviour
	{
        static KudanTracker kudanTracker;

        /// <summary>
        /// The default width of the camera.
        /// </summary>
        private const int DefaultCameraWidth = 640;
		
		/// <summary>
		/// The default height of the camera.
		/// </summary>
		private const int DefaultCameraHeight = 480;

		[Tooltip("The Editor API Key Issued by Kudan")]
		/// <summary>
		/// The editor API key.
		/// </summary>
		public string _EditorAPIKey = string.Empty;

		/// <summary>
		/// Reference to the tracker plugin.
		/// </summary>
		protected TrackerBase _trackerPlugin;

		/// <summary>
		/// Array of detected trackables.
		/// </summary>
		protected Trackable[] _lastDetectedTrackables;
		
		[Tooltip("The API License key issued by Kudan")]
		/// <summary>
		/// The API License key.
		/// </summary>
		public string _APIKey = string.Empty;

		/// <summary>
		/// The default tracking method used by the tracker on startup.
		/// </summary>
		public TrackingMethodBase _defaultTrackingMethod;

		/// <summary>
		/// Array of all tracking methods that can be used by the tracker.
		/// </summary>
		public TrackingMethodBase[] _trackingMethods;

		/// <summary>
		/// Sets the Marker Recovery Mode.
		/// Enabling this will help with recovering a lost marker, however this comes at the cost of slightly more CPU usage.
		/// </summary>
		public bool _markerRecoveryMode;

		[Tooltip("Don't destroy between level loads")]
		/// <summary>
		/// Whether or not to make this tracker persist between scenes.
		/// </summary>
		public bool _makePersistent = true;

		/// <summary>
		/// Whether or not to start initialise this tracker when it is loaded.
		/// </summary>
		public bool _startOnEnable = true;

		/// <summary>
		/// Whether or not to apply the projection matrix.
		/// </summary>
		public bool _applyProjection = true;

		[Tooltip("The camera to apply the projection matrix to. If left blank this will use the main camera.")]
		/// <summary>
		/// The camera to apply the projection matrix to.
		/// </summary>
		public Camera _renderingCamera;

		[Tooltip("The renderer to draw the tracking texture to.")]
		/// <summary>
		/// The renderer to draw the tracking texture to.
		/// </summary>
		public Renderer _background;

		/// <summary>
		/// Whether or not to display the debug GUI.
		/// </summary>
		public bool _displayDebugGUI = true;

		[Range(1, 4)]
		/// <summary>
		/// The size of the debug GUI.
		/// </summary>
		public int _debugGUIScale = 1;

		[HideInInspector]
		/// <summary>
		/// The debug shader.
		/// </summary>
		public Shader _debugFlatShader;

		/// <summary>
		/// ArbiTracker floor height
		/// </summary>
		protected float _floorHeight = 200.0f;


		/// <summary>
		/// Gets the interface exposing the Kudan API for those that need scripting control.
		/// </summary>
		public TrackerBase Interface
		{
			get { return _trackerPlugin; }
		}

		/// <summary>
		/// The current tracking method.
		/// </summary>
		private TrackingMethodBase _currentTrackingMethod;

		/// <summary>
		/// Gets the current tracking method.
		/// </summary>
		public TrackingMethodBase CurrentTrackingMethod
		{
			get { return _currentTrackingMethod; }
		}

#if UNITY_EDITOR
		/// <summary>
		/// The index of the toolbar.
		/// </summary>
		private int _toolbarIndex;

		/// <summary>
		/// If you have more than one webcam you can change your prefered webcam ID here.
		/// ID is of the webcam used in Play Mode.
		/// </summary>
		[Range(0, 10)]
		[Tooltip ("If you have more than one webcam you can change your prefered webcam ID here.")]
		public int _playModeWebcamID;



		/// <summary>
		/// Checks the validity of the license key.
		/// </summary>
		private void checkLicenseKeyValidity() 
		{
			bool result = NativeInterface.CheckAPIKeyIsValid(_APIKey.Trim(), PlayerSettings.bundleIdentifier);

			if (result)
            {
				Debug.Log ("[KudanAR] Your Bundle License Key Is Valid");
			}
            else
            {
				Debug.LogError ("[KudanAR] License Key is INVALID for Bundle: "+ PlayerSettings.bundleIdentifier);
			}
		}

		/// <summary>
		/// Checks the editor license key.
		/// </summary>
		private void checkEditorLicenseKey() 
		{
			bool result = NativeInterface.SetUnityEditorApiKey (_EditorAPIKey.Trim());
			if (result)
			{
				Debug.Log ("[KudanAR] Editor Play Mode Key is Valid");
			}
			else
			{
				Debug.LogError ("[KudanAR] Editor Play Mode Key is NOT Valid");
			}
		}
#endif

		/// <summary>
		/// Gets the appropriate tracker Plugin for the platform being used
		/// </summary>
		void GetPlugin ()
		{
			#if UNITY_EDITOR_OSX
				_trackerPlugin = new TrackerOSX();
				checkEditorLicenseKey();
				checkLicenseKeyValidity();
			#elif UNITY_EDITOR_WIN
				_trackerPlugin = new TrackerWindows();	
				checkEditorLicenseKey();
				checkLicenseKeyValidity();
			#elif UNITY_IOS
				_trackerPlugin = new TrackeriOS(_background);
			#elif UNITY_ANDROID
				_trackerPlugin = new TrackerAndroid(_background);
			#endif 
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start()
		{
			// Check there is only a single instance of this component
			if (FindObjectsOfType<KudanTracker>().Length > 1)
			{
				Debug.LogError("[KudanAR] There should only be one instance of KudanTracker active at a time");
				return;
			}

			CreateDebugLineMaterial();

			// Create the platform specific plugin interface
			GetPlugin();

			if (_trackerPlugin == null)
			{
				Debug.LogError("[KudanAR] Failed to initialise");
				this.enabled = false;
				return;
			}
				
			// Initialise plugin
			if (!_trackerPlugin.InitPlugin())
			{
				Debug.LogError("[KudanAR] Error initialising plugin");
				this.enabled = false;
			}
			else
			{
				// Set the API key
				if (!string.IsNullOrEmpty(_APIKey))
				{
					_trackerPlugin.SetApiKey (_APIKey, Application.bundleIdentifier);
				}
				
				// Print plugin version
				string version = _trackerPlugin.GetPluginVersion();
				float nativeVersion = _trackerPlugin.GetNativePluginVersion();
				Debug.Log(string.Format("[KudanAR] Initialising v{0} (native v{1})", version, nativeVersion));

				// Don't destroy this component between level loads
				if (_makePersistent)
				{
					GameObject.DontDestroyOnLoad(this.gameObject);
				}

				foreach (TrackingMethodBase method in _trackingMethods)
				{
					method.Init();
				}
				_trackerPlugin.SetMarkerRecoveryStatus(_markerRecoveryMode);

				ChangeTrackingMethod(_defaultTrackingMethod);

				// Start the camera
				#if UNITY_EDITOR
				if (_trackerPlugin.StartInputFromCamera(_playModeWebcamID, DefaultCameraWidth, DefaultCameraHeight)) 
				#else
				if (_trackerPlugin.StartInputFromCamera(0, DefaultCameraWidth, DefaultCameraHeight)) 
				#endif
				{
					// Start tracking
					if (_startOnEnable)
					{
						_trackerPlugin.StartTracking();
					}
				}
				else
				{
					Debug.LogError("[KudanAR] Failed to start camera, is it already in use?");
				}
			}
		}


        void Awake()
        {
            // If there is no KudanTracker currently in the scene when it loads, make sure that this persists between scenes, then set the static reference of KudanTracker to this object.
            if (kudanTracker == null)
            {
                if (_makePersistent)
                {
                    DontDestroyOnLoad(gameObject);
                    kudanTracker = this;
                }
            }
            // If KudanTracker already exists in the scene, but this is not it, destroy this gameobject, because there should only be one KudanTracker in a scene at any one time.
            else if (kudanTracker != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Raises the enable event.
        /// </summary>
        void OnEnable()
		{
			if (_startOnEnable)
			{
				StartTracking();
			}
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable()
		{
			StopTracking();
		}

		/// <summary>
		/// Raises the application focus event.
		/// </summary>
		/// <param name="focusStatus">If set to <c>true</c> focus status.</param>
		void OnApplicationFocus(bool focusStatus)
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.OnApplicationFocus(focusStatus);
			}
		}

		/// <summary>
		/// Raises the application pause event.
		/// </summary>
		/// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
		void OnApplicationPause(bool pauseStatus)
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.OnApplicationPause(pauseStatus);
			}
		}

		/// <summary>
		/// Starts the tracking.
		/// </summary>
		public void StartTracking()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.StartTracking();
			}
		}
		
		/// <summary>
		/// Stops the tracking.
		/// </summary>
		public void StopTracking()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.StopTracking();
			}
			
			// Restore projection matrix
			Camera camera = _renderingCamera;
			if (camera == null) 
			{
				camera = Camera.main;
			}
			if (camera != null)
			{
				camera.ResetProjectionMatrix();
			}
		}
		
		/// <summary>
		/// Changes the tracking method.
		/// </summary>
		/// <param name="newTrackingMethod">New tracking method.</param>
		public void ChangeTrackingMethod(TrackingMethodBase newTrackingMethod)
		{
			if (newTrackingMethod != null && _currentTrackingMethod != newTrackingMethod)
			{
				if (_currentTrackingMethod != null)
				{
					_currentTrackingMethod.StopTracking();
				}

				_currentTrackingMethod = newTrackingMethod;
				_currentTrackingMethod.StartTracking();
			}
		}

		/// <summary>
		/// Starts Arbitrary Tracking.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
        public void ArbiTrackStart(Vector3 position, Quaternion orientation)
        {
            _trackerPlugin.ArbiTrackStart(position, orientation);
        }

		/// <summary>
		/// Checks if Arbitrary Tracking is running.
		/// </summary>
		/// <returns><c>true</c>, if Arbitrary Tracking is running, <c>false</c> otherwise.</returns>
        public bool ArbiTrackIsTracking()
        {
            return _trackerPlugin.ArbiTrackIsTracking();
        }

		/// <summary>
		/// Gets the current position and orientation of the floor, relative to the device.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
        public void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
        {
            _trackerPlugin.FloorPlaceGetPose(out position, out orientation);
        }

		/// <summary>
		/// Gets the current position and orientation of the markerless driver being tracked.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="orientation">Orientation.</param>
        public void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
        {
            _trackerPlugin.ArbiTrackGetPose(out position, out orientation);
        }

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy()
		{
			if (_trackerPlugin != null)
			{
				StopTracking();
				_trackerPlugin.StopInput();
				_trackerPlugin.DeinitPlugin();
				_trackerPlugin = null;
			}

			if (_lineMaterial != null)
			{
				Material.Destroy(_lineMaterial);
				_lineMaterial = null;
			}
		}

		/// <summary>
		/// Raises the pre render event.
		/// </summary>
		void OnPreRender()
		{
			_trackerPlugin.updateCam ();
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update()
		{
			if (_trackerPlugin != null)
			{
				Camera renderingCamera = _renderingCamera;

                if (renderingCamera == null)
                {
                    renderingCamera = Camera.main;
                }

				_trackerPlugin.SetupRenderingCamera(renderingCamera.nearClipPlane, renderingCamera.farClipPlane);
				
				// Update tracking
				_trackerPlugin.UpdateTracking();

                // Apply projection matrix
                if (_applyProjection)
                {
                    renderingCamera.projectionMatrix = _trackerPlugin.GetProjectionMatrix();
                }
                else
                {
                    renderingCamera.ResetProjectionMatrix();
                }

				// Take a copy of the detected trackables
				ProcessNewTrackables();

				_currentTrackingMethod.ProcessFrame();

				// Apply texture to background renderer
				Texture texture = _trackerPlugin.GetTrackingTexture();

				if (_background != null && texture != null)
				{
					_background.material.mainTexture = texture;
				}
			}
		}

#if UNITY_ANDROID
		/// <summary>
		/// Raises the post render event.
		/// </summary>
		void OnPostRender()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.PostRender();
			}

			if (_displayDebugGUI)
			{
				RenderAxes();
			}
		}
#else
		/// <summary>
		/// Raises the post render event.
		/// </summary>
		void OnPostRender()
		{
			if (_displayDebugGUI)
			{
				RenderAxes();
			}
		}
#endif
		/// <summary>
		/// Processes new trackables.
		/// </summary>
		private void ProcessNewTrackables()
		{
			_lastDetectedTrackables = _trackerPlugin.GetDetectedTrackablesAsArray();
		}

		/// <summary>
		/// Determines whether this instance has active tracking data.
		/// </summary>
		/// <returns><c>true</c> if this instance has active tracking data; otherwise, <c>false</c>.</returns>
		public bool HasActiveTrackingData()
		{
			return (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning() && _lastDetectedTrackables != null && _lastDetectedTrackables.Length > 0);
		}


		public void SetArbiTrackFloorHeight(float floorHeight)
		{
			_trackerPlugin.SetArbiTrackFloorHeight (floorHeight);
		}

		/// <summary>
		/// Raises the draw gizmos event.
		/// </summary>
		void OnDrawGizmos()
		{
			// Draw useful debug rendering in Editor
			if (HasActiveTrackingData())
			{
				foreach (Trackable t in _lastDetectedTrackables)
				{
					// Draw circle
					Gizmos.color = Color.cyan;
					Gizmos.DrawSphere(t.position, 10f);

					// Draw line from origin to point (useful if object is offscreen)
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(Vector3.zero, t.position);
					
					// Draw axes
					Matrix4x4 xform = Matrix4x4.TRS(t.position, t.orientation, Vector3.one * 250f);
					Gizmos.matrix = xform;

					Gizmos.color = Color.red;
					Gizmos.DrawLine(Vector3.zero, Vector3.right);

					Gizmos.color = Color.green;
					Gizmos.DrawLine(Vector3.zero, Vector3.up);

					Gizmos.color = Color.blue;
					Gizmos.DrawLine(Vector3.zero, Vector3.forward);
				}
			}
		}

		/// <summary>
		/// Starts line rendering.
		/// </summary>
		/// <returns><c>true</c>, if line rendering was started, <c>false</c> otherwise.</returns>
		public bool StartLineRendering()
		{
			bool result = false;
			if (_lineMaterial != null)
			{
				_lineMaterial.SetPass(0);
				result = true;
			}
			return result;
		}

		/// <summary>
		/// Renders axes for debugging.
		/// </summary>
		private void RenderAxes()
		{
			if (HasActiveTrackingData() && StartLineRendering())
			{			
				foreach (Trackable t in _lastDetectedTrackables)
				{
					Matrix4x4 xform = Matrix4x4.TRS(t.position, t.orientation, Vector3.one * 250f);

					GL.PushMatrix();

					Matrix4x4 m = GL.GetGPUProjectionMatrix(_trackerPlugin.GetProjectionMatrix(), false);
					m = _trackerPlugin.GetProjectionMatrix();
					GL.LoadProjectionMatrix(m);

					// Draw line from origin to point (useful if object is offscreen)
					GL.Color(Color.cyan);
					GL.Vertex(Vector3.zero);
					GL.Vertex(t.position);		

					GL.Begin(GL.LINES);
					GL.MultMatrix(xform);
					GL.Color(Color.red);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.right);

					GL.Color(Color.green);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.up);

					GL.Color(Color.blue);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.forward);

					GL.End();
					GL.PopMatrix();
				}
			}
		}
		
		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		void OnGUI()
		{
			// Display debug GUI with tracking information
			if (_displayDebugGUI)
			{
				GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_debugGUIScale, _debugGUIScale, 1f));
				GUILayout.BeginVertical("box");
#if UNITY_EDITOR
				GUILayout.Label("KUDAN AR", UnityEditor.EditorStyles.boldLabel);
#else
				GUILayout.Label("KUDAN AR");
#endif
				// Tracking status
				if (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning())
				{
					GUI.color = Color.green;
					GUILayout.Label("Tracker is running");
				}
				else
				{
					GUI.color = Color.red;
					GUILayout.Label("Tracker NOT running");
				}
				GUI.color = Color.white;

				// Screen resolution
				GUILayout.Label("Screen: " + Screen.width + "x" + Screen.height);

				// Frame rates
				if (_trackerPlugin != null)
				{
					GUILayout.Label("Camera rate:  " + _trackerPlugin.CameraFrameRate.ToString("F2") + "hz");
					GUILayout.Label("Tracker rate: " + _trackerPlugin.TrackerFrameRate.ToString("F2") + "hz");
					GUILayout.Label("App rate: " + _trackerPlugin.AppFrameRate.ToString("F2") + "hz");
				}

				if (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning())
				{
					// Texture image and resolution
					if (_currentTrackingMethod != null)
					{
						GUILayout.Label("Method: " + _currentTrackingMethod.Name);
						_currentTrackingMethod.DebugGUI(_debugGUIScale);
					}
				}
			}
		}

		/// <summary>
		/// The line material.
		/// </summary>
		private Material _lineMaterial;

		/// <summary>
		/// Creates the debug line material.
		/// </summary>
		private void CreateDebugLineMaterial()
		{
			if (!_lineMaterial && _debugFlatShader != null)
			{
				_lineMaterial = new Material(_debugFlatShader);
				_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
};
