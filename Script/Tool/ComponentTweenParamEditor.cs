#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public partial class ComponentTweenParam
	{
		private bool[] _lockCache = new bool[4];

		public void OnGUI(float totalTime)
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var end = End;
				if (Module == null || Module.ParamType != typeof(bool))
					EditorGUILayout.MinMaxSlider(ref Start, ref end, 0f, totalTime);
				else
					Start = EditorGUILayout.Slider(Start, 0f, totalTime);
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
						// bool の場合は長さを指定できない
						if (Module == null || Module.ParamType != typeof(bool))
							Length = EditorGUILayout.FloatField("Length", Length);

						if (check.changed)
						{
							Start = Mathf.Clamp(Start, 0, totalTime);
							if (Module == null || Module.ParamType != typeof(bool))
								Length = Mathf.Clamp(Length, 0.01f, totalTime - Start);
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
						if (Module.ParamType != typeof(bool))
							Length = 0f;
						BeginValue.Value = EndValue.Value = Vector4.zero;
						Lock = 0;
						IsRelative = false;
					}
				}

				if (Module != null)
				{
					DrawParam();
				}

				if (Module == null || Module.ParamType != typeof(bool))
				{
					IsRelative = EditorGUILayout.Toggle("IsRelative", IsRelative);
					EaseType = (EaseType) EditorGUILayout.EnumPopup("EaseType", EaseType);
					if (EaseType == EaseType.AnimationCurve)
					{
						Curve = EditorGUILayout.CurveField("Curve", Curve);
					}
				}

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

			if (Module.ParamType == typeof(Vector2))
				DrawLock(2);
			if (Module.ParamType == typeof(Vector3))
				DrawLock(3);
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

		private static string[] VectorName = {"x", "y", "z", "w"};

		private void DrawCustomVector(string label, ref TweenValue value, bool requireZ)
		{
			var loop = requireZ ? 3 : 2;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(label);
				var _cacheValue = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 10;
				var v = value.GetVector3();
				using (var check = new EditorGUI.ChangeCheckScope())
				{
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

		private void DrawLock(int displayCount = 3)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Lock");

				var _cacheValue = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 10;

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					for (var i = 0; i < displayCount; i++)
						_lockCache[i] =
							EditorGUILayout.Toggle(VectorName[i], Lock.HasFlags((LockValue) (1 << (i + 1))));

					if (check.changed)
					{
						Lock = 0;
						for (var i = 0; i < displayCount; i++)
							if (_lockCache[i])
								Lock |= (LockValue) (1 << (i + 1));
					}
				}
				EditorGUIUtility.labelWidth = _cacheValue;
			}
		}
	}
}
#endif
