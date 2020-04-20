using Yorozu.UniEditor;
using UnityEditor;
using UnityEngine;

namespace Yorozu.RectTween.Editor
{
	[CustomEditor(typeof(RectTweenSequence))]
	public class RectTweenSequenceEditor : UnityEditor.Editor
	{
		private SerializedProperty _playOnAwake;
		private SerializedProperty _id;
		private SerializedProperty _totalTime;
		private SerializedProperty _isIgnoreTimeScale;
		private SerializedProperty _loopType;
		private SerializedProperty _targets;
		private SerializedProperty _tweenObject;
		private SerializedProperty _params;
		
		private SerializedProperty _cacheParams;
		private SerializedProperty _cacheTweenerRoot;
		private SerializedProperty _currentTargetObjects;
		
		private float _simulateDuration;
		private RectTweenSequence _target;
		private bool _isPlaying;
		private bool _isReverse;

		private TweenProperty[] _tweenProperties;
		private UnityEditor.Editor _cacheTweenObjectEditor;
		private int _editIndex;
		
		private class TweenProperty
		{
			public SerializedProperty Property;
			
			public SerializedProperty Type;
			public SerializedProperty EaseType;
			public SerializedProperty StartTime;
			public SerializedProperty EndTime;
			public SerializedProperty ControlTarget;
			public SerializedProperty Begin;
			public SerializedProperty End;

			public TweenProperty(SerializedProperty property)
			{
				Property = property;
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
			_target = (RectTweenSequence) serializedObject.targetObject;
			
			_playOnAwake = serializedObject.FindProperty("_playOnAwake");
			_id = serializedObject.FindProperty("_id");
			_totalTime = serializedObject.FindProperty("_totalTime");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");
			
			_loopType = serializedObject.FindProperty("_loopType");
			_params = serializedObject.FindProperty("_params");
			_targets = serializedObject.FindProperty("_targets");
			_tweenObject = serializedObject.FindProperty("_tweenObject");
			
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
						EditorGUILayout.PropertyField(_tweenObject);
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
			
			if (_tweenObject.objectReferenceValue != null)
			{
				if (_cacheTweenObjectEditor == null)
					_cacheTweenObjectEditor = CreateEditor(_tweenObject.objectReferenceValue);
				
				var param = _cacheTweenObjectEditor.serializedObject.FindProperty("Params");
				if (param != null)
					_cacheParams = param;
				else
				{
					_tweenObject.objectReferenceValue = null;
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
				case RectTweenLoopType.None:
					StopSimulate();

					break;
				case RectTweenLoopType.PingPong:
					_isReverse = !_isReverse;
					_target.EditorReset();
					break;
				case RectTweenLoopType.Loop:
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
						tp.StartTime.floatValue = s;
						tp.EndTime.floatValue = e;
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
						tp.StartTime.floatValue = EditorGUILayout.DelayedFloatField("Start", tp.StartTime.floatValue);
						tp.EndTime.floatValue = EditorGUILayout.DelayedFloatField("End", tp.EndTime.floatValue);
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
							case (int) RectTweenType.Scale:
							case (int) RectTweenType.CanvasGroupAlpha:
							case (int) RectTweenType.ChangeActive:
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
							var _cacheValue = EditorGUIUtility.labelWidth;
							EditorGUIUtility.labelWidth = 30;
							
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
							
							EditorGUIUtility.labelWidth = _cacheValue;
						}
						DrawCustomVector3(tp.Begin, (ControlTarget)tp.ControlTarget.intValue, tp.Type.intValue == (int) RectTweenType.EulerAngle);
						DrawCustomVector3(tp.End, (ControlTarget)tp.ControlTarget.intValue, tp.Type.intValue == (int) RectTweenType.EulerAngle);
						break;

					case (int) RectTweenType.ImageColor:
						DrawColor(tp.Begin, tp.End);
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
				e.x = EditorGUILayout.FloatField("End", end.vector4Value.x);
				if (check.changed)
					end.vector4Value = e;
			}
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
	}
}