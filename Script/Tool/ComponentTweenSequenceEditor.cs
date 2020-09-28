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

		internal void EditorReset()
		{
			foreach (var t in _tweeners)
				t.Reset();
		}

		/// <summary>
		/// シミュレートするための準備
		/// </summary>
		internal void EditorSimulatePrepare()
		{
			Initialize();
			foreach (var t in _tweeners)
				t.PreEval();
		}
	}
}
#endif
