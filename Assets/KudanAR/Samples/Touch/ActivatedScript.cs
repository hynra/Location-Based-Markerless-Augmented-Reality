using UnityEngine;
using System.Collections;

/// <summary>
/// Used in the touch sample. Tapping the screen in that sample activates this script.
/// </summary>
public class ActivatedScript : MonoBehaviour 
{
	void Update()
	{
		// All this script does for this sample is slowly spin the object it is attached to
		this.transform.Rotate (0, 1, 0);
	}
}