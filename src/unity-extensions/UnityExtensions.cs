using UnityEngine;
using UnityEngine.UI;

namespace BeatThat
{
	/// <summary>
	/// Extension methods for Unity's core classes, e.g. Component
	/// </summary>
	public static class UnityExtensions
	{
		/// <summary>
		/// syntactic sugar. Lets you chain Min calls into a single line without using (allocatings) Mathf.Min(params float[]) overload.
		/// </summary>
		/// <param name="f">F.</param>
		/// <param name="x">The x coordinate.</param>
		public static float Min(this float f, float x)
		{
			return Mathf.Min(f, x);
		}

		/// <summary>
		/// syntactic sugar. Lets you chain Max calls into a single line without using (allocatings) Mathf.Max(params float[]) overload.
		/// </summary>
		/// <param name="f">F.</param>
		/// <param name="x">The x coordinate.</param>
		public static float Max(this float f, float x)
		{
			return Mathf.Max(f, x);
		}

		public static float GetAspect(this Texture2D tex)
		{
			if(tex.width <= 0) {
				Debug.LogWarning("Texture has no width");
				return float.NaN;
			}


			return (float)tex.height / (float)tex.width;
		}

		public static void VisitAll<T>(this Component c, System.Action<T> visit)
		{
			c.gameObject.VisitAll<T>(visit);
		}

		/// <summary>
		/// Syntax sugar to perform an 'visit' action on all of a GameObject's child components of type T.
		/// Includes components on inactive GameObjects.
		///
		/// example: activate all child renderers
		///
		/// <code>
		/// this.gameObject.VisitAll<Renderer>((r) => {
		/// 	r.enabled = true;
		/// });
		/// </code>
		///
		/// NOTE: it is better to use this function with a visit action that does NOT capture local params (to avoid allocation).
		/// </summary>
		/// <param name="go">extension method 'this'</param>
		/// <param name="visit">Action that will be performed on all found components</param>
		/// <typeparam name="T">The component type to visit.</typeparam>
		public static void VisitAll<T>(this GameObject go, System.Action<T> visit)
		{
			using(var list = ListPool<T>.Get()) {
				go.GetComponentsInChildren<T>(true, list);
				foreach(var c in list) {
					visit(c);
				}
			}
		}

		public static void VisitActive<T>(this Component c, System.Action<T> visit)
		{
			c.gameObject.VisitActive<T>(visit);
		}

		/// <summary>
		/// Same as GameObject::VisitAll but visits only components on active GameObjects
		///
		/// example: activate all child renderers
		///
		/// <code>
		/// this.gameObject.VisitActive<Renderer>((r) => {
		/// 	r.enabled = true;
		/// });
		/// </code>
		///
		/// NOTE: it is better to use this function with a visit action that does NOT capture local params (to avoid allocation).
		/// </summary>
		/// <param name="go">extension method 'this'</param>
		/// <param name="visit">Action that will be performed on all found components</param>
		/// <typeparam name="T">The component type to visit.</typeparam>
		public static void VisitActive<T>(this GameObject go, System.Action<T> visit)
		{
			using(var list = ListPool<T>.Get()) {
				go.GetComponentsInChildren<T>(false, list);
				foreach(var c in list) {
					visit(c);
				}
			}
		}


		/// <summary>
		/// Sets the position of a transform to align with some other transform.
		/// Handles RectTransforms, setting their anchored position regardless of pivot.
		///
		/// Not tested for all cases, but useful when you have instantiated a RectTransform prefab
		/// that has a non-center pivot point, and you want it to fill the exact space of it's new parent.
		/// </summary>
		public static void MatchPosition(this Transform t, Transform match)
		{
			Vector3 startPosGlobal;
			var matchRT = match as RectTransform;
			if(matchRT != null) {
				startPosGlobal = (matchRT.parent != null)? matchRT.parent.TransformPoint(matchRT.anchoredPosition): (Vector3)matchRT.anchoredPosition;
			}
			else {
				startPosGlobal = match.position;
			}

			var startPosLocal = (t.parent != null)? t.parent.InverseTransformPoint(startPosGlobal): startPosGlobal;


			var rectTransform = t as RectTransform;
			if(rectTransform != null) {
				rectTransform.anchoredPosition = startPosLocal;
			}
			else {
				t.localPosition = startPosLocal;
			}
		}

	}
}
