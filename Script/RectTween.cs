using System.Collections.Generic;

namespace Yorozu.RectTween
{
	public static class RectTween
	{
		private static List<RectTweenSequence> _playingSequences = new List<RectTweenSequence>();

		internal static void Add(RectTweenSequence tween)
		{
			_playingSequences.TryAdd(tween);
		}

		internal static void Remove(RectTweenSequence tween)
		{
			_playingSequences.TryRemove(tween);
		}
		
		public static void StopAll()
		{
			foreach (var seq in _playingSequences)
				seq.Stop();
		}
		
		public static void KillAll()
		{
			foreach (var seq in _playingSequences)
				seq.Kill();
		}

		public static void Stop(string id)
		{
			if (string.IsNullOrEmpty(id))
				return;

			foreach (var seq in _playingSequences.FindAll(s => s.ID == id))
				seq.Stop();
		}
		
		public static void Kill(string id)
		{
			if (string.IsNullOrEmpty(id))
				return;

			foreach (var seq in _playingSequences.FindAll(s => s.ID == id))
				seq.Kill();
		}

	}
}