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
		internal ComponentTweenParam[] Params => _tweenData != null ? _tweenData.Params : _params;

		internal TweenTarget[] Targets
		{
			get { return _targets; }
			set { _targets = value; }
		}

		internal ComponentTweenData Data => _tweenData;

		internal void EditorReset()
		{
			foreach (var t in _tweeners)
				t.Reset();
		}
	}
}
#endif
