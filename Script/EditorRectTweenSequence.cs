using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniLib.RectTween
{
	
#if UNITY_EDITOR
	public partial class RectTweenSequence
	{
		public RectTweenLoopType LoopType => _loopType;

		public float TotalTime => _totalTime;

		public void ResetSimulate(float t = 0)
		{
			for (int i = _tweeners.Length - 1; i >= 0; i--)
				_tweeners[i].EditorEval(t);
		}

		public void Simulate(float t, bool isReverse)
		{
			if (isReverse)
				t = _totalTime - t;
			
			foreach (var tweener in _tweeners)
			{
				if (t < tweener.StartTime || t > tweener.EndTime)
					continue;
				
				tweener.EditorEval(t - tweener.StartTime);
			}
		}
	}
	
#endif
}