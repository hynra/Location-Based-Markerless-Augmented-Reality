using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	[CustomEditor(typeof(MarkerTransformDriver))]
	/// <summary>
	/// Class that creates a custom inspector entry for the MarkerTransformDriver script.
	/// </summary>
	public class MarkerTransformDriverEditor : Editor
	{
		public Texture2D _kudanLogo;
		private MarkerTransformDriver _driver;

		void Awake()
		{
			_driver = (MarkerTransformDriver)target;
		}

		public override void OnInspectorGUI()
		{
			if (_driver == null) 
			{
				return;
			}

			GUILayout.BeginVertical();

			this.DrawDefaultInspector();

			GUILayout.Space(16f);

			if (_driver._markerPlaneWidth > 0 && _driver._markerPlaneHeight > 0)
			{
				if (GUILayout.Button("Set Scale From Marker Size"))
				{
					_driver.SetScaleFromMarkerSize();
				}
			}

			GUILayout.EndVertical();
		}
	}
}
