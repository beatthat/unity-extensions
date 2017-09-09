﻿using UnityEngine;
using System.Text;
using System.Collections.Generic;
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

		public static Component AddIfMissing(this Component targetObject, System.Type concreteType)
		{
			return targetObject.gameObject.AddIfMissing(concreteType, concreteType);
		}

		public static Component AddIfMissing(this GameObject targetObject, System.Type concreteType)
		{
			return targetObject.AddIfMissing(concreteType, concreteType);
		}

		public static Component AddIfMissing(this Component targetObject, System.Type interfaceType, System.Type concreteType)
		{
			return targetObject.gameObject.AddIfMissing(interfaceType, concreteType);
		}

		public static Component AddIfMissing(this GameObject targetObject, System.Type interfaceType, System.Type concreteType)
		{
			foreach(Component c in targetObject.GetComponents<Component>()) {
				if(interfaceType.IsInstanceOfType(c)) {
					return c;
				}
			}

			return targetObject.AddComponent(concreteType) as Component;
		}

		public static T AddIfMissing<T>(this Component c)
			where T : Component
		{
			return c.gameObject.AddIfMissing<T, T>();
		}

		public static I AddIfMissing<I, T>(this Component c)
			where I : class
			where T : Component, I
		{
			return c.gameObject.AddIfMissing<I, T>();
		}

		/// <summary>
		/// If type matching interface (or base type) 'I' is missing adds a component of concrete type 'T' (which must implement 'I')
		/// </summary>
		/// <returns>The component that was either found or added.</returns>
		/// <param name="go">extension method 'this'</param>
		/// <typeparam name="I">The interface type to look for.</typeparam>
		/// <typeparam name="T">The concrete implementation type of I to add if no instance of I is found.</typeparam>
		public static T AddIfMissing<T>(this GameObject go)
			where T : Component
		{
			return go.AddIfMissing<T, T>();
		}

		/// <summary>
		/// If type matching interface (or base type) 'I' is missing adds a component of concrete type 'T' (which must implement 'I')
		/// </summary>
		/// <returns>The component that was either found or added.</returns>
		/// <param name="go">extension method 'this'</param>
		/// <typeparam name="I">The interface type to look for.</typeparam>
		/// <typeparam name="T">The concrete implementation type of I to add if no instance of I is found.</typeparam>
		public static I AddIfMissing<I, T>(this GameObject go)
			where I : class
			where T : Component, I
		{
			I inst = go.GetComponent<I>();
			if(inst == null) {
				inst = go.AddComponent<T>();
			}
			return inst;
		}

		/// <summary>
		/// Very expensive version of GameObject.FindObjectsOfType
		/// that allows interface types.
		/// Use only in very controlled circumstances.
		/// </summary>
		public static T FindObjectOfType<T>() where T : class
		{
			foreach(Object o in Object.FindObjectsOfType(typeof(Component))) {
				var t = o as T;
				if(t != null) {
					return t;
				}
			}
			return null;
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

		public static RectTransform GetViewport(this ScrollRect scrollRect)
		{
			// Analysis disable ConvertConditionalTernaryToNullCoalescing
			return scrollRect.viewport != null? scrollRect.viewport: scrollRect.transform as RectTransform;
			// Analysis restore ConvertConditionalTernaryToNullCoalescing
		}


		public static T GetComponentInParent<T>(this Component c, bool includeInactive = false, bool excludeSelf = false) where T : class
		{
			if(!excludeSelf && (includeInactive || c.gameObject.activeSelf)) {
				var comp = c.GetComponent<T>();
				if(comp != null) {
					return comp;
				}
			}

			Transform p = (c is Transform)? (c as Transform).parent: c.transform.parent;

			if(p == null) {
				return null;
			}

			return p.GetComponentInParent<T>(includeInactive, false);
		}

		/// <summary>
		/// Same as GetComponent but excludes the caller
		/// </summary>
		public static T GetSiblingComponent<T>(this Component primary, bool includeInactive = false) where T : class
		{
			using(var comps = ListPool<T>.Get()) {
				primary.GetComponents<T>(comps, includeInactive);
				foreach(var c in comps) {
					if(object.ReferenceEquals(c, primary)) { continue; }
					return c;
				}
			}
			return null;
		}

		/// <summary>
		/// Same as GetComponent but excludes the caller
		/// </summary>
		public static void GetSiblingComponents<T>(this Component primary, List<T> results, bool includeInactive = false) where T : class
		{
			primary.GetComponents<T>(results, includeInactive);
			for(int i = results.Count - 1; i >= 0; i--) {
				if(object.ReferenceEquals(primary, results[i])) {
					results.RemoveAt(i);
				}
			}
		}

		public static void GetComponents<T>(this Component t, List<T> results, bool includeInactive) where T : class
		{
			if(includeInactive) {

				using(var tmp = ListPool<T>.Get()) {
					t.GetComponents<T>(tmp);
					results.AddRange(tmp);
				}
			}
			else {
				using(var tmp = ListPool<T>.Get()) {
					t.GetComponents<T>(tmp);
					foreach(var c in tmp) {
						if((c as Component).gameObject.activeInHierarchy) {
							results.Add(c);
						}
					}
				}
			}
		}

		public static T GetComponentInDirectChildren<T>(this Transform t, bool includeInactive = false) where T : class
		{
			T c = t.GetComponent<T>();
			if(c != null) {
				return c;
			}

			foreach(Transform childT in t) {
				if((c = childT.GetComponent<T>()) != null
					&& (includeInactive || (c as Component).gameObject.activeInHierarchy)) {
					return c;
				}
			}
			return null;
		}

		public static void GetComponentsInDirectChildren<T>(this Transform t, List<T> results, bool includeInactive = false) where T : class
		{
			var n = t.childCount;

			for(int i = 0; i < n; i++) {
				var child = t.GetChild(i);
				child.GetComponents<T>(results, includeInactive);
			}
		}

		public static void GetComponentsInTrueChildren<T>(this GameObject go, List<T> results, bool includeInactive = false) where T : class
		{
			go.transform.GetComponentsInTrueChildren<T>(results, includeInactive);
		}

		public static void GetComponentsInTrueChildren<T>(this Component c, List<T> results, bool includeInactive = false) where T : class
		{
			c.transform.GetComponentsInTrueChildren<T>(results, includeInactive);
		}

		public static void GetComponentsInTrueChildren<T>(this Transform t, List<T> results, bool includeInactive = false) where T : class
		{
			t.GetComponentsInChildren<T>(includeInactive, results);
			for(int i = results.Count - 1; i >= 0; i--) {
				if((results[i] as Component).transform == t) {
					results.RemoveAt(i);
				}
			}
		}

		private static void GetComponentsInChildren<T>(Transform t, List<T> results, bool includeInactive = false, int depth = 0, int maxDepth = int.MaxValue) where T : class
		{
			if(depth > maxDepth) {
				return;
			}

			// TODO: this needs to be retested. Seems like GetComponents may now be clearing the result list on every call???
			t.GetComponents<T>(results, includeInactive);

			if(depth == maxDepth) {
				return;
			}

			foreach(Transform childT in t) {
				GetComponentsInChildren<T>(childT, results, includeInactive, depth + 1, maxDepth);
			}
		}


		public static void SetHideFlagsRecursively(this GameObject go, HideFlags flags)
		{
			go.hideFlags = flags;
			foreach(Transform c in go.transform) {
				SetHideFlagsRecursively(c.gameObject, flags);
			}
		}

		// Recursively set the layer of this object and all its children
		public static void SetLayerRecursively(this GameObject go, int newLayer, bool includeInactive = false)
		{
			using(var tmp = ListPool<Transform>.Get()) {

				go.layer = newLayer;

				go.GetComponentsInChildren<Transform>(true, tmp);

				foreach(var c in tmp) {
					c.gameObject.layer = newLayer;
				}
			}
		}

		/// <summary>
		/// Determines whether Transform t2 is an ancestor (parent or beyond) of the caller.
		/// </summary>
		public static bool IsAncestorOf(this Transform t, Transform t2)
		{
			Transform p = t2;
			while((p = p.parent) != null) {
				if(p == t) {
					return true;
				}
			}
			return false;
		}


		public static Color WithAlpha(this Color c, float a)
		{
			return new Color(c.r, c.g, c.b, a);
		}

		public static T FindByTag<T>(this GameObject caller, string tag) where T : Component
		{
			if(string.IsNullOrEmpty(tag)) {
				return null;
			}

			var go = GameObject.FindGameObjectWithTag(tag);
			if(go == null) {
				Debug.LogWarning("[" + Time.frameCount + "][" + caller.Path() + "] failed to find " + typeof(T).Name + " by tag '" + tag + "'");
				return null;
			}

			var c = go.GetComponent<T>();
			if(c == null) {
				Debug.LogWarning("[" + Time.frameCount + "][" + caller.Path() + "] object with tag '"
					+ tag + "' is missing " + typeof(T).Name + " component");
			}

			return c;
		}
	}
}
