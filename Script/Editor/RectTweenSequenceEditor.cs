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
				{
					_target.Simulate(_simulateDuration);
				}

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

				for (int i = 0; i < _tweeners.arraySize; i++)
				{
					using (new EditorGUILayout.VerticalScope("box"))
						DrawTweener(i, _tweeners.GetArrayElementAtIndex(i));
				}
			}
			
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				_totalDuration = ((RectTweenSequence) serializedObject.targetObject).TotalDuration;
				_target.ResetSimulate();
				_simulateDuration = 0f;
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
			
			var beginVector3Property = property.serializedObject.FindProperty(property.propertyPath + "._beginVector3");
			var endVector3Property = property.serializedObject.FindProperty(property.propertyPath + "._endVector3");
			
			var beginColorProperty = property.serializedObject.FindProperty(property.propertyPath + "._beginColor");
			var endColorProperty = property.serializedObject.FindProperty(property.propertyPath + "._endColor");
			
			var beginFloatProperty = property.serializedObject.FindProperty(property.propertyPath + "._beginFloat");
			var endFloatProperty = property.serializedObject.FindProperty(property.propertyPath + "._endFloat");
			
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
				
				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
				{
					_tweeners.DeleteArrayElementAtIndex(index);
					return;
				}
			}
			
			if (index > 0)
			{
				joinProperty.boolValue = EditorGUILayout.ToggleLeft("isJoin", joinProperty.boolValue);
			}

			if (joinProperty.boolValue)
			{
				EditorGUI.indentLevel++;
			}

			EditorGUILayout.PropertyField(typeProperty);
			EditorGUILayout.PropertyField(easeTypeProperty);
			EditorGUILayout.PropertyField(durationProperty);
			EditorGUILayout.PropertyField(delayProperty);
			
			targetsProperty.DrawList();
			
			switch (typeProperty.intValue)
			{
				case (int)RectTweenType.Scale:
				case (int)RectTweenType.CanvasGroupAlpha:
					EditorGUILayout.PropertyField(beginFloatProperty);
					EditorGUILayout.PropertyField(endFloatProperty);
					break;
					
				case (int)RectTweenType.AnchoredPosition:
				case (int)RectTweenType.Rotation:
					EditorGUILayout.PropertyField(beginVector3Property);
					EditorGUILayout.PropertyField(endVector3Property);
					break;

				case (int)RectTweenType.ImageColor:
					EditorGUILayout.PropertyField(beginColorProperty);
					EditorGUILayout.PropertyField(endColorProperty);
					break;
			}
			
			if (joinProperty.boolValue)
			{
				EditorGUI.indentLevel--;
			}
		}
	}
}