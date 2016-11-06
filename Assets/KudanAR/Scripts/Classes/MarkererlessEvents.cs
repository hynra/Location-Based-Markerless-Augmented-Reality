using UnityEngine;
using System.Collections;

/// <summary>
/// (WIP) Class to detect when ArbiTracking starts, stops and if it is running each frame.
/// </summary>
public class MarkererlessEvents : MonoBehaviour 
{
	struct markerStruct
	{
		public GameObject markerless;
		public string name;
		public bool isActive;
		public bool wasActive;

		public void checkStart()
		{
			if (isActive && !wasActive)
			{
				name = markerless.name;
				Debug.LogWarning ("ArbiTracking Started: " + name);
				wasActive = true;
			}
		}

		public void checkTrack()
		{
			if (markerless.activeInHierarchy) {
				Debug.LogWarning ("ArbiTracking: " + name);
			} else
				markerless = null;
		}

		public void checkStop()
		{
			if (!isActive && wasActive) 
			{
				Debug.LogWarning ("ArbiTracking Stopped: " + name);
				wasActive = false;
			}
		}
	}

	GameObject[] markerlessArray;

	markerStruct[] markerlessObjs;

	public int numMaxEventTracking = 10;

	void Start () 
	{
		markerlessObjs = new markerStruct[numMaxEventTracking];
	}

	void Update ()
	{
		markerlessArray = GameObject.FindGameObjectsWithTag ("Markerless");

		if (markerlessArray.Length < numMaxEventTracking) 
		{
			Resize (numMaxEventTracking, ref markerlessArray);
		}

		for (int i = 0; i < numMaxEventTracking; i++)
		{
			markerlessObjs [i].markerless = markerlessArray [i];

			if (markerlessObjs [i].markerless == null) 
			{
				markerlessObjs [i].isActive = false;
				markerlessObjs [i].checkStop ();
			} 

			else 
			{
				markerlessObjs [i].isActive = true;
				markerlessObjs [i].checkStart ();
				markerlessObjs [i].checkTrack ();
			}
		}
	}

	void Resize(int size, ref GameObject[] array)
	{
		GameObject[] temp = new GameObject[size];
		for (int i = 0; i < Mathf.Min (size, array.Length); i++) 
		{
			temp [i] = array [i];
		}

		array = temp;
	}
}
