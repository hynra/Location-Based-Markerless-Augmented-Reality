using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	/// <summary>
	/// The base tracking method that other tracking methods inherit from.
	/// </summary>
	public abstract class TrackingMethodBase : MonoBehaviour
	{
		/// <summary>
		/// Reference to the Kudan Tracker.
		/// </summary>
		public KudanTracker _kudanTracker;

		/// <summary>
		/// The name of this tracking method.
		/// </summary>
		/// <value>The name.</value>
		public abstract string Name { get; }

		/// <summary>
		/// The ID of this tracking method.
		/// </summary>
		/// <value>The tracking method identifier.</value>
		public abstract int TrackingMethodId { get; }

		/// <summary>
		/// Is tracking enabled?
		/// </summary>
		protected bool _isTrackingEnabled;

		/// <summary>
		/// Gets the plugin interface.
		/// </summary>
		/// <value>The plugin.</value>
		public TrackerBase Plugin
		{
			get { return _kudanTracker.Interface; }
		}

		/// <summary>
		/// Gets a value indicating whether tracking is enabled.
		/// </summary>
		/// <value><c>true</c> if tracking enabled; otherwise, <c>false</c>.</value>
		public bool TrackingEnabled
		{
			get { return _isTrackingEnabled; }
		}

		/// <summary>
		/// Awake this instance.
		/// </summary>
		void Awake()
		{
			if (_kudanTracker == null)
			{
				_kudanTracker = FindObjectOfType<KudanTracker>();
			}
			if (_kudanTracker == null)
			{
				Debug.LogWarning("[KudanAR] Cannot find KudanTracker in scene", this);
			}
		}

		/// <summary>
		/// Initialise this instance.
		/// </summary>
		public virtual void Init()
		{
		}

		/// <summary>
		/// Starts tracking.
		/// </summary>
		public virtual void StartTracking()
		{
			if (Plugin != null)
			{
				if (Plugin.EnableTrackingMethod(TrackingMethodId))
				{
					_isTrackingEnabled = true;
				}
				else
				{
					Debug.LogError(string.Format("[KudanAR] Tracking method {0} not supported", TrackingMethodId));
				}
			}
		}

		/// <summary>
		/// Stops tracking.
		/// </summary>
		public virtual void StopTracking()
		{
			if (Plugin != null)
			{
				if (Plugin.DisableTrackingMethod(TrackingMethodId))
				{
					_isTrackingEnabled = false;
				}
				else
				{
					Debug.LogError(string.Format("[KudanAR] Tracking method {0} not supported", TrackingMethodId));
				}
			}
		}

		/// <summary>
		/// Processes the current frame.
		/// </summary>
		public virtual void ProcessFrame()
		{
		}

		/// <summary>
		/// Draws the debug GUI.
		/// </summary>
		/// <param name="uiScale">User interface scale.</param>
		public virtual void DebugGUI(int uiScale)
		{
		}
	}
}