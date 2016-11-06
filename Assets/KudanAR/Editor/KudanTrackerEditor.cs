using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	[CustomEditor(typeof(KudanTracker))]
	/// <summary>
	/// Script that creates a custom inspector entry for the Kudan Tracker. 
	/// </summary>
	public class KudanTrackerEditor : Editor
	{
		//private KudanTracker _target;

		void Awake()
		{
			//_target = (KudanTracker)target;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginVertical();


			this.DrawDefaultInspector();

			GUILayout.Space(16f);

			EditorGUILayout.LabelField("App/Bundle ID:", PlayerSettings.bundleIdentifier);

			if (GUILayout.Button("Set App/Bundle ID"))
			{
				EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
			}

			GUILayout.Space(5f);
			if (GUILayout.Button("Get Editor API Key"))
			{
				Application.OpenURL("https://www.kudan.eu/api/");
			}
			GUILayout.Space(5f);

			if (GUILayout.Button("Get Support"))
			{
				Application.OpenURL("https://www.kudan.eu/support/");
			}

			
			//TrackingMethodBase[] trackers = (TrackingMethodBase[])Resources.FindObjectsOfTypeAll(typeof(TrackingMethodBase));
			
			//typeof(TrackingMethodMarkerless)

			bool externalOperation = false;

			GUILayout.EndVertical();

			if (externalOperation)
			{
				// This has to be here otherwise we get strange GUI stack exceptions
				EditorGUIUtility.ExitGUI();
			}			
		}
	}
}