using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public class TrackeriOS : TrackerBase
	{
		private Renderer _background;
		private MeshFilter _cameraBackgroundMeshFilter;
		
		private Texture2D _textureYp;
		private int _textureYpID;

		private Texture2D _textureCbCr;
		private int _textureCbCrID;
		
		private float _cameraAspect = 1.0f;

		private int _numFramesRenderedLast = 0;
		private int _numFramesRendered = 0;
		private float _rateTimer = 0.0f;

		private ScreenOrientation _prevScreenOrientation;
		private Matrix4x4 _projectionRotation = Matrix4x4.identity;
		
		//-------------------------------------------------------------------------------------------------------------//
		// Plugin Interface
		// - Supplements NativeInterface.cs
		//-------------------------------------------------------------------------------------------------------------//
		
		[DllImport("__Internal")]
		private static extern System.IntPtr GetRenderEventFunc();
		
		[DllImport("__Internal")]
		private static extern void SetApiKeyNative(string key, string bundleId);

		[DllImport("__Internal")]
		private static extern System.IntPtr GetTextureForPlane(int plane, ref int width, ref int height);
		
		[DllImport("__Internal")]
		private static extern int GetCaptureDeviceCount();
		
		[DllImport("__Internal")]
		private static extern bool BeginCaptureSession(int deviceIndex, int targetWidth, int targetHeight);
		
		[DllImport("__Internal")]
		private static extern void EndCaptureSession();

		[DllImport("__Internal")]
		private static extern void BeginTracking();

		[DllImport("__Internal")]
		private static extern void EndTracking();
		
		[DllImport("__Internal")]
		private static extern float GetCaptureDeviceRate();
		
		[DllImport("__Internal")]
		private static extern float GetTrackerRate();
		
		[DllImport("__Internal")]
		private static extern void GetDetectedTrackable(int index, StringBuilder name, int nameSize, ref int width, ref int height, float[] pos, float[] ori);
		
		//-------------------------------------------------------------------------------------------------------------//
		
		public TrackeriOS(Renderer background)
		{
			_background = background;
			_cameraBackgroundMeshFilter = background.GetComponent<MeshFilter> ();

			SetYpCbCrMaterialOnBackground();
		}
		
		public override bool InitPlugin()
		{
			if (NativeInterface.Init())
			{
				return true;
			}
			return false;
		}

		public override void DeinitPlugin()
		{
			NativeInterface.Deinit();
		}

		public override float GetNativePluginVersion()
		{
			return NativeInterface.GetPluginVersion();
		}

		public override void OnApplicationFocus( bool focusStatus )
		{
		}

		public override void OnApplicationPause( bool pauseStatus )
		{
		}
		
		// Cameras
		public override int GetNumCameras()
		{
			return GetCaptureDeviceCount();
		}

		// Start sources
		public override bool StartInputFromImage(Texture2D image)
		{
			return false;
		}

		public override bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight)
		{
			if (BeginCaptureSession(deviceIndex, targetWidth, targetHeight))
			{
				StartTracking();
				return true;
			}
			return false;
		}

		public override void StopInput()
		{
			EndCaptureSession();
		}

		// Trackables
		public override bool AddTrackable(byte[] data, string id)
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			bool result = NativeInterface.AddTrackableSet(handle.AddrOfPinnedObject(), data.Length);
			handle.Free();

			if (result)
			{
				Trackable trackable = new Trackable();
				trackable.name = id;
				_trackables.Add(trackable);
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
		
		private const int kKudanARRenderEventId = 0x0a736f21;
		
		// Fire events etc
		public override void UpdateTracking()
		{
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4
			GL.IssuePluginEvent(GetRenderEventFunc(), kKudanARRenderEventId);
#else
			GL.IssuePluginEvent(kKudanARRenderEventId);
#endif
			UpdateBackground();
			_cameraRate = GetCaptureDeviceRate();
			
			_trackerRate = GetTrackerRate();

			_rateTimer += Time.deltaTime;
			_numFramesRendered++;
			if (_rateTimer >= 1.0f)
			{
				_appRate = (float)(_numFramesRendered - _numFramesRenderedLast) / _rateTimer;
				
				_numFramesRenderedLast = _numFramesRendered;
				_rateTimer = 0f;
			}

			// Grab the detected trackables.
			_detected = GetDetected();

			// Grab the projection matrix.
			float[] projectionFloats = new float[16];
			NativeInterface.GetProjectionMatrix (_cameraNearPlane, _cameraFarPlane, projectionFloats);
			_projectionMatrix = ConvertNativeFloatsToMatrix (projectionFloats, _cameraAspect);

			UpdateRotation();

			// Transform the projection matrix depending on orientation.
			_projectionMatrix = _projectionMatrix * _projectionRotation;
		}

		public void UpdateRotation()
		{
			ScreenOrientation currentOrientation = Screen.orientation;

			if (currentOrientation == _prevScreenOrientation) {
				return;
			}

			Debug.Log(currentOrientation);

			_prevScreenOrientation = currentOrientation;
			float projectionScale = 1.0f / _cameraAspect;

			int[] indices;

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

		}
		
		// Tracking
		public override void StartTracking()
		{
			BeginTracking();
			_isTrackingRunning = true;
		}

		public override void StopTracking()
		{
			EndTracking();
			_isTrackingRunning = false;
		}
		
		public override bool EnableTrackingMethod(int trackingMethodId)
		{
			return NativeInterface.EnableTrackingMethod(trackingMethodId);
		}

		public override bool DisableTrackingMethod(int trackingMethodId)
		{
			return NativeInterface.DisableTrackingMethod(trackingMethodId);
		}

		// Licensing
		public override void SetApiKey(string key, string bundleId)
		{
			SetApiKeyNative(key, bundleId);
		}

		// Utility functions for converting native data into Unity data
		public static Matrix4x4 ConvertNativeFloatsToMatrix(float[] r, float cameraAspect)
		{
			Matrix4x4 m = new Matrix4x4();
			m.SetRow(0, new Vector4(r[ 0], r[ 1], r[ 2], r[ 3]));
			m.SetRow(1, new Vector4(r[ 4], r[ 5], r[ 6], r[ 7]));
			m.SetRow(2, new Vector4(r[ 8], r[ 9], r[10], r[11]));
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
			
			return m;
		}
		
		protected static Vector3 ConvertNativeFloatsToVector3(float x, float y, float z)
		{
			return new Vector3(-x, -y, -z);
		}
		
		protected static Quaternion ConvertNativeFloatsToQuaternion(float x, float y, float z, float w)
		{
			return new Quaternion(x, y, z, w) * Quaternion.AngleAxis(-90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.left);
		}
		
		private List<Trackable> GetDetected()
		{
			// Grab detected trackables from nativeland into C# Unity land
			int num = NativeInterface.GetNumberOfDetectedTrackables();
			List<Trackable> result = new List<Trackable>(num);
			for (int i = 0; i < num; i++)
			{
				Trackable trackable = new Trackable();
				StringBuilder sbName = new StringBuilder(512);
				int width = 0, height = 0;
				float[] pos = new float[3];
				float[] ori = new float[4];
//				NativeInterface.Get(p, sbName);
				GetDetectedTrackable(i, sbName, 512, ref width, ref height, pos, ori);

				trackable.name = sbName.ToString();
				trackable.width = width;
				trackable.height = height;
				trackable.position = ConvertNativeFloatsToVector3(pos[0], pos[1], pos[2]);
				trackable.orientation = ConvertNativeFloatsToQuaternion(ori[0], ori[1], ori[2], ori[3]);
				result.Add(trackable);
			}
			return result;
		}

		private void SetYpCbCrMaterialOnBackground()
		{
			Material matYpCbCr = Resources.Load("YpCbCr", typeof(Material)) as Material;
			if (matYpCbCr != null)
			{
				_background.material = matYpCbCr;

				_textureYpID = Shader.PropertyToID("Yp");
				Debug.Log("_textureYpID == " + _textureYpID);

				_textureCbCrID = Shader.PropertyToID("CbCr");
				Debug.Log("_textureCbCrID == " + _textureCbCrID);

				_background.material.SetTextureScale("Yp", new Vector2(1.0f, -1.0f));
				_background.material.SetTextureOffset("Yp", new Vector3(0.0f, 1.0f));
				_background.material.SetTextureScale("CbCr", new Vector2(1.0f, -1.0f));
				_background.material.SetTextureOffset("CbCr", new Vector3(0.0f, 1.0f));
			}
			else
			{
				Debug.LogError("[KudanAR] Failed to load YpCbCr material");
			}
		}
		
		private void UpdateBackground()
		{
			int width = 0, height = 0;
			System.IntPtr texture = GetTextureForPlane(0, ref width, ref height);
			if (texture != System.IntPtr.Zero)
			{
				if (_textureYp == null)
				{
					Debug.Log("CreateExternalTexture(" + width + ", " + height + ", TextureFormat.Alpha8, false, false, " + texture + ")");
					_textureYp = Texture2D.CreateExternalTexture(width, height, TextureFormat.Alpha8, false, false, texture);
					_cameraAspect = (float)width / (float)height;
					_finalTexture = _textureYp;
				}
				else
				{
					_textureYp.UpdateExternalTexture(texture);
				}
			}

			texture = GetTextureForPlane(1, ref width, ref height);
			if (texture != System.IntPtr.Zero)
			{
				if (_textureCbCr == null)
				{
					Debug.Log("CreateExternalTexture(" + width / 2 + ", " + height + ", TextureFormat.RGBA32, false, false, " + texture + ")");
					_textureCbCr = Texture2D.CreateExternalTexture(width / 2, height, TextureFormat.RGBA32, false, false, texture);
				}
				else
				{
					_textureCbCr.UpdateExternalTexture(texture);
				}
			}

			if (_textureYp != null && _textureCbCr != null)
			{
				_background.material.SetTexture(_textureYpID, _textureYp);
				_background.material.SetTexture(_textureCbCrID, _textureCbCr);
			}
			else
			{
				Debug.LogError("[KudanAR] Failed to create external textures");
			}
		}

		public override void ArbiTrackStart(Vector3 position, Quaternion orientation)
		{
			float[] f = new float[7];
			
			f[0] = position.x;
			f[1] = position.y;
			f[2] = position.z;
			
			f[3] = orientation.x;
			f[4] = orientation.y;
			f[5] = orientation.z;
			f[6] = orientation.w;
			
			NativeInterface.ArbiTrackStart(f);
		}
		
		public override bool ArbiTrackIsTracking()
		{
			return NativeInterface.ArbiTrackIsTracking();
		}
		
		
		public override void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
		{
			float[] f = new float[7];
			
			NativeInterface.FloorPlaceGetPose(f, _floorHeight);
			
			position = new Vector3(f[0], f[1], f[2]);
			orientation = new Quaternion(f[3], f[4], f[5], f[6]);
		}
		
		public override void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
		{
			float[] result = new float[7];
			NativeInterface.ArbiTrackGetPose(result);
			
			position = new Vector3(result[0], result[1], -result[2]);
			orientation = new Quaternion(result[3], result[4], result[5], result[6]);
		}
	}
}
