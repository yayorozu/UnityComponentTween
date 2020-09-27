#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public partial class ComponentTweenSequence
	{
		internal LoopType LoopType => _loopType;

		internal float TotalTime => _totalTime;

		/// <summary>
		/// SerializedProperty でやるのが面倒になってしまったので
		/// </summary>
		internal ComponentTweenParam[] Params => _params;
		internal TweenTarget[] Targets => _targets;

		/// <summary>
		/// 停止時に再生時のパラメータを再度適応
		/// </summary>
		internal void EditorUndoParam()
		{

		}

		internal void EditorReset()
		{
			foreach (var tweener in _tweeners)
				tweener.Reset();
		}

		/// <summary>
		/// シミュレートするための準備
		/// </summary>
		internal void EditorSimulatePrepare()
		{
			Initialize();


			foreach (var tweener in _tweeners)
			{

			}
		}

		internal void EditorSimulate(float t, bool isReverse)
		{
			Eval(t, isReverse);
		}
	}
}
#endif
