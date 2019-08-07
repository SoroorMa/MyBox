﻿using MyBox.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MyBox
{
	/// <summary>
	/// Prepare() called on every MonoBehaviour by IPrepareFeature class. If Prepare() returns true, parent scene will be marked dirty 
	/// </summary>
	public interface IPrepare
	{
		bool Prepare();
	}
}

#if UNITY_EDITOR
namespace MyBox.Internal
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;

	[InitializeOnLoad]
	public class IPrepareFeature
	{
		public static bool IsEnabled = true;

		static IPrepareFeature()
		{
			MyEditorEvents.BeforePlaymode += PrepareOnPlay;
		}

		private static void PrepareOnPlay()
		{
			if (!IsEnabled) return;
			
			RunPrepare();
		}
		
		/// <summary>
		/// Calls Prepare() on any MonoBehaviour with IPrepare interface. If Prepare() returns true, parent scene will be marked dirty
		/// </summary>
		public static void RunPrepare()
		{
			
			var toPrepare = MyExtensions.FindObjectsOfInterfaceAsComponents<IPrepare>();

			HashSet<Scene> modifiedScenes = null;
			foreach (var prepare in toPrepare)
			{
				bool changed = prepare.Interface.Prepare();

				if (changed && prepare.Component != null)
				{
					if (modifiedScenes == null) modifiedScenes = new HashSet<Scene>();
					modifiedScenes.Add(prepare.Component.gameObject.scene);

					EditorUtility.SetDirty(prepare.Component);
					Debug.Log(prepare.Component.name + "." + prepare.Component.GetType().Name + ": Changed on Prepare", prepare.Component);
				}
			}

			if (modifiedScenes != null) EditorSceneManager.SaveScenes(modifiedScenes.ToArray());
		}
	}
}
#endif