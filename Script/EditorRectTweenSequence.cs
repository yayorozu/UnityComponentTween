using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniLib.RectTween
{
	
#if UNITY_EDITOR
	public partial class RectTweenSequence
	{
		public RectTweenLoopType LoopType => _loopType;

		public float TotalDuration
		{
			get
			{
				float total = 0f;
				for (int i = 0; i < _tweeners.Length; i++)
				{
					List<float> times = new List<float>();
					times.Add(_tweeners[i].Delay + _tweeners[i].Duration);
					if (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
					{
						while (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
						{
							times.Add(_tweeners[i + 1].Delay + _tweeners[i + 1].Duration);
							i++;
						}
					}

					total += times.Max();
				}

				return total;
			}
		}

		public void ResetSimulate(float t = 0)
		{
			for (int i = _tweeners.Length - 1; i >= 0; i--)
				_tweeners[i].EditorEval(t);
		}

		public void SimulateReverse(float duration, float total)
		{
			var groups = GetGroup();
			for (int i = groups.Count - 1; i >= 0; i--)
			{
				if (duration <= 0f)
					continue;

				float prev = 0f;
				for (int j = 0; j < i; j++)
					prev += groups[j].Select(t => t.Delay + t.Duration).Max();

				foreach (var tweener in groups[i])
				{
					var t = (total - duration - prev) / tweener.Duration;
					if (t <= 1f)
						tweener.EditorEval(t);
				}

				float diff = groups[i].Select(t => t.Delay + t.Duration).Max();
				duration -= diff;
				total -= diff;
			}
		}
		
		public void Simulate(float duration)
		{
			var groups = GetGroup();
			foreach (var group in groups)
			{
				if (duration <= 0)
					continue;
				
				foreach (var tweener in group)
				{
					var t = (duration - tweener.Delay) / tweener.Duration;
					if (t <= 1f)
						tweener.EditorEval(t);
				}

				duration -= group.Select(t => t.Delay + t.Duration).Max();
			}
		}

		private List<List<RectTweener>> GetGroup()
		{
			var ret = new List<List<RectTweener>>();
			for (int i = 0; i < _tweeners.Length; i++)
			{
				var tweens = new List<RectTweener> {_tweeners[i]};
				if (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
				{
					while (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
					{
						tweens.Add(_tweeners[i + 1]);
						i++;
					}
				}
				ret.Add(tweens);
			}

			return ret;
		}
	}
	
#endif
}