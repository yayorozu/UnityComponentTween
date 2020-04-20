using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.RectTween
{
	public enum RectTweenLoopType
	{
		None,
		Loop,
		PingPong,
	}

	[Serializable]
	internal class RectTweenTarget
	{
		public List<GameObject> TargetObjects = new List<GameObject>(0);
	}
	
	public partial class RectTweenSequence : MonoBehaviour
	{
		[SerializeField]
		private bool _playOnAwake;
		[SerializeField]
		private string _id;
		[SerializeField, Range(0.01f, 20f)]
		private float _totalTime = 10f;
		[SerializeField]
		private bool _isIgnoreTimeScale;
		[SerializeField]
		private RectTweenLoopType _loopType;
		/// <summary>
		/// Tweenパラメータ
		/// </summary>
		[SerializeField]
		private RectTweenParam[] _params = new RectTweenParam[0];
		/// <summary>
		/// 操作対象
		/// </summary>
		[SerializeField]
		private RectTweenTarget[] _targets = new RectTweenTarget[0];
		[SerializeField]
		private RectTweenObject _tweenObject;

		public string ID => _id;
		
		private bool _isReverse;
		private float _time;
		private bool _isPlaying;
		private RectTweener[] _tweeners;
	
		public delegate void CompleteDelegate();

		public event CompleteDelegate CompleteEvent;
		
		private void Awake()
		{
			InitTweener();

			RectTween.Add(this);
			if (_playOnAwake)
			{
				Play();
			}
		}

		private void InitTweener()
		{
			if (_tweenObject != null)
				_params = _tweenObject.Params;

			_tweeners = new RectTweener[_params.Length];
			for (var i = 0; i < _tweeners.Length; i++)
			{
				// null対応
				if (_targets.Length <= i)
					_tweeners[i] = new RectTweener(_params[i], new RectTweenTarget());
				else
					_tweeners[i] = new RectTweener(_params[i], _targets[i]);
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

			_time += _isIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			Eval(_time, _isReverse);
		}

		private void Eval(float t, bool isReverse)
		{
			if (isReverse)
				t = _totalTime - t;
			
			foreach (var tweener in _tweeners)
			{
				if (isReverse && t < tweener.StartTime)
				{
					tweener.FixValue(0f);
					continue;
				}
				if (!isReverse && t > tweener.EndTime)
				{
					tweener.FixValue(1f);
					continue;
				}
				
				tweener.Eval((t - tweener.StartTime) / (tweener.EndTime  - tweener.StartTime));
			}
		}

		private void Complete()
		{
			foreach (var tweener in _tweeners)
			{
				// 終了時のいちに
				tweener.FixValue(_isReverse ? 0f : 1f);
				tweener.Reset();
			}

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
			_isReverse = false;
			Eval(0f, _isReverse);
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