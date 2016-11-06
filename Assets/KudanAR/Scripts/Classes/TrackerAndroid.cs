#if UNITY_ANDROID

using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public class TrackerAndroid : TrackerBase
	{
        private AndroidJavaObject	m_KudanAR_Instance	= null;
        private AndroidJavaObject	m_ActivityContext	= null;

		private int					m_DeviceIndex		= -1;
		private int					m_TextureHandle		= 0;
		private int					m_Width				= 0;
		private int					m_Height			= 0;

		private Texture2D			m_InputTexture		= null;

		private bool				m_WasTrackingWhenApplicationPaused = false;
		
		private int					_numFramesRendered		= 0;
		private float				_rateTimer				= 0.0f;

		private MeshFilter 			_cameraBackgroundMeshFilter;

		private ScreenOrientation 	_prevScreenOrientation;
		private Matrix4x4 			_projectionRotation = Matrix4x4.identity;

		public TrackerAndroid(Renderer background)
		{
			_cameraBackgroundMeshFilter = background.GetComponent<MeshFilter> ();
		}

		public override bool InitPlugin()
		{
//			AndroidJNIHelper.debug = true;

			Debug.LogError( "[KudanAR] Graphics vendor: " + SystemInfo.graphicsDeviceVendor );
			Debug.LogError( "[KudanAR] Graphics version: " + SystemInfo.graphicsDeviceVersion );
			
			bool bInited = false;

            if (m_ActivityContext == null)
            {
                AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                if (activityClass != null)
                {
                    m_ActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                    AndroidJavaClass kudanArClass = new AndroidJavaClass("eu.kudan.androidar.KudanAR");
                    if (kudanArClass != null )
                    {
                        m_KudanAR_Instance = kudanArClass.CallStatic<AndroidJavaObject>( "getInstance" );

						if ( m_KudanAR_Instance != null )
						{
							// Initialise java bits (camera, openGL render to target bits, etc)
							// Does the initialisation of the Kudan library as well
							bInited = m_KudanAR_Instance.Call<bool>( "Initialise", m_ActivityContext );
						}
					}
                }
            }

			return bInited;
		}

		public override void DeinitPlugin()
		{
			if ( m_KudanAR_Instance != null )
			{
				// De-initialise everything on the java side
				// Also calls Deinit on the Kudan library
				m_KudanAR_Instance.Call( "Deinitialise" );
			}
		}

		public override void SetApiKey(string key, string bundleId)
		{
			if ( m_KudanAR_Instance != null )
			{
                byte[] aKey = Encoding.UTF8.GetBytes( key );
                m_KudanAR_Instance.Call( "SetApiKey", aKey );
			}
		}

		public override float GetNativePluginVersion()
		{
			float fVersion = 0.0f;

			if ( m_KudanAR_Instance != null )
			{
				// Version
				fVersion = m_KudanAR_Instance.Call<float>( "GetPluginVersion" );
			}
			
			return fVersion;
		}

		public override void OnApplicationFocus( bool focusStatus )
		{
//			Debug.LogError( "[KudanAR] OnApplicationFocus called - focusStatus: " + focusStatus );

			if ( focusStatus )
			{
				if ( m_DeviceIndex > -1 && m_Width > 0 && m_Height > 0 )
				{
					StartInputFromCamera( m_DeviceIndex, m_Width, m_Height );
				}
				
				if ( m_WasTrackingWhenApplicationPaused )
				{
					StartTracking();

					m_WasTrackingWhenApplicationPaused = false;
				}
			}
		}

		public override void OnApplicationPause( bool pauseStatus )
		{
//			Debug.LogError( "[KudanAR] OnApplicationPause called - pauseStatus: " + pauseStatus );

			if (pauseStatus) {
				// First stop existing input
				m_WasTrackingWhenApplicationPaused = _isTrackingRunning;
				StopInput ();
			} else {

				OnApplicationFocus (true);
			}
		}
		
		public override int GetNumCameras()
		{
			int iNumCameras = 0;

			if ( m_KudanAR_Instance != null )
			{
				// De-initialise the Kudan library
				iNumCameras = m_KudanAR_Instance.Call<int>( "GetNumberOfBackFacingCameras" );
			}
			
			return iNumCameras;
		}

		public override bool StartInputFromImage(Texture2D image)
		{
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput();

			// Start new input
			m_InputTexture = image;
			CreateBuffersForTexture( m_InputTexture );

			m_Width = m_InputTexture.width;
			m_Height = m_InputTexture.height;

			_finalTexture = m_InputTexture;
			m_TextureHandle = 0;

			m_DeviceIndex = -1;

			// Resume tracking
			if (wasTracking)
			{
				StartTracking();
			}

			return true;
		}

		public override bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight)
		{
			bool bAllGood = false;
			
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput();

			if (deviceIndex < GetNumCameras())
			{
				if ( m_KudanAR_Instance != null )
				{
					m_DeviceIndex = deviceIndex;
					
//					targetWidth = 1280;
//					targetHeight = 720;
					m_KudanAR_Instance.Call( "startCamera", deviceIndex, targetWidth, targetHeight );

					int[] cameraResolution = new int[ 2 ];
					cameraResolution = m_KudanAR_Instance.Call<int[]>( "getCameraResolution" );
					//
					if ( m_Width != cameraResolution[ 0 ] || m_Height != cameraResolution[ 1 ] )
					{
						_finalTexture = null;
						m_TextureHandle = 0;

						m_Width = cameraResolution[ 0 ];
						m_Height = cameraResolution[ 1 ];
					}
					//
					Debug.Log( "[KudanAR] Camera resolution: " + m_Width + " x " + m_Height );

					if (wasTracking)
					{
						StartTracking();
					}
					
					bAllGood = true;
				}
			}
			return bAllGood;
		}

		private void CreateBuffersForTexture(Texture texture)
		{
		}

		public override void StopInput()
		{
			StopTracking();

			if ( m_KudanAR_Instance != null )
			{
				m_KudanAR_Instance.Call( "stopCamera" );
			}

			_finalTexture = null;
			m_TextureHandle = 0;
		}

		// Trackables
		public override bool AddTrackable(byte[] data, string id)
		{
			bool result = false;
			if (m_KudanAR_Instance != null )
			{
				result = m_KudanAR_Instance.Call<bool>( "AddTrackableSet", data, data.Length );
			}

			if ( result )
			{
				Trackable trackable = new Trackable();
				trackable.name = id;
				_trackables.Add( trackable );
			}

			return result;
		}

		private void UpdateFrameRates()
		{
			_rateTimer += Time.deltaTime;
			_numFramesRendered++;
			if (_rateTimer >= 1.0f)
			{
				_appRate = (float)(_numFramesRendered) / _rateTimer;

				_numFramesRendered = 0;
				_rateTimer = 0f;
			}

			if ( m_KudanAR_Instance != null )
			{
				_cameraRate = m_KudanAR_Instance.Call<float>( "getCameraDisplayFrameRate" );
				_trackerRate = m_KudanAR_Instance.Call<float>( "getTrackerFrameRate" );
			}
		}

		public override void StartTracking()
		{
			if (_isTrackingRunning)
			{
				Debug.LogWarning("[KudanAR] Trying to start tracking when it's already running");
				return;
			}

			_isTrackingRunning = true;
			
			if ( m_KudanAR_Instance != null )
			{
				m_KudanAR_Instance.Call( "SetTrackingEnabled", _isTrackingRunning );
			}
		}

		public override void StopTracking()
		{
			_isTrackingRunning = false;

			if ( m_KudanAR_Instance != null )
			{
				m_KudanAR_Instance.Call( "SetTrackingEnabled", _isTrackingRunning );
			}
		}

		public override bool EnableTrackingMethod( int trackingMethodId )
		{
			if ( m_KudanAR_Instance != null )
			{
				return m_KudanAR_Instance.Call<bool>( "EnableTrackingMethod", trackingMethodId );
			}
			return false;
		}

		public override bool DisableTrackingMethod(int trackingMethodId)
		{
			if ( m_KudanAR_Instance != null )
			{
				return m_KudanAR_Instance.Call<bool>( "DisableTrackingMethod", trackingMethodId );
			}
			return false;
		}

		public override bool GetMarkerRecoveryStatus() 
		{
			if (m_KudanAR_Instance != null) {
				return m_KudanAR_Instance.Call<bool> ("GetMarkerRecoveryStatus");
			} else {
				return false;
			}
		}

		public override void SetMarkerRecoveryStatus (bool status)
		{
			if (m_KudanAR_Instance != null)
			{
				m_KudanAR_Instance.Call ( "SetMarkerRecoveryStatus", status);
			}
		}

		public override void UpdateTracking()
		{
			UpdateRotation ();

			if ( m_KudanAR_Instance != null )
			{
				// Update projection matrix
				float[] projectionFloats = new float[ 16 ];
				projectionFloats = m_KudanAR_Instance.Call<float[]>( "GetProjectionMatrix", _cameraNearPlane, _cameraFarPlane );
				
				float fCameraAspectRatio = (float)( m_Width ) / (float)( m_Height );
				_projectionMatrix = ConvertNativeFloatsToMatrix( projectionFloats, fCameraAspectRatio );
				_projectionMatrix = _projectionMatrix * _projectionRotation;
			}

			if ( _isTrackingRunning )
			{
				if ( m_KudanAR_Instance != null )
				{
					// Create a texture if required
					if ( _finalTexture == null )
					{
						int iTextureHandle = m_KudanAR_Instance.Call<int>( "getTextureHandle" );
						if ( m_TextureHandle != iTextureHandle )
						{
							m_TextureHandle = iTextureHandle;
							_finalTexture = Texture2D.CreateExternalTexture( m_Width, m_Height, TextureFormat.RGBA32, false, false, new System.IntPtr( m_TextureHandle ) );
							Debug.Log("[KudanAR] m_TextureHandle: " + m_TextureHandle );
						}
					}

					// Update the Java side of things
					m_KudanAR_Instance.Call( "update" );
				}

                _detected = GetDetected();


			}

			// Update our frame rates
			UpdateFrameRates();
		}

		public void UpdateRotation()
		{
			ScreenOrientation currentOrientation = Screen.orientation;

			if (currentOrientation == _prevScreenOrientation) {
				return;
			}
				
			_prevScreenOrientation = currentOrientation;

			int[] indices;
			float fCameraAspectRatio = (float)( m_Width ) / (float)( m_Height );
			float projectionScale = 1.0f / fCameraAspectRatio;

			if (currentOrientation == ScreenOrientation.LandscapeLeft) {
				indices = new int[]{ 0, 1, 2, 3 };
				_projectionRotation = Matrix4x4.identity;
			} else if (currentOrientation == ScreenOrientation.Portrait) {
				indices = new int[]{ 2, 3, 1, 0 };
				_projectionRotation = Matrix4x4.TRS (Vector3.zero, Quaternion.AngleAxis(90, Vector3.back), new Vector3( projectionScale, projectionScale, 1 ));
			} else if (currentOrientation == ScreenOrientation.LandscapeRight) {
				indices = new int[]{ 1, 0, 3, 2 };
				_projectionRotation = Matrix4x4.TRS (Vector3.zero, Quaternion.AngleAxis(180, Vector3.back), Vector3.one);
			} else if (currentOrientation == ScreenOrientation.PortraitUpsideDown) {
				indices = new int[]{ 3, 2, 0, 1 };
				_projectionRotation = Matrix4x4.TRS (Vector3.zero, Quaternion.AngleAxis(270, Vector3.back), new Vector3( projectionScale, projectionScale, 1 ));			
			} else {
				return;
			}

			Vector3[] pos = new Vector3[4];

			pos [indices[0]] = new Vector3 (-0.5f, -0.5f, 0.0f);
			pos [indices[1]] = new Vector3 (0.5f, 0.5f, 0.0f);
			pos [indices[2]] = new Vector3 (0.5f, -0.5f, 0.0f);
			pos [indices[3]] = new Vector3 (-0.5f, 0.5f, 0.0f);

			_cameraBackgroundMeshFilter.mesh.vertices = pos;
		}

		public override void PostRender()
		{
			// Some versions of Unity have a bug where rendering to texture can only happen from OnPostRender

//			GL.InvalidateState();
//			GL.IssuePluginEvent(0);
//			GL.InvalidateState();

			if ( m_KudanAR_Instance != null )
            {
				GL.InvalidateState();
				m_KudanAR_Instance.Call<bool>( "render" );
				GL.InvalidateState();
			}
		}

        public override void ArbiTrackStart(Vector3 position, Quaternion orientation)
        {
            if (m_KudanAR_Instance != null)
            {
                m_KudanAR_Instance.Call("ArbiTrackStart", position.x, position.y, position.z, orientation.x, orientation.y, orientation.z, orientation.w);
            }
        }

        public override bool ArbiTrackIsTracking()
        {
            if (m_KudanAR_Instance != null)
            {
                return m_KudanAR_Instance.Call<bool>("ArbiTrackIsTracking");
            }
            else
            {
                return false;
            }
        }


        public override void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
        {
            position = new Vector3();
            orientation = new Quaternion();

            if (m_KudanAR_Instance != null)
            {
				m_KudanAR_Instance.Call ("updateArbi", _floorHeight);

                AndroidJavaObject floorPosition = m_KudanAR_Instance.Get<AndroidJavaObject>("m_FloorPosition");
                AndroidJavaObject floorOrientation = m_KudanAR_Instance.Get<AndroidJavaObject>("m_FloorOrientation");

                position.x = floorPosition.Get<float>("x");
                position.y = floorPosition.Get<float>("y");
                position.z = floorPosition.Get<float>("z");


                orientation.x = floorOrientation.Get<float>("x");
                orientation.y = floorOrientation.Get<float>("y");
                orientation.z = floorOrientation.Get<float>("z");
                orientation.w = floorOrientation.Get<float>("w");
            }
        }

        public override void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
        {
            position = new Vector3();
            orientation = new Quaternion();


            if (m_KudanAR_Instance != null)
            {
				m_KudanAR_Instance.Call ("updateArbi", _floorHeight);

                AndroidJavaObject arbiPosition = m_KudanAR_Instance.Get<AndroidJavaObject>("m_ArbiPosition");
                AndroidJavaObject arbiOrientation = m_KudanAR_Instance.Get<AndroidJavaObject>("m_ArbiOrientation");

                position.x = arbiPosition.Get<float>("x");
                position.y = arbiPosition.Get<float>("y");
                position.z = -arbiPosition.Get<float>("z");


                orientation.x = arbiOrientation.Get<float>("x");
                orientation.y = arbiOrientation.Get<float>("y");
                orientation.z = arbiOrientation.Get<float>("z");
                orientation.w = arbiOrientation.Get<float>("w");

//				orientation = orientation * Quaternion.AngleAxis(-90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.left);

				// return new Quaternion(x, y, z, w) * Quaternion.AngleAxis(-90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.left);
            }
        }

		private List<Trackable> GetDetected()
		{
            int num = 0;
			if ( m_KudanAR_Instance != null )
			{
				num = m_KudanAR_Instance.Call<int>( "GetNumberOfDetectedTrackables" );
			}

			// Grab detected trackables from java/native/kudan-lib into C# Unity land
			List<Trackable> result = new List<Trackable>(num);
			for (int i = 0; i < num; i++)
			{
				AndroidJavaObject thisTrackable = m_KudanAR_Instance.Call<AndroidJavaObject>( "GetTrackable", i );
				AndroidJavaObject thisTrackablePosition = thisTrackable.Get<AndroidJavaObject>( "m_Position" );
				AndroidJavaObject thisTrackableOrientation = thisTrackable.Get<AndroidJavaObject>( "m_Orientation" );

                Trackable trackable = new Trackable();
                trackable.name = thisTrackable.Get<string>( "m_Name" );
				trackable.width = thisTrackable.Get<int>( "m_Width" );
				trackable.height = thisTrackable.Get<int>( "m_Height" );
				//
				// Works for 180 rotated camera
//				trackable.position = new Vector3( thisTrackablePosition.Get<float>( "x" ), thisTrackablePosition.Get<float>( "y" ), -thisTrackablePosition.Get<float>( "z" ) );
//				trackable.orientation = new Quaternion( -thisTrackableOrientation.Get<float>( "y" ), thisTrackableOrientation.Get<float>( "x" ), thisTrackableOrientation.Get<float>( "w" ), -thisTrackableOrientation.Get<float>( "z" ) );
				//
				trackable.position = ConvertNativeFloatsToVector3( thisTrackablePosition.Get<float>( "x" ), thisTrackablePosition.Get<float>( "y" ), thisTrackablePosition.Get<float>( "z" ) );
				trackable.orientation = ConvertNativeFloatsToQuaternion( thisTrackableOrientation.Get<float>( "x" ), thisTrackableOrientation.Get<float>( "y" ), thisTrackableOrientation.Get<float>( "z" ), thisTrackableOrientation.Get<float>( "w" ) );

//				Debug.LogError( "[KudanAR] trackable.orientation: " + trackable.orientation );			
				
				result.Add( trackable );

                thisTrackable.Dispose();
			}

			return result;
		}
			
		// Utility functions for converting native data into Unity data
		public Matrix4x4 ConvertNativeFloatsToMatrix(float[] r, float cameraAspect)
		{
			Matrix4x4 m = new Matrix4x4();

			m.SetRow(0, new Vector4(r[0], r[1], r[2], r[3]));
			m.SetRow(1, new Vector4(r[4], r[5], r[6], r[7]));
			m.SetRow(2, new Vector4(r[8], r[9], r[10], r[11]));
			m.SetRow(3, new Vector4(r[12], r[13], r[14], r[15]));

			// Scale the aspect ratio based on camera vs screen ratios
			float screenAspect = ((float)Screen.width / (float)Screen.height);
			float scale = cameraAspect / screenAspect;

			if (scale > 1)
				m.m00 *= scale;
			else
				m.m11 /= scale;

			m = m.transpose;

			m.m02 *= -1f;
			m.m12 *= -1f;

/*
//			float width = 1280.0f;
//			float height = 768.0f;
			float width = 640.0f;
			float height = 480.0f;
			float focalX = 546.3904f;
			float focalY = 546.792f;
			float prinX = 313.824f;
			float prinY = 234.4488f;

			m.m00 = 2.0f * focalX / width;
			m.m01 = 0.0f;
			m.m02 = 2.0f * (prinX / width) - 1.0f;
			m.m03 = 0.0f;
			//
			m.m10 = 0.0f;
			m.m11 = 2.0f * focalY / height;
			m.m12 = 2.0f * (prinY / height) - 1.0f;
			m.m13 = 0.0f;
			//
			m.m20 = 0.0f;
			m.m21 = 0.0f;
			m.m22 = -(_cameraFarPlane + _cameraNearPlane) / (_cameraFarPlane - _cameraNearPlane);
			m.m23 = -2.0f * _cameraNearPlane * _cameraFarPlane / (_cameraFarPlane - _cameraNearPlane);
			//
			m.m30 = 0.0f;
			m.m31 = 0.0f;
			m.m32 = -1.0f;
			m.m33 = 0.0f;
*/

//			Debug.LogError( "[KudanAR] Projection matrix:\n" + m );

			return m;
		}

		protected static Vector3 ConvertNativeFloatsToVector3(float x, float y, float z)
		{
			return new Vector3(-x, -y, -z);
//			return new Vector3(x, y, -z);
		}

		protected static Quaternion ConvertNativeFloatsToQuaternion(float x, float y, float z, float w)
		{
			return new Quaternion(x, y, z, w) * Quaternion.AngleAxis(-90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.left);
//			return new Quaternion(x, y, z, w);
		}
	}
};

#endif
