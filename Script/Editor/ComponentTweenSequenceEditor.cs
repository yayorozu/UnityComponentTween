using UnityEditor;
using UnityEngine;

namespace Yorozu.ComponentTween.Editor
{
	[CustomEditor(typeof(ComponentTweenSequence))]
	public class ComponentTweenSequenceEditor : UnityEditor.Editor
	{
		private SerializedProperty _playOnAwake;
		private SerializedProperty _id;
		private SerializedProperty _totalTime;
		private SerializedProperty _isIgnoreTimeScale;
		private SerializedProperty _loopType;
		private SerializedProperty _targets;
		private SerializedProperty _tweenData;
		private SerializedProperty _params;
		
		private SerializedProperty _cacheParams;
		private SerializedProperty _cacheTweenerRoot;
		private SerializedProperty _currentTargetObjects;
		
		private float _simulateDuration;
		private ComponentTweenSequence _target;
		private bool _isPlaying;
		private bool _isReverse;

		private TweenProperty[] _tweenProperties;
		private UnityEditor.Editor _cacheTweenObjectEditor;
		private int _editIndex;
		
		private class TweenProperty
		{
			public SerializedProperty Type;
			public SerializedProperty EaseType;
			public SerializedProperty StartTime;
			public SerializedProperty EndTime;
			public SerializedProperty ControlTarget;
			public SerializedProperty Begin;
			public SerializedProperty End;

			public TweenProperty(SerializedProperty property)
			{
				Type = property.serializedObject.FindProperty(property.propertyPath + ".Type");
				EaseType = property.serializedObject.FindProperty(property.propertyPath + ".EaseType");
				StartTime = property.serializedObject.FindProperty(property.propertyPath + ".StartTime");
				EndTime = property.serializedObject.FindProperty(property.propertyPath + ".EndTime");
				
				ControlTarget = property.serializedObject.FindProperty(property.propertyPath + ".ControlTarget");
				Begin = property.serializedObject.FindProperty(property.propertyPath + ".Begin");
				End = property.serializedObject.FindProperty(property.propertyPath + ".End");
			}
		}

		private void OnEnable()
		{
			_target = (ComponentTweenSequence) serializedObject.targetObject;
			
			_playOnAwake = serializedObject.FindProperty("_playOnAwake");
			_id = serializedObject.FindProperty("_id");
			_totalTime = serializedObject.FindProperty("_totalTime");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");
			
			_loopType = serializedObject.FindProperty("_loopType");
			_params = serializedObject.FindProperty("_params");
			_targets = serializedObject.FindProperty("_targets");
			_tweenData = serializedObject.FindProperty("_tweenData");
			
			// 初期化して保存する
			serializedObject.Update();
			RefreshDic();
			serializedObject.ApplyModifiedProperties();
			
			_editIndex = -1;
			if (_cacheParams.arraySize > 0)
			{
				_editIndex = 0;
				_currentTargetObjects = _targets.GetArrayElementAtIndex(_editIndex).FindPropertyRelative("TargetObjects");
			}
		}

