using System;
using System.Linq;
using UnityEngine;
using Yorozu.Easing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yorozu.ComponentTween
{
	[Serializable]
	public class ComponentTweenParam
	{
		[SerializeReference]
		public ModuleAbstract Module;

		public EaseType EaseType = EaseType.Linear;
		/// <summary>
		/// 開始時間
		/// </summary>
		public float Start;
		/// <summary>
		/// 長さ
		/// </summary>
		public float Length;

		public float End => Start + Length;

		public LockValue LockValue = LockValue.None;

		/// <summary>
		/// 値
		/// </summary>
		public TweenValue BeginValue;
		public TweenValue EndValue;

		/// <summary>
		/// 相対的か
		/// </summary>
		public bool IsRelative;


#if UNITY_EDITOR

		public void OnGUI(float totalTime)
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var end = End;
				EditorGUILayout.MinMaxSlider(ref Start, ref end, 0f, totalTime);
				if (check.changed)
				{
					GUI.FocusControl("");
					Start = (int) (Start * 100) / 100f;
					end = (int) (end * 100) / 100f;
					Length = end - Start;
				}
			}
		}

		public void OnGUIDetail(float totalTime, Type[] moduleTypes, TweenTarget target)
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{
				// Time
				using (new EditorGUILayout.HorizontalScope())
				{
					var _cacheValue = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 50;

					using (var check = new EditorGUI.ChangeCheckScope())
					{
						Start = EditorGUILayout.FloatField("Start", Start);
						Length = EditorGUILayout.FloatField("Length", Length);
						if (check.changed)
						{
							Start = Mathf.Clamp(Start, 0, Length);
							Length = Mathf.Clamp(Length, Start, totalTime);
						}
					}

					EditorGUIUtility.labelWidth = _cacheValue;
				}

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var index = -1;
					if (Module != null)
					{
						for (var i = 0; i < moduleTypes.Length; i++)
						{
							if (moduleTypes[i] == Module.GetType())
							{
								index = i;
								break;
							}
						}
					}


					index = EditorGUILayout.Popup("Module", index, moduleTypes.Select(t => t.Name).ToArray());
					if (check.changed)
					{
						Module = (ModuleAbstract) Activator.CreateInstance(moduleTypes[index]);
						BeginValue.Value = EndValue.Value = Vector4.zero;
					}
				}

				EaseType = (EaseType) EditorGUILayout.EnumPopup("EaseType", EaseType);


				// DrawObjectList
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Target Objects", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
					{
						ArrayUtility.Add(ref target.TargetObjects, null);
					}
				}

				for (var i = 0; i < target.TargetObjects.Length; i++)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						target.TargetObjects[i] = (GameObject) EditorGUILayout.ObjectField(target.TargetObjects[i], typeof(GameObject), true);

						if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
						{
							ArrayUtility.RemoveAt(ref target.TargetObjects, i);
						}
					}
				}
			}
		}

#endif
	}
}
