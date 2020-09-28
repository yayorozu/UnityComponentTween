using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public class TweenTarget
	{
		public GameObject[] TargetObjects = new GameObject[0];
	}


	public partial class ComponentTweenSequence : MonoBehaviour
	{
		[SerializeField]
		private bool _playOnAwake = true;
		[SerializeField]
		private string _id;
		[SerializeField, Range(0.01f, 20f)]
		private float _totalTime = 2f;
		[SerializeField]
		private bool _isIgnoreTimeScale = false;
		[SerializeField]
		private LoopType _loopType;
		/// <summary>
		/// Tweenパラメータ
		/// </summary>
		[SerializeField]
		private ComponentTweenParam[] _params = new ComponentTweenParam[0];
		/// <summary>
		/// 操作対象
		/// </summary>
		[SerializeField]
		private TweenTarget[] _targets = new TweenTarget[0];
		[SerializeField]
		private ComponentTweenData _tweenData;

		public string ID => _id;

		private bool _isReverse;
		private float _time;
		private bool _isPlaying;
		private ComponentTweener[] _tweeners;

		/// <summary>
		/// Event
		/// </summary>
		public delegate void CompleteDelegate();
		public event CompleteDelegate CompleteEvent;

		private void Awake()
		{
			ComponentTween.Add(this);
			Initialize();

			if (_playOnAwake)
				Play();
		}

		private void Initialize()
		{
			if (_tweenData != null)
				_params = _tweenData.Params;

			_tweeners = new ComponentTweener[_params.Length];
			for (var i = 0; i < _tweeners.Length; i++)
				_tweeners[i] = new ComponentTweener(_params[i], _targets.Length <= i ? new TweenTarget() : _targets[i]);

			foreach (var tween in _tweeners)
				tween.Initialize();
		}

		private void OnDestroy()
		{
			ComponentTween.Remove(this);
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

		internal void Eval(float t, bool isReverse)
		{
			if (isReverse)
				t = _totalTime - t;

			foreach (var tween in _tweeners)
			{
				if (isReverse && t < tween.Param.Start)
				{
					tween.FixValue(0f);
					continue;
				}
				if (!isReverse && t > tween.Param.End)
				{
					tween.FixValue(1f);
					continue;
				}

				tween.Eval((t - tween.Param.Start) / tween.Param.Length);
			}
		}

		private void Complete()
		{
			foreach (var tween in _tweeners)
			{
				// 終了時のいちに
				tween.FixValue(_isReverse ? 0f : 1f);
				tween.Reset();
			}

			if (_loopType != LoopType.None)
			{
				_time = 0f;
				if (_loopType == LoopType.PingPong)
					_isReverse = !_isReverse;

				return;
			}

			_isPlaying = false;
			CompleteEvent?.Invoke();
		}

		/// <summary>
		/// Public Methods
		/// </summary>
		public void Play()
		{
			_time = 0f;
			_isPlaying = true;
			_isReverse = false;
			foreach (var tween in _tweeners)
				tween.PreEval();

			Eval(0f, _isReverse);
		}

		public void Undo()
		{
			foreach (var t in _tweeners)
				t.Undo();
		}

		public void Stop()
		{
			_isPlaying = false;
		}

		public void Kill()
		{
			_isPlaying = false;
			// TODO
			Destroy(gameObject);
		}
	}
}
