using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[System.Serializable]
	/// <summary>
	/// Trackable Data is a set that contains marker data.
	/// </summary>
	public class TrackableData : ScriptableObject
	{
		/// <summary>
		/// The name of this trackable data set.
		/// </summary>
		public string id;

		[Header("Optional")]

		/// <summary>
		/// Preview image of the data set.
		/// </summary>
		public Texture2D image;

		[HideInInspector]
		/// <summary>
		/// The data.
		/// </summary>
		public byte[] data;

		/// <summary>
		/// Gets the ID.
		/// </summary>
		/// <value>The I.</value>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>The data.</value>
		public byte[] Data
		{
			get { return data; }
		}

		/// <summary>
		/// Set ID and data using the given ID and data
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="data">Data.</param>
		public void Set(string id, byte[] data)
		{
			this.id = id;
			this.data = data;
		}

#if UNITY_EDITOR
		[Multiline(8)]
		/// <summary>
		/// Optional notes to keep track of useful information, such as individual marker names.
		/// </summary>
		public string notes;
#endif

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Kudan AR Trackable Data")]
		/// <summary>
		/// Creates a new trackable asset using KARMarker data from a selected file.
		/// </summary>
		public static void EditorCreateIssue()
		{
			string path = UnityEditor.EditorUtility.OpenFilePanel("Kudan AR", "", "KARMarker");
			if (!string.IsNullOrEmpty(path))
			{
				TrackableData obj = ScriptableObject.CreateInstance<TrackableData>();
				UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/NewKudanTrackable.asset");
				UnityEditor.AssetDatabase.SaveAssets();

				obj.id = System.IO.Path.GetFileNameWithoutExtension(path);
				obj.data = System.IO.File.ReadAllBytes(path);
				UnityEditor.EditorUtility.SetDirty(obj);

				UnityEditor.EditorUtility.FocusProjectWindow();
				UnityEditor.Selection.activeObject = obj;
			}
		}
#endif
	}
}