#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public class ComponentTweenSimulate
	{
		private float _simulateDuration;
		private bool _isPlaying;
		private bool _isReverse;
		private ComponentTweenSequence _target;
		private Editor _editor;
		private int _loopCount;

		public ComponentTweenSimulate(ComponentTweenSequence target, Editor editor)
		{
			_target = target;
			_editor = editor;
		}

		internal bool IsPlaying => _isPlaying;

		public void OnGUI()
		{
			using (new EditorGUI.DisabledScope(Application.isPlaying))
			{
				using (new EditorGUILayout.HorizontalScope("box"))
				{
					EditorGUILayout.LabelField($"{_simulateDuration:F1}/{_target.TotalTime:F1}");

					var text = _isPlaying ? "Stop" : "Play";
					if (GUILayout.Button(text))
					{
						if (_isPlaying)
							StopSimulate();
						else
							StartSimulate();
					}
				}
			}
		}

		internal void Reset()
		{
			_simulateDuration = 0f;
			_isPlaying = false;
		}

		/// <summary>
		/// シミュレーション開始
		/// </summary>
		private void StartSimulate()
		{
			if (_isPlaying)
				return;

			_target.Initialize();
			_isPlaying = true;
			_isReverse = false;
			_simulateDuration = 0f;
			_loopCount = 0;
			EditorApplication.update += UpdateSimulate;
		}

		/// <summary>
		/// シミュレーション停止
		/// </summary>
		internal void StopSimulate()
		{
			if (!_isPlaying)
				return;

			EditorApplication.update -= UpdateSimulate;
			_simulateDuration = 0f;
			_isPlaying = false;
			_isReverse = false;
			_target.Undo();
		}

		/// <summary>
		/// 更新中
		/// </summary>
		private void UpdateSimulate()
		{
			if (!_isPlaying)
				return;

			_editor.Repaint();

			if (!(_simulateDuration >= _target.TotalTime))
			{
				_target.Eval(_simulateDuration, _isReverse);
				_simulateDuration += 0.02f;
				return;
			}

			_loopCount++;

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
				case LoopType.PingPongOnce:
					_isReverse = !_isReverse;
					_target.EditorReset();
					if (_loopCount >= 2)
						StopSimulate();
					break;
				case LoopType.Loop:
					_target.EditorReset();
					break;
			}
		}
	}
}

#endif
