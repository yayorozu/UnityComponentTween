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

		private ComponentTweenSequence _component;
		[SerializeField]
		private ComponentTweenSimulate _simulate;

		private int _editIndex;

		private Type[] _moduleTypes;

		private void OnEnable()
		{
			_component = (ComponentTweenSequence) serializedObject.targetObject;
			_simulate = new ComponentTweenSimulate(_component, this);

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

			using (new EditorGUI.DisabledScope(_simulate.IsPlaying))
			{
				EditorGUILayout.PropertyField(_playOnAwake);
				EditorGUILayout.PropertyField(_id);
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.PropertyField(_totalTime);
					if (check.changed)
					{
						var max = _component.Params.Max(p => p.End);
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
						// データが差し替わったらTargetObjectsのサイズをあわせる
						EditorGUILayout.PropertyField(_tweenData);
						if (check2.changed)
						{
							var count = _component.Params.Length;
							_component.Targets = new TweenTarget[count];
							for (var i = 0; i < count; i++)
								_component.Targets[i] = new TweenTarget();

							_editIndex = -1;
						}
					}

					// Tweenのデータを保存できるように
					if (GUILayout.Button(_tweenData.objectReferenceValue != null ? "Save As" : "Save"))
					{
						var path = EditorUtility.SaveFilePanelInProject("Select Save Path", "TweenData", "asset", "Select ComponentTweenData Save Path");
						if (string.IsNullOrEmpty(path))
							return;

						var data = CreateInstance<ComponentTweenData>();
						data.Params = _component.Params;
						AssetDatabase.CreateAsset(data, path);
						_tweenData.objectReferenceValue = AssetDatabase.LoadAssetAtPath<ComponentTweenData>(path);
					}

					using (new EditorGUILayout.VerticalScope())
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField("Tweeners", EditorStyles.boldLabel);
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
							{
								_targets.InsertArrayElementAtIndex(_params.arraySize);
								_component.Targets[_component.Targets.Length - 1] = new TweenTarget();
								// Param はデータがセットしてある場合はそっちのを増やす
								if (_tweenData.objectReferenceValue == null)
								{
									_params.InsertArrayElementAtIndex(_params.arraySize);
									_component.Params[_component.Params.Length - 1] = new ComponentTweenParam {Length = 0.01f};
								}
								else
								{
									ArrayUtility.Add(ref _component.Data.Params, new ComponentTweenParam{Length = 0.01f});
									EditorUtility.SetDirty(_component.Data);
									AssetDatabase.SaveAssets();
								}

								serializedObject.ApplyModifiedProperties();

								GUIUtility.ExitGUI();
							}
						}

						for (var i = 0; i < _component.Params.Length; i++)
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

				_component.Params[index].OnGUI(_totalTime.floatValue);
				if (GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton", GUILayout.Width(16)))
				{
					_targets.DeleteArrayElementAtIndex(index);
					if (_tweenData.objectReferenceValue == null)
					{
						_params.DeleteArrayElementAtIndex(index);
					}
					else
					{
						ArrayUtility.RemoveAt(ref _component.Data.Params, index);
						EditorUtility.SetDirty(_component.Data);
						AssetDatabase.SaveAssets();
					}

					serializedObject.ApplyModifiedProperties();
					if (_editIndex == index)
						_editIndex--;

					GUIUtility.ExitGUI();
				}
			}
		}

		/// <summary>
		/// パラメータ
		/// </summary>
		private void DrawParameter()
		{
			if (_component.Params == null || _component.Params.Length <= 0 || _editIndex < 0)
				return;

			if (_component.Params.Length <= _editIndex)
				_editIndex = 0;

			_component.Params[_editIndex].OnGUIDetail(_totalTime.floatValue, _moduleTypes, _component.Targets[_editIndex]);
		}
	}
}

#endif
