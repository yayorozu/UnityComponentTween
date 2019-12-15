using System;
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
			_id = serializedObject.FindProperty("id");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");
			_loopType = serializedObject.FindProperty("_loopType");
			_tweeners = serializedObject.FindProperty("_tweeners");
			_target = (RectTweenSequence) serializedObject.targetObject;
			_totalDuration = _target.TotalDuration;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUI.BeginChangeCheck();
				_simulateDuration = EditorGUILayout.Slider("Simulate", _simulateDuration, 0f, _totalDuration);
				if (EditorGUI.EndChangeCheck())
					_target.Simulate(_simulateDuration);

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
			EditorGUILayout.PropertyField(_isIgnoreTimeScale);
			EditorGUILayout.PropertyField(_loopType);

			using (new EditorGUILayout.VerticalScope("box"))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Tweeners");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						_tweeners.InsertArrayElementAtIndex(_tweeners.arraySize);
				}

				for (var i = 0; i < _tweeners.arraySize; i++)
					using (new EditorGUILayout.VerticalScope("box"))
					{
						DrawTweener(i, _tweeners.GetArrayElementAtIndex(i));
					}
			}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				_totalDuration = ((RectTweenSequence) serializedObject.targetObject).TotalDuration;
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

			if (_isReverse)
				_target.SimulateReverse(_simulatePlayDuration, _totalDuration);
			else
				_target.Simulate(_simulatePlayDuration);

			_simulatePlayDuration += 0.02f;
		}

		private void DrawTweener(int index, SerializedProperty property)
		{
			var typeProperty = property.serializedObject.FindProperty(property.propertyPath + "._type");
			var easeTypeProperty = property.serializedObject.FindProperty(property.propertyPath + "._easeType");
			var durationProperty = property.serializedObject.FindProperty(property.propertyPath + "._duration");
			var delayProperty = property.serializedObject.FindProperty(property.propertyPath + "._delay");
			var joinProperty = property.serializedObject.FindProperty(property.propertyPath + "._isJoin");
			var targetsProperty = property.serializedObject.FindProperty(property.propertyPath + "._targets");
			var controlTargetProperty = property.serializedObject.FindProperty(property.propertyPath + "._controlTarget");

			var beginProperty = property.serializedObject.FindProperty(property.propertyPath + "._begin");
			var endProperty = property.serializedObject.FindProperty(property.propertyPath + "._end");

			using (new EditorGUILayout.HorizontalScope("box"))
			{
				GUI.enabled = index > 0;
				if (GUILayout.Button("↑", EditorStyles.label, GUILayout.Width(16)))
					_tweeners.MoveArrayElement(index, index - 1);

				GUI.enabled = index < _tweeners.arraySize - 1;
				if (GUILayout.Button("↓", EditorStyles.label, GUILayout.Width(16)))
					_tweeners.MoveArrayElement(index, index + 1);
				GUI.enabled = true;

				EditorGUILayout.LabelField(index.ToString(), EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton",
					GUILayout.Width(16)))
				{
					_tweeners.DeleteArrayElementAtIndex(index);

					return;
				}
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
			EditorGUILayout.PropertyField(durationProperty);
			EditorGUILayout.PropertyField(delayProperty);

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
						x = EditorGUILayout.Toggle("x", x);
						var y = ((ControlTarget) controlTargetProperty.intValue).HasFlags(ControlTarget.Y);
						y = EditorGUILayout.Toggle("y", y);
						var z = ((ControlTarget) controlTargetProperty.intValue).HasFlags(ControlTarget.Z);
						z = EditorGUILayout.Toggle("z", z);
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
					DrawVector3(beginProperty, endProperty);
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

		private void DrawVector3(SerializedProperty begin, SerializedProperty end)
		{
			Vector3 b = begin.vector4Value;
			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Vector3Field("Begin", b);
			if (EditorGUI.EndChangeCheck())
				begin.vector4Value = b;

			Vector3 e = end.vector4Value;
			EditorGUI.BeginChangeCheck();
			e = EditorGUILayout.Vector3Field("End", e);
			if (EditorGUI.EndChangeCheck())
				end.vector4Value = e;
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