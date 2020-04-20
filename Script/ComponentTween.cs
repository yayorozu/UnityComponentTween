using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public static class ComponentTween
	{
		private static List<ComponentTweenSequence> _playingSequences = new List<ComponentTweenSequence>();

		internal static void Add(ComponentTweenSequence tween)
		{
			_playingSequences.TryAdd(tween);
		}

		internal static void Remove(ComponentTweenSequence tween)
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

		public static ComponentTweenSequence Play(GameObject gameObject, string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;

			var components = gameObject.GetComponentsInChildren<ComponentTweenSequence>();
			foreach (var rectTweenSequence in components)
			{
				if (rectTweenSequence.ID != id)
					continue;

				rectTweenSequence.Play();
				return rectTweenSequence;
			}

			return null;
		}

	}
}