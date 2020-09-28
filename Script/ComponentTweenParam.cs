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
		///     値
		/// </summary>
		public TweenValue BeginValue;
		public TweenValue EndValue;
		public LockValue Lock = LockValue.None;
		/// <summary>
		///     相対的か
		/// </summary>
		public bool IsRelative;
		/// <summary>
		///     開始時間
		/// </summary>
		public float Start;
		/// <summary>
		///     長さ
		/// </summary>
		public float Length;
		public float End => Start + Length;
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
						for (var i = 0; i < moduleTypes.Length; i++)
							if (moduleTypes[i] == Module.GetType())
							{
								index = i;

								break;
							}

					index = EditorGUILayout.Popup("Module", index, moduleTypes.Select(t => t.Name).ToArray());
					if (check.changed)
					{
						Module = (ModuleAbstract) Activator.CreateInstance(moduleTypes[index]);
						BeginValue.Value = EndValue.Value = Vector4.zero;
					}
				}

				if (Module != null)
				{
					DrawParam();
				}

				EaseType = (EaseType) EditorGUILayout.EnumPopup("EaseType", EaseType);

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Target Objects", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						ArrayUtility.Add(ref target.TargetObjects, null);
				}

				for (var i = 0; i < target.TargetObjects.Length; i++)
					using (new EditorGUILayout.HorizontalScope())
					{
						target.TargetObjects[i] =
							(GameObject) EditorGUILayout.ObjectField(target.TargetObjects[i], typeof(GameObject), true);

						if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton",
							GUILayout.Width(16)))
							ArrayUtility.RemoveAt(ref target.TargetObjects, i);
					}
			}
		}

		/// <summary>
		/// パラメータを描画
		/// </summary>
		private void DrawParam()
		{
			DrawParamValue("Begin", ref BeginValue);
			DrawParamValue("End", ref EndValue);
		}

		private void DrawParamValue(string label, ref TweenValue value)
		{
			var t = Module.ParamType;
			if (t == typeof(bool))
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var v = value.GetBool();
					v = EditorGUILayout.Toggle(label, v);
					if (check.changed)
						value.SetBool(v);
				}
			}
			else if (t == typeof(int))
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var v = value.GetInt();
					v = EditorGUILayout.IntField(label, v);
					if (check.changed)
						value.SetInt(v);
				}
			}
			else if (t == typeof(float))
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var v = value.GetFloat();
					v = EditorGUILayout.FloatField(label, v);
					if (check.changed)
						value.SetFloat(v);
				}
			}
			else if (t == typeof(Vector2))
			{
				DrawCustomVector(label, ref value, false);
			}
			else if (t == typeof(Vector3))
			{
				DrawCustomVector(label, ref value, true);
			}
			else if (t == typeof(Color))
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var v = value.GetColor();
					v = EditorGUILayout.ColorField(label, v);
					if (check.changed)
						value.SetColor(v);
				}
			}
		}

		private static string[] VectorName = {"x", "y", "z"};

		private void DrawCustomVector(string label, ref TweenValue value, bool requireZ)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(label);

				var _cacheValue = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 20;
				var v = value.GetVector3();
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var loop = requireZ ? 3 : 2;
					for (var i = 0; i < loop; i++)
					{
						using (new EditorGUI.DisabledScope(Lock.HasFlag((LockValue) (1 << (i + 1)))))
						{
							v[i] = EditorGUILayout.FloatField(VectorName[i], v[i]);
						}
					}

					if (check.changed)
						value.SetVector3(v);
				}

				EditorGUIUtility.labelWidth = _cacheValue;
			}
		}

#endif
	}
}
