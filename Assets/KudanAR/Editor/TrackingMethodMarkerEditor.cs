using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	[CustomEditor(typeof(TrackingMethodMarker))]
	/// <summary>
	/// Class that creates a custom inspector entry for TrackingMethodMarker.
	/// </summary>
	public class TrackingMethodMarkerEditor : Editor
	{
		//private TrackingMethodMarker _target;

		void Awake()
		{
			//_target = (TrackingMethodMarker)target;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginVertical();


			this.DrawDefaultInspector();

			GUILayout.Space(16f);

			
			bool externalOperation = false;

			if (GUILayout.Button("Add KARMarker Asset"))
			{
				externalOperation = true;
				TrackableData.EditorCreateIssue();
			}

			GUILayout.EndVertical();

			if (externalOperation)
			{
				// This has to be here otherwise we get strange GUI stack exceptions
				EditorGUIUtility.ExitGUI();
			}
		}
	}
}