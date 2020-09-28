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

		private ComponentTweenSequence _target;
		[SerializeField]
		private ComponentTweenSimulate _simulate;

		private int _editIndex;

		private Type[] _moduleTypes;

		private void OnEnable()
		{
			_target = (ComponentTweenSequence) serializedObject.targetObject;
			_simulate = new ComponentTweenSimulate(_target, this);

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
			_simulate?.StopSimulate();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			_simulate.OnGUI();

			EditorGUILayout.Space();

			using (new EditorGUI.DisabledScope(_simulate.IsPlaying))
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
						_simulate.Reset();
					}
				}

				DrawParameter();
			}

			serializedObject.ApplyModifiedProperties();
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
