using System.Collections.Generic;
using UnityEngine;

namespace UniLib.RectTween
{
	public enum RectTweenLoopType
	{
		None,
		Loop,
		PingPong,
	}
	
	public partial class RectTweenSequence : MonoBehaviour
	{
		[SerializeField]
		private bool _playOnAwake;
		[SerializeField]
		private string id;
		[SerializeField]
		private bool _isIgnoreTimeScale;
		[SerializeField]
		private RectTweener[] _tweeners = new RectTweener[0];
		[SerializeField]
		private RectTweenLoopType _loopType;

		private bool isReverse;
		
		public string ID => id;
		
		private bool _isPlaying;
	
		/// <summary>
		/// 再生中のやつ
		/// </summary>
		private int _playIndex;

		private List<List<int>> playGroups;

		public delegate void CompleteDelegate();

		public event CompleteDelegate CompleteEvent;

		private void Awake()
		{
			playGroups = new List<List<int>>();
			for (int i = 0; i < _tweeners.Length; i++)
			{
				var indexed = new List<int> {i};
				if (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
				{
					while (i + 1 < _tweeners.Length && _tweeners[i + 1].IsJoin)
					{
						indexed.Add(i + 1);
						i++;
					}
				}
				playGroups.Add(indexed);
			}
			
			if (_playOnAwake)
			{
				Play();
			}
		}
		
		private void OnDestroy()
		{
			RectTween.Remove(this);
		}

		private void FixedUpdate()
		{
			if (!_isPlaying)
				return;

			if (isReverse && _playIndex < 0 || !isReverse && _playIndex >= playGroups.Count)
			{
				Complete();
				return;
			}

			int count = 0;
			foreach (var i in playGroups[_playIndex])
			{
				if (_tweeners[i].Update(_isIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime, isReverse))
					count++;
			}

			if (count >= playGroups[_playIndex].Count)
				_playIndex += isReverse ? -1 : 1;
		}

		private void Complete()
		{
			if (_loopType != RectTweenLoopType.None)
			{
				if (_loopType == RectTweenLoopType.PingPong)
					isReverse = !isReverse;
				
				foreach (var tweener in _tweeners)
					tweener.Prepare();
				_playIndex = isReverse ? playGroups.Count - 1 : 0;
				return;
			}
			_isPlaying = false;
			RectTween.Remove(this);
			CompleteEvent?.Invoke();
		}

		public void Play()
		{
			if (playGroups.Count <= 0)
				return;
			
			RectTween.Add(this);
			_playIndex = isReverse ? playGroups.Count - 1 : 0;
			foreach (var tweener in _tweeners)
				tweener.Prepare();
			
			_isPlaying = true;
		}
		
		public void Stop()
		{
			_isPlaying = false;
		}

		public void Kill()
		{
			_isPlaying = false;
			RectTween.Remove(this);
		}
	}
}