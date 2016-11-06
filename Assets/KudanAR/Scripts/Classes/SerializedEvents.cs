using UnityEngine;

namespace Kudan.AR
{
	[System.Serializable]
	/// <summary>
	/// The Marker Lost Event, which is triggered when the tracker stops tracking a target.
	/// </summary>
	public class MarkerLostEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}

	[System.Serializable]
	/// <summary>
	/// The Marker Found Event, which is triggered when the tracker starts tracking a target.
	/// </summary>
	public class MarkerFoundEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}

	[System.Serializable]
	/// <summary>
	/// The Marker Update Event, which is triggered each frame the target is active
	/// </summary>
	public class MarkerUpdateEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}
}
