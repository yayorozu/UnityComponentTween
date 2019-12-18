using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UniLib.UniEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniLib.RectTween.Editor
{
	[CustomEditor(typeof(RectTweenSequence))]
	public class RectTweenSequenceEditor : UnityEditor.Editor
	{
		private SerializedProperty _playOnAwake;
		private SerializedProperty _id;
		private SerializedProperty _totalTime;
		private SerializedProperty _isIgnoreTimeScale;
		private SerializedProperty _loopType;
		private SerializedProperty _tweeners;
		private float _simulateDuration;
		private RectTweenSequence _target;
		private bool _isPlaying;
		private bool _isReverse;

		private Dictionary<int, TweenProperty> _tweenProperties;

		private class TweenProperty
		{
			public SerializedProperty Property;
			public SerializedProperty Type;
			public SerializedProperty EaseType;
			public SerializedProperty StartTime;
			public SerializedProperty EndTime;
			public SerializedProperty Join;
			public SerializedProperty Targets;
			public SerializedProperty ControlTarget;
			public SerializedProperty Begin;
			public SerializedProperty End;

			public TweenProperty(SerializedProperty property)
			{
				Property = property;
				Type = property.serializedObject.FindProperty(property.propertyPath + "._type");
				EaseType = property.serializedObject.FindProperty(property.propertyPath + "._easeType");
				StartTime = property.serializedObject.FindProperty(property.propertyPath + "._startTime");
				EndTime = property.serializedObject.FindProperty(property.propertyPath + "._endTime");
				Join = property.serializedObject.FindProperty(property.propertyPath + "._isJoin");
				Targets = property.serializedObject.FindProperty(property.propertyPath + "._targets");
				ControlTarget = property.serializedObject.FindProperty(property.propertyPath + "._controlTarget");
				Begin = property.serializedObject.FindProperty(property.propertyPath + "._begin");
				End = property.serializedObject.FindProperty(property.propertyPath + "._end");	
			}
		}

		private void OnEnable()
		{
			_playOnAwake = serializedObject.FindProperty("_playOnAwake");
			_id = serializedObject.FindProperty("_id");
			_totalTime = serializedObject.FindProperty("_totalTime");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");
			_loopType = serializedObject.FindProperty("_loopType");
			_tweeners = serializedObject.FindProperty("_tweeners");
			_target = (RectTweenSequence) serializedObject.targetObject;
			
			_tweenProperties = new Dictionary<int, TweenProperty>();
			RefreshDic();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			using (new EditorGUILayout.VerticalScope("box"))
			{
				using (new EditorGUI.DisabledScope(_isPlaying))
				{
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						_simulateDuration =
							EditorGUILayout.Slider("Simulate", _simulateDuration, 0f, _target.TotalTime);
						if (check.changed)
							_target.Simulate(_simulateDuration, _isReverse);
					}
				}

				if (_isPlaying && _target.LoopType != RectTweenLoopType.None)
				{
					if (GUILayout.Button("Stop"))
					{
						StopSimulate();
					}
				}
				else
				{
					using (new EditorGUI.DisabledScope(_isPlaying))
					{
						if (GUILayout.Button("Play"))
						{
							_target.ResetSimulate();
							_isPlaying = true;
							_isReverse = false;
							_simulateDuration = 0f;
							EditorApplication.update += UpdateSimulate;
						}
					}
				}
			}
			
			EditorGUILayout.PropertyField(_playOnAwake);
			EditorGUILayout.PropertyField(_id);
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.PropertyField(_totalTime);
				if (check.changed)
				{
					float max = 0f;
					for (var i = 0; i < _tweeners.arraySize; i++)
					{
						if (max < _tweenProperties[i].EndTime.floatValue)
							max = _tweenProperties[i].EndTime.floatValue;
					}

					if (_totalTime.floatValue < max)
						_totalTime.floatValue = max;
				}
			}

			EditorGUILayout.PropertyField(_isIgnoreTimeScale);

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.PropertyField(_loopType);

				using (new EditorGUILayout.VerticalScope())
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Tweeners");
						GUILayout.FlexibleSpace();
						if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						{
							_tweeners.InsertArrayElementAtIndex(_tweeners.arraySize);
							RefreshDic();
						}
					}

					for (var i = 0; i < _tweeners.arraySize; i++)
						using (new EditorGUILayout.VerticalScope())
						{
							EditorGUI.indentLevel++;
							DrawTweener(i);
							EditorGUI.indentLevel--;
						}
				}

				if (check.changed)
				{
					_target.ResetSimulate();
					_simulateDuration = 0f;
					_isPlaying = false;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void RefreshDic()
		{
			_tweenProperties.Clear();
			for (var i = 0; i < _tweeners.arraySize; i++)
				_tweenProperties.Add(i, new TweenProperty(_tweeners.GetArrayElementAtIndex(i)));	
		}

		private void StopSimulate()
		{
			EditorApplication.update -= UpdateSimulate;
			_simulateDuration = 0f;
			_isPlaying = false;
			_isReverse = false;
			_target.ResetSimulate();
		}

		private void UpdateSimulate()
		{
			Repaint();
			
			if (_simulateDuration >= _target.TotalTime)
			{
				_simulateDuration = 0f;
				switch (_target.LoopType)
				{
					case RectTweenLoopType.None:
						StopSimulate();
						break;
					case RectTweenLoopType.PingPong:
						_isReverse = !_isReverse;
						break;
				}
				return;
			}
			
			_target.Simulate(_simulateDuration, _isReverse);
			_simulateDuration += 0.02f;
		}

		private void DrawTweener(int index)
		{
			var tp = _tweenProperties[index];

			using (new EditorGUILayout.HorizontalScope())
			{
				var rect = GUILayoutUtility.GetRect(8f, 24f, 16f, 16f, EditorStyles.label);
				var width = rect.width;

				rect.width = 10f;
				tp.Property.isExpanded = EditorGUI.Foldout(rect, tp.Property.isExpanded, index.ToString());

				rect.x = 40f;
				rect.width = 20f;

				if (index > 0)
				{
					if (GUI.Button(rect, new GUIContent(tp.Join.boolValue ? 
							EditorGUIUtility.TrIconContent("UnityEditor.FindDependencies") : 
							EditorGUIUtility.TrIconContent("LookDevClose")),
						EditorStyles.label))
					{
						tp.Join.boolValue = !tp.Join.boolValue;
					}
				}

				using (new EditorGUI.DisabledScope(tp.Join.boolValue))
				{
					var s = tp.StartTime.floatValue;
					var e = tp.EndTime.floatValue;
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						rect.x = 50f;
						rect.width = width - 45f;
						EditorGUI.MinMaxSlider(rect, ref s, ref e, 0f, _totalTime.floatValue);
						if (check.changed)
						{
							tp.StartTime.floatValue = s;
							tp.EndTime.floatValue = e;
						}
					}
				}

				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
				{
					_tweeners.DeleteArrayElementAtIndex(index);
					RefreshDic();
					return;
				}
			}
			
			if (!tp.Property.isExpanded)
				return;

			// Time
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(tp.Join.boolValue))
				{
					using (new LabelWidthScope(50))
					{
						using (var check = new EditorGUI.ChangeCheckScope())
						{
							EditorGUILayout.PropertyField(tp.StartTime, new GUIContent("Start"));
							EditorGUILayout.PropertyField(tp.EndTime, new GUIContent("End"));
							if (check.changed)
							{
								tp.StartTime.floatValue =
									Mathf.Clamp(tp.StartTime.floatValue, 0, tp.EndTime.floatValue);
								tp.EndTime.floatValue = Mathf.Clamp(tp.EndTime.floatValue, tp.StartTime.floatValue,
									_target.TotalTime);
							}
						}
					}
				}
			}
			
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.PropertyField(tp.Type);
				if (check.changed)
				{
					tp.Begin.vector4Value = Vector4.zero;
					tp.End.vector4Value = Vector4.zero;
					switch (tp.Type.intValue)
					{
						case (int) RectTweenType.Scale:
						case (int) RectTweenType.CanvasGroupAlpha:
							tp.ControlTarget.intValue = (int)ControlTarget.X;
							break;

						case (int) RectTweenType.ScaleAll:
						case (int) RectTweenType.AnchoredPosition:
							tp.ControlTarget.intValue = (int)ControlTarget.XY;
							break;
						
						case (int) RectTweenType.EulerAngle:
							tp.ControlTarget.intValue = (int)ControlTarget.XYZ;
							break;

						case (int) RectTweenType.ImageColor:
							tp.ControlTarget.intValue = (int)ControlTarget.ALL;
							tp.Begin.vector4Value = new Vector4(1, 1, 1, 1);
							tp.End.vector4Value = new Vector4(1, 1, 1, 1);
							break;
					}
				}
			}

			EditorGUILayout.PropertyField(tp.EaseType);

			switch (tp.Type.intValue)
			{
				case (int) RectTweenType.Scale:
				case (int) RectTweenType.CanvasGroupAlpha:
					DrawFloat(tp.Begin, tp.End);
					break;

				case (int) RectTweenType.ScaleAll:
				case (int) RectTweenType.AnchoredPosition:
				case (int) RectTweenType.EulerAngle:
					using (new EditorGUILayout.HorizontalScope())
					{
						using (new LabelWidthScope(30))
						{
							using (var check = new EditorGUI.ChangeCheckScope())
							{
								var x = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.X);
								x = EditorGUILayout.Toggle("x", x);
								var y = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.Y);
								y = EditorGUILayout.Toggle("y", y);
								var z = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.Z);
								if (tp.Type.intValue == (int) RectTweenType.EulerAngle)
								{
									z = EditorGUILayout.Toggle("z", z);	
								}
								if (check.changed)
								{
									tp.ControlTarget.intValue = 0;
									if (x)
										tp.ControlTarget.intValue |= (int) ControlTarget.X;
									if (y)
										tp.ControlTarget.intValue |= (int) ControlTarget.Y;
									if (tp.Type.intValue == (int) RectTweenType.EulerAngle && z)
										tp.ControlTarget.intValue |= (int) ControlTarget.Z;
								}
							}
						}
					}
					DrawCustomVector3(tp.Begin, (ControlTarget)tp.ControlTarget.intValue, tp.Type.intValue == (int) RectTweenType.EulerAngle);
					DrawCustomVector3(tp.End, (ControlTarget)tp.ControlTarget.intValue, tp.Type.intValue == (int) RectTweenType.EulerAngle);
					break;

				case (int) RectTweenType.ImageColor:
					DrawColor(tp.Begin, tp.End);
					break;
			}

			tp.Targets.DrawList();
		}

		private void DrawFloat(SerializedProperty begin, SerializedProperty end)
		{
			var b = begin.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				b.x = EditorGUILayout.FloatField("Begin", begin.vector4Value.x);
				if (check.changed)
					begin.vector4Value = b;
			}

			var e = end.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				b.x = EditorGUILayout.FloatField("End", end.vector4Value.x);
				if (check.changed)
					end.vector4Value = e;
			}
		}

		private void DrawCustomVector3(SerializedProperty property, ControlTarget target, bool requireZ)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new LabelWidthScope(25))
				{
					Vector3 b = property.vector4Value;
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						using (new EditorGUI.DisabledScope(!target.HasFlag(ControlTarget.X)))
							b.x = EditorGUILayout.FloatField("x", b.x);
						using (new EditorGUI.DisabledScope(!target.HasFlag(ControlTarget.Y)))
							b.y = EditorGUILayout.FloatField("y", b.y);
						if (requireZ)
						{
							using (new EditorGUI.DisabledScope(!target.HasFlag(ControlTarget.Z)))
								b.z = EditorGUILayout.FloatField("z", b.z);
						}
						if (check.changed)
						{
							property.vector4Value = b;
						}
					}
				}
			}
		}

		private void DrawColor(SerializedProperty begin, SerializedProperty end)
		{
			Color b = begin.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				b = EditorGUILayout.ColorField("Begin", b);
				if (check.changed)
					begin.vector4Value = b;
			}

			Color e = end.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				e = EditorGUILayout.ColorField("End", e);
				if (check.changed)
					end.vector4Value = e;
			}
		}

		public class LabelWidthScope : GUI.Scope
		{
			private readonly float _cacheWidth;

			public LabelWidthScope(float width)
			{
				_cacheWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = width;
			}
			protected override void CloseScope()
			{
				EditorGUIUtility.labelWidth = _cacheWidth;
			}
		}
	}
}