#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[CustomEditor(typeof(ComponentTweenSequence))]
	public class ComponentTweenSequenceEditor : Editor
	{
		private SerializedProperty _playOnAwake;
		private SerializedProperty _id;
		private SerializedProperty _totalTime;
		private SerializedProperty _isIgnoreTimeScale;
		private SerializedProperty _loopType;
		private SerializedProperty _targets;
		private SerializedProperty _tweenData;
		private SerializedProperty _params;


		private float _simulateDuration;
		private ComponentTweenSequence _target;
		private bool _isPlaying;
		private bool _isReverse;

		private int _editIndex;

		private Type[] _moduleTypes;

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

			_editIndex = -1;
			if (_params.arraySize > 0)
			{
				_editIndex = 0;
			}

			_moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsSubclassOf(typeof(ModuleAbstract)) && !t.IsAbstract)
				.ToArray();
		}

		private void OnDisable()
		{
			StopSimulate();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

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
						foreach (var p in _target.Params)
						{
							if (max < p.End)
								max = p.End;
						}

						if (_totalTime.floatValue < max)
							_totalTime.floatValue = max;
					}
				}

				EditorGUILayout.PropertyField(_isIgnoreTimeScale);

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.PropertyField(_loopType);
					EditorGUILayout.PropertyField(_tweenData);

					using (new EditorGUILayout.VerticalScope())
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField("Tweeners", EditorStyles.boldLabel);
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
							{
								_targets.InsertArrayElementAtIndex(_params.arraySize);
								_params.InsertArrayElementAtIndex(_params.arraySize);
								serializedObject.ApplyModifiedProperties();
								GUIUtility.ExitGUI();
							}
						}

						for (var i = 0; i < _target.Params.Length; i++)
						{
							using (new EditorGUILayout.VerticalScope())
							{
								EditorGUI.indentLevel++;
								DrawTweener(i);
								EditorGUI.indentLevel--;
							}
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
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button(
					index.ToString(),
					index == _editIndex ? EditorStyles.boldLabel : EditorStyles.label,
					GUILayout.Width(10))
				)
				{
					if (_editIndex != index)
						_editIndex = index;
					else
						_editIndex = -1;

					GUI.FocusControl("");
				}

				_target.Params[index].OnGUI(_totalTime.floatValue);
				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
				{
					_params.DeleteArrayElementAtIndex(index);
					_targets.DeleteArrayElementAtIndex(index);
					serializedObject.ApplyModifiedProperties();
					GUIUtility.ExitGUI();
				}
			}
		}

		/// <summary>
		/// パラメータ
		/// </summary>
		private void DrawParameter()
		{
			if (_target.Params == null || _target.Params.Length == 0 || _editIndex < 0)
				return;

			if (_target.Params.Length <= _editIndex)
				_editIndex = 0;

			_target.Params[_editIndex].OnGUIDetail(_totalTime.floatValue, _moduleTypes, _target.Targets[_editIndex]);
		}
	}
}

#endif
