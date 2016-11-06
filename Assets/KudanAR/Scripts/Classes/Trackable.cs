using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	/// <summary>
	/// A Trackable is something that the tracker can detect.
	/// </summary>
	public class Trackable
	{
		/// <summary>
		/// The name of the trackable.
		/// </summary>
		public string name;

		/// <summary>
		/// Width of the trackable, in pixels.
		/// </summary>
		public int width;

		/// <summary>
		/// Height of the trackable, in pixels.
		/// </summary>
		public int height;

		/// <summary>
		/// Is this trackable currently being detected?
		/// </summary>
		public bool isDetected;

		/// <summary>
		/// The position of this trackable in 3D space.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// The orientation of this trackable in 3D space.
		/// </summary>
		public Quaternion orientation;

		/// <summary>
		/// The method used to track this trackable object.
		/// </summary>
		public int trackingMethod;
	}
};