		private void OnDisable()
		{
			StopSimulate();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (_cacheTweenObjectEditor != null)
				_cacheTweenObjectEditor.serializedObject.Update();

			using (new EditorGUI.DisabledScope(Application.isPlaying))
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					using (new EditorGUI.DisabledScope(_isPlaying))
					{
						using (var check = new EditorGUI.ChangeCheckScope())
						{
							_simulateDuration = EditorGUILayout.Slider("Simulate", _simulateDuration, 0f, _target.TotalTime);
							if (check.changed)
								_target.EditorSimulate(_simulateDuration, _isReverse);
						}
					}

					if (_isPlaying)
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
								if (_isPlaying)
									return;
							
								_target.EditorSimulatePrepare();
								_isPlaying = true;
								_isReverse = false;
								_simulateDuration = 0f;
								EditorApplication.update += UpdateSimulate;
							}
						}
					}
				}
			}

			EditorGUILayout.Space();
			
			using (new EditorGUI.DisabledScope(_isPlaying))
			{
				EditorGUILayout.PropertyField(_playOnAwake);
				EditorGUILayout.PropertyField(_id);
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.PropertyField(_totalTime);
					if (check.changed)
					{
						var max = 0f;
						foreach (var tp in _tweenProperties)
						{
							if (max < tp.EndTime.floatValue)
								max = tp.EndTime.floatValue;
						}

						if (_totalTime.floatValue < max)
							_totalTime.floatValue = max;
					}
				}

				EditorGUILayout.PropertyField(_isIgnoreTimeScale);
			
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.PropertyField(_loopType);
					using (var check2 = new EditorGUI.ChangeCheckScope())
					{
						EditorGUILayout.PropertyField(_tweenData);
						if (check2.changed)
						{
							RefreshDic();
						}
					}

					using (new EditorGUILayout.VerticalScope())
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField("Tweeners", EditorStyles.boldLabel);
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
							{
								_targets.InsertArrayElementAtIndex(_cacheParams.arraySize);
								_cacheParams.InsertArrayElementAtIndex(_cacheParams.arraySize);
								RefreshDic();
							}
						}

						for (var i = 0; i < _tweenProperties.Length; i++)
							using (new EditorGUILayout.VerticalScope())
							{
								EditorGUI.indentLevel++;
								DrawTweener(i);
								EditorGUI.indentLevel--;
							}
					}
				
					if (check.changed)
					{
						_simulateDuration = 0f;
						_isPlaying = false;
					}
				}
			
				DrawParameter();
			}

			serializedObject.ApplyModifiedProperties();
			if (_cacheTweenObjectEditor != null)
				_cacheTweenObjectEditor.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Propertyのキャッシュをリフレッシュ
		/// </summary>
		private void RefreshDic()
		{
			_cacheParams = _params;
			
			if (_tweenData.objectReferenceValue != null)
			{
				if (_cacheTweenObjectEditor == null)
					_cacheTweenObjectEditor = CreateEditor(_tweenData.objectReferenceValue);
				
				var param = _cacheTweenObjectEditor.serializedObject.FindProperty("Params");
				if (param != null)
					_cacheParams = param;
				else
				{
					_tweenData.objectReferenceValue = null;
					_cacheTweenObjectEditor = null;
				}
			}
			else
			{
				_cacheTweenObjectEditor = null;
			}

			// TargetObjectsのサイズが一致してないなら一致させる
			if (_cacheParams.arraySize > _targets.arraySize)
				for (var index = _targets.arraySize; index < _cacheParams.arraySize; index++)
					_targets.InsertArrayElementAtIndex(index);
			else if (_cacheParams.arraySize < _targets.arraySize)
				for (var index = _targets.arraySize - 1; index >= _cacheParams.arraySize; index--)
					_targets.DeleteArrayElementAtIndex(index);
			
			_tweenProperties = new TweenProperty[_cacheParams.arraySize];
			for (var i = 0; i < _cacheParams.arraySize; i++)
				_tweenProperties[i] = new TweenProperty(_cacheParams.GetArrayElementAtIndex(i));
		}

		private void StopSimulate()
		{
			if (!_isPlaying)
				return;
			
			EditorApplication.update -= UpdateSimulate;
			_simulateDuration = 0f;
			_isPlaying = false;
			_isReverse = false;
			_target.EditorUndoParam();
		}

		private void UpdateSimulate()
		{
			if (!_isPlaying)
				return;
			
			Repaint();

			if (!(_simulateDuration >= _target.TotalTime))
			{
				_target.EditorSimulate(_simulateDuration, _isReverse);
				_simulateDuration += 0.02f;
				return;
			}

			_simulateDuration = 0f;
			switch (_target.LoopType)
			{
				case LoopType.None:
					StopSimulate();

					break;
				case LoopType.PingPong:
					_isReverse = !_isReverse;
					_target.EditorReset();
					break;
				case LoopType.Loop:
					_target.EditorReset();
					break;
			}
		}

		private void DrawTweener(int index)
		{
			var tp = _tweenProperties[index];
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button(
					index.ToString(), 
					index == _editIndex ? EditorStyles.boldLabel : EditorStyles.label, 
					GUILayout.Width(10))
				)
				{
					if (_editIndex != index)
					{
						_editIndex = index;
						_currentTargetObjects = _targets.GetArrayElementAtIndex(_editIndex).FindPropertyRelative("TargetObjects");
					}
					else
					{
						_editIndex = -1;
						_currentTargetObjects = null;
					}
					GUI.FocusControl("");
				}

				var s = tp.StartTime.floatValue;
				var e = tp.EndTime.floatValue;
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.MinMaxSlider(ref s, ref e, 0f, _totalTime.floatValue);
					if (check.changed)
					{
						GUI.FocusControl("");
						tp.StartTime.floatValue = (int) (s * 100) / 100f;
						tp.EndTime.floatValue = (int) (e * 100) / 100f;
					}
				}

				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
				{
					_cacheParams.DeleteArrayElementAtIndex(index);
					_targets.DeleteArrayElementAtIndex(index);
					RefreshDic();
				}
			}
		}
		
		/// <summary>
		/// パラメータ
		/// </summary>
		private void DrawParameter()
		{
			if (_tweenProperties == null || _tweenProperties.Length == 0 || _editIndex < 0)
				return;

			if (_tweenProperties.Length <= _editIndex)
				_editIndex = 0;

			var tp = _tweenProperties[_editIndex];
			
			using (new EditorGUILayout.VerticalScope("box"))
			{
				// Time
				using (new EditorGUILayout.HorizontalScope())
				{
					var _cacheValue = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 50;
				
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						tp.StartTime.floatValue = EditorGUILayout.FloatField("Start", tp.StartTime.floatValue);
						tp.EndTime.floatValue = EditorGUILayout.FloatField("End", tp.EndTime.floatValue);
						if (check.changed)
						{
							tp.StartTime.floatValue = Mathf.Clamp(tp.StartTime.floatValue, 0, tp.EndTime.floatValue);
							tp.EndTime.floatValue = Mathf.Clamp(tp.EndTime.floatValue, tp.StartTime.floatValue, _target.TotalTime);
						}
					}

					EditorGUIUtility.labelWidth = _cacheValue;
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
							case (int) TweenType.Scale:
							case (int) TweenType.CanvasGroupAlpha:
							case (int) TweenType.ChangeActive:
								tp.ControlTarget.intValue = (int)ControlTarget.X;
								break;

							case (int) TweenType.ScaleFlags:
							case (int) TweenType.AnchoredPosition:
								tp.ControlTarget.intValue = (int)ControlTarget.XY;
								break;
						
							case (int) TweenType.EulerAngle:
								tp.ControlTarget.intValue = (int)ControlTarget.XYZ;
								break;

							case (int) TweenType.ImageColor:
								tp.ControlTarget.intValue = (int)ControlTarget.ALL;
								tp.Begin.vector4Value = new Vector4(1, 1, 1, 1);
								tp.End.vector4Value = new Vector4(1, 1, 1, 1);
								break;
						}
					}
				}

				EditorGUILayout.PropertyField(tp.EaseType);

				switch ((TweenType)tp.Type.intValue)
				{
					case TweenType.Scale:
					case TweenType.CanvasGroupAlpha:
						DrawFloat(tp.Begin, "Begin");
						DrawFloat(tp.End, "End");
						break;

					case TweenType.AnchoredPosition:
						DrawCustomVector3s(tp, false);
						break;
					
					case TweenType.ScaleFlags:
					case TweenType.EulerAngle:
					case TweenType.Position:
						DrawCustomVector3s(tp, true);
						break;

					case TweenType.ImageColor:
						DrawColor(tp.Begin, "Begin");
						DrawColor(tp.Begin, "End");
						break;
					
					case TweenType.ChangeActive:
						DrawBool(tp.Begin, "Begin");
						DrawBool(tp.Begin, "End");
						break;
				}

				if (_currentTargetObjects != null)
				{
					// DrawObjectList
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Target Objects", EditorStyles.boldLabel);
						GUILayout.FlexibleSpace();
						if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						{
							_currentTargetObjects.InsertArrayElementAtIndex(_currentTargetObjects.arraySize);
						}
					}

					for (var i = 0; i < _currentTargetObjects.arraySize; i++)
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.PropertyField(_currentTargetObjects.GetArrayElementAtIndex(i));

							if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton",
								GUILayout.Width(16)))
							{
								_currentTargetObjects.DeleteArrayElementAtIndex(i);
							}
						}
					}
				}
			}
		}

		private void DrawCustomVector3s(TweenProperty tp, bool requireZ)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				var _cacheValue = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 30;
							
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var x = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.X);
					x = EditorGUILayout.Toggle("x", x);
					var y = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.Y);
					y = EditorGUILayout.Toggle("y", y);
					var z = ((ControlTarget) tp.ControlTarget.intValue).HasFlags(ControlTarget.Z);
					if (requireZ)
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
						if (requireZ && z)
							tp.ControlTarget.intValue |= (int) ControlTarget.Z;
					}
				}
							
				EditorGUIUtility.labelWidth = _cacheValue;
			}
			DrawCustomVector3(tp.Begin, (ControlTarget)tp.ControlTarget.intValue, requireZ);
			DrawCustomVector3(tp.End, (ControlTarget)tp.ControlTarget.intValue, requireZ);
		}

		private void DrawCustomVector3(SerializedProperty property, ControlTarget target, bool requireZ)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				var _cacheValue = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 25;

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

				EditorGUIUtility.labelWidth = _cacheValue;
			}
		}
		
		private void DrawFloat(SerializedProperty property, string label)
		{
			var v = property.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				v.x = EditorGUILayout.FloatField(label, property.vector4Value.x);
				if (check.changed)
					property.vector4Value = v;
			}
		}
		
		private void DrawColor(SerializedProperty property, string label)
		{
			Color c = property.vector4Value;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				c = EditorGUILayout.ColorField(label, c);
				if (check.changed)
					property.vector4Value = c;
			}	
		}

		private void DrawBool(SerializedProperty property, string label)
		{
			var b = property.vector4Value.x == 1f;
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				b = EditorGUILayout.Toggle(label, b);
				if (check.changed)
				{
					var value = property.vector4Value;
					value.x = b ? 1 : 0;
					property.vector4Value = value;
				}
			}
		}
		
	}
}