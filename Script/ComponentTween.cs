using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public static class ComponentTween
	{
		private static List<ComponentTweenSequence> _activeSequences = new List<ComponentTweenSequence>(10);

		internal static void Add(ComponentTweenSequence tween)
		{
			if (_activeSequences.Contains(tween))
				return;

			_activeSequences.Add(tween);
		}

		internal static void Remove(ComponentTweenSequence tween)
		{
			if (!_activeSequences.Contains(tween))
				return;

			_activeSequences.Remove(tween);
		}

		/// <summary>
		/// すべて停止
		/// </summary>
		public static void StopAll()
		{
			foreach (var seq in _activeSequences)
				seq.Stop();
		}

		/// <summary>
		/// 全部終了
		/// </summary>
		public static void KillAll()
		{
			foreach (var seq in _activeSequences)
				seq.Kill();
		}

		/// <summary>
		/// 指定したものを停止
		/// </summary>
		public static void Stop(string id)
		{
			if (string.IsNullOrEmpty(id))
				return;

			foreach (var seq in _activeSequences.FindAll(s => s.ID == id))
				seq.Stop();
		}


		/// <summary>
		/// 指定したものを強制終了
		/// </summary>
		public static void Kill(string id)
		{
			if (string.IsNullOrEmpty(id))
				return;

			foreach (var seq in _activeSequences.FindAll(s => s.ID == id))
				seq.Kill();
		}

		/// <summary>
		/// 指定したものを再生
		/// </summary>
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
