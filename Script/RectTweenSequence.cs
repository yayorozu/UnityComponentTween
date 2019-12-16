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
		private string _id;
		[SerializeField, Range(0.01f, 20f)]
		private float _totalTime;
		[SerializeField]
		private bool _isIgnoreTimeScale;
		[SerializeField]
		private RectTweener[] _tweeners = new RectTweener[0];
		[SerializeField]
		private RectTweenLoopType _loopType;

		private bool _isReverse;
		private float _time;
		
		public string ID => _id;
		
		private bool _isPlaying;
	
		public delegate void CompleteDelegate();

		public event CompleteDelegate CompleteEvent;

		private void Awake()
		{
			RectTween.Add(this);
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

			if (_time >= _totalTime)
			{
				Complete();
				return;
			}

			var t = _isReverse ? _totalTime - _time : _time;
			foreach (var tweener in _tweeners)
			{
				if (t < tweener.StartTime || t > tweener.EndTime)
					continue;
				
				tweener.EditorEval(t - tweener.StartTime);
			}
		}

		private void Complete()
		{
			if (_loopType != RectTweenLoopType.None)
			{
				_time = 0f;
				if (_loopType == RectTweenLoopType.PingPong)
					_isReverse = !_isReverse;
				
				return;
			}
			
			_isPlaying = false;
			CompleteEvent?.Invoke();
		}

		public void Play()
		{
			_time = 0f;
			_isPlaying = true;
		}
		
		public void Stop()
		{
			_isPlaying = false;
		}

		public void Kill()
		{
			_isPlaying = false;
			// TODO 
		}
	}
}