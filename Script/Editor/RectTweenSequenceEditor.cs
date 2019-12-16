using System;
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
		private float _totalDuration;
		private float _simulateDuration;
		private RectTweenSequence _target;
		private float _simulatePlayDuration;
		private bool _isPlaying;
		private bool _isReverse;

		private void OnEnable()
		{
			_playOnAwake = serializedObject.FindProperty("_playOnAwake");
			_id = serializedObject.FindProperty("_id");
			_totalTime = serializedObject.FindProperty("_totalTime");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");
			_loopType = serializedObject.FindProperty("_loopType");
			_tweeners = serializedObject.FindProperty("_tweeners");
			_target = (RectTweenSequence) serializedObject.targetObject;
			_totalDuration = _target.TotalTime;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUI.BeginChangeCheck();
				_simulateDuration = EditorGUILayout.Slider("Simulate", _simulateDuration, 0f, _totalDuration);
				if (EditorGUI.EndChangeCheck())
					_target.Simulate(_simulateDuration, _isReverse);

				if (_isPlaying && _target.LoopType != RectTweenLoopType.None)
				{
					if (GUILayout.Button("Stop"))
					{
						EditorApplication.update -= UpdateSimulate;
						_isPlaying = false;
					}
				}
				else
				{
					if (GUILayout.Button("Play"))
					{
						_target.ResetSimulate();
						_isPlaying = true;
						_isReverse = false;
						_simulatePlayDuration = 0f;
						EditorApplication.update += UpdateSimulate;
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_playOnAwake);
			EditorGUILayout.PropertyField(_id);
			EditorGUILayout.PropertyField(_totalTime);
			EditorGUILayout.PropertyField(_isIgnoreTimeScale);
			EditorGUILayout.PropertyField(_loopType);

			using (new EditorGUILayout.VerticalScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Tweeners");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						_tweeners.InsertArrayElementAtIndex(_tweeners.arraySize);
				}

				for (var i = 0; i < _tweeners.arraySize; i++)
					using (new EditorGUILayout.VerticalScope())
					{
						EditorGUI.indentLevel++;
						DrawTweener(i, _tweeners.GetArrayElementAtIndex(i));
						EditorGUI.indentLevel--;
					}
			}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				_totalDuration = ((RectTweenSequence) serializedObject.targetObject).TotalTime;
				_target.ResetSimulate();
				_simulateDuration = 0f;
				_isPlaying = false;
			}
		}

		private void UpdateSimulate()
		{
			if (_simulatePlayDuration > _totalDuration)
			{
				_simulatePlayDuration = 0f;
				switch (_target.LoopType)
				{
					case RectTweenLoopType.None:
						EditorApplication.update -= UpdateSimulate;

						break;
					case RectTweenLoopType.PingPong:
						_isReverse = !_isReverse;

						break;
				}

				return;
			}
			
			_target.Simulate(_simulatePlayDuration, _isReverse);

			_simulatePlayDuration += 0.02f;
		}

		private void DrawTweener(int index, SerializedProperty property)
		{
			var typeProperty = property.serializedObject.FindProperty(property.propertyPath + "._type");
			var easeTypeProperty = property.serializedObject.FindProperty(property.propertyPath + "._easeType");
			var startTimeProperty = property.serializedObject.FindProperty(property.propertyPath + "._startTime");
			var endTimeProperty = property.serializedObject.FindProperty(property.propertyPath + "._endTime");
			var joinProperty = property.serializedObject.FindProperty(property.propertyPath + "._isJoin");
			var targetsProperty = property.serializedObject.FindProperty(property.propertyPath + "._targets");
			var controlTargetProperty = property.serializedObject.FindProperty(property.propertyPath + "._controlTarget");

			var beginProperty = property.serializedObject.FindProperty(property.propertyPath + "._begin");
			var endProperty = property.serializedObject.FindProperty(property.propertyPath + "._end");

			using (new EditorGUILayout.HorizontalScope())
			{
				property.isExpanded = EditorGUI.Foldout(
					GUILayoutUtility.GetRect(8f, 8f, 16f, 16f, EditorStyles.label), 
					property.isExpanded,
					index.ToString());
				
				var s = startTimeProperty.floatValue;
				var e = endTimeProperty.floatValue;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.MinMaxSlider(ref s, ref e, 0f, _totalTime.floatValue);
				if (EditorGUI.EndChangeCheck())
				{
					startTimeProperty.floatValue = s;
					endTimeProperty.floatValue = e;
				}

				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton",
					GUILayout.Width(16)))
				{
					_tweeners.DeleteArrayElementAtIndex(index);

					return;
				}
			}
			
			if (!property.isExpanded)
				return;

			// Time
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField($"s:{startTimeProperty.floatValue:N2} e:{endTimeProperty.floatValue:N2}");
			}

			if (index > 0)
				joinProperty.boolValue = EditorGUILayout.ToggleLeft("isJoin", joinProperty.boolValue);

			if (joinProperty.boolValue)
				EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(typeProperty);
			if (EditorGUI.EndChangeCheck())
			{
				beginProperty.vector4Value = Vector4.zero;
				endProperty.vector4Value = Vector4.zero;
				switch (typeProperty.intValue)
				{
					case (int) RectTweenType.Scale:
					case (int) RectTweenType.CanvasGroupAlpha:
						controlTargetProperty.intValue = (int)ControlTarget.X;
						break;

					case (int) RectTweenType.ScaleAll:
					case (int) RectTweenType.AnchoredPosition:
					case (int) RectTweenType.Rotation:
						controlTargetProperty.intValue = (int)ControlTarget.XYZ;
						break;

					case (int) RectTweenType.ImageColor:
						controlTargetProperty.intValue = (int)ControlTarget.ALL;
						beginProperty.vector4Value = new Vector4(1, 1, 1, 1);
						endProperty.vector4Value = new Vector4(1, 1, 1, 1);
						break;
				}

			}

			EditorGUILayout.PropertyField(easeTypeProperty);

			switch (typeProperty.intValue)
			{
				case (int) RectTweenType.Scale:
				case (int) RectTweenType.CanvasGroupAlpha:
					DrawFloat(beginProperty, endProperty);
					break;

				case (int) RectTweenType.ScaleAll:
				case (int) RectTweenType.AnchoredPosition:
				case (int) RectTweenType.Rotation:
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUI.BeginChangeCheck();
						var x = ((ControlTarget) controlTargetProperty.intValue).HasFlags(ControlTarget.X);
						x = EditorGUILayout.ToggleLeft("x", x, GUILayout.Width(60));
						var y = ((ControlTarget) controlTargetProperty.intValue).HasFlags(ControlTarget.Y);
						y = EditorGUILayout.ToggleLeft("y", y, GUILayout.Width(60));
						var z = ((ControlTarget) controlTargetProperty.intValue).HasFlags(ControlTarget.Z);
						z = EditorGUILayout.ToggleLeft("z", z, GUILayout.Width(60));
						if (EditorGUI.EndChangeCheck())
						{
							controlTargetProperty.intValue = 0;
							if (x)
								controlTargetProperty.intValue |= (int) ControlTarget.X;
							if (y)
								controlTargetProperty.intValue |= (int) ControlTarget.Y;
							if (z)
								controlTargetProperty.intValue |= (int) ControlTarget.Z;
						}
					}
					DrawCustomVector3(beginProperty, (ControlTarget)controlTargetProperty.intValue);
					DrawCustomVector3(endProperty, (ControlTarget)controlTargetProperty.intValue);
					break;

				case (int) RectTweenType.ImageColor:
					DrawColor(beginProperty, endProperty);
					break;
			}

			targetsProperty.DrawList();

			if (joinProperty.boolValue)
				EditorGUI.indentLevel--;
		}

		private void DrawFloat(SerializedProperty begin, SerializedProperty end)
		{
			var b = begin.vector4Value;
			EditorGUI.BeginChangeCheck();
			b.x = EditorGUILayout.FloatField("Begin", begin.vector4Value.x);
			if (EditorGUI.EndChangeCheck())
				begin.vector4Value = b;

			var e = end.vector4Value;
			EditorGUI.BeginChangeCheck();
			b.x = EditorGUILayout.FloatField("End", end.vector4Value.x);
			if (EditorGUI.EndChangeCheck())
				end.vector4Value = e;
		}

		private void DrawCustomVector3(SerializedProperty property, ControlTarget target)
		{
			var rect = EditorGUILayout.GetControlRect(
				true,
				EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, GUIContent.none), 
				EditorStyles.numberField
			);
			using (new EditorGUILayout.HorizontalScope())
			{
				Vector3 b = property.vector4Value;
				EditorGUI.BeginChangeCheck();
				EditorGUI.MultiFloatField(rect, GUIContent.none, new[]
				{
					new GUIContent("x"), 
					new GUIContent("y"), 
					new GUIContent("z"), 
				}, new[]
				{
					b.x, b.y, b.z
				});
				if (EditorGUI.EndChangeCheck())
					property.vector4Value = b;
			}
		}

		private void DrawColor(SerializedProperty begin, SerializedProperty end)
		{
			Color b = begin.vector4Value;
			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.ColorField("Begin", b);
			if (EditorGUI.EndChangeCheck())
				begin.vector4Value = b;

			Color e = end.vector4Value;
			EditorGUI.BeginChangeCheck();
			e = EditorGUILayout.ColorField("End", e);
			if (EditorGUI.EndChangeCheck())
				end.vector4Value = e;
		}
	}
}