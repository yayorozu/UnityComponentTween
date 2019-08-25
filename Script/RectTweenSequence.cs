using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UniLib.RectTween
{
	public class RectTweenSequence : MonoBehaviour
	{
		[SerializeField]
		private bool _playOnAwake;
		[SerializeField]
		public string ID;
		[SerializeField]
		internal bool _isIgnoreTimeScale;
		[SerializeField]
		private RectTweener[] _tweeners = new RectTweener[0];
		
		private bool _isPlaying;

		/// <summary>
		/// 再生中のやつ
		/// </summary>
		private int _playIndex;
		private List<int> _playIndexs = new List<int>();
		private List<int> _removeIndexs = new List<int>();

		public delegate void CompleteDelegate();

		public event CompleteDelegate CompleteEvent;

		private void Awake()
		{
			if (_playOnAwake)
			{
				Play();
			}
		}

		private void FixedUpdate()
		{
			if (!_isPlaying)
				return;
			
			if (_playIndexs.Count <= 0)
				return;
			
			foreach (var i in _playIndexs)
				if (_tweeners[i].Update(_isIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime))
					_removeIndexs.Add(i);

			if (_removeIndexs.Count > 0)
				foreach (var i in _removeIndexs)
					_playIndexs.Remove(i);
			
			// 次を再生
			if (_playIndexs.Count <= 0)
				PlayNext();
		}

		private void PlayNext()
		{
			if (_playIndex >= _tweeners.Length)
			{
				Complete();
				return;
			}

			_playIndexs.Add(_playIndex++);
			while (_playIndex < _tweeners.Length && _tweeners[_playIndex].IsJoin)
				_playIndexs.Add(_playIndex++);

			if (_playIndexs.Count <= 0)
				Complete();
			else
				foreach (var i in _playIndexs)
					_tweeners[i].Play();
		}

		private void OnDestroy()
		{
			RectTween.Remove(this);
		}

		private void Complete()
		{
			RectTween.Remove(this);
			CompleteEvent?.Invoke();
		}

		public void Play()
		{
			_isPlaying = true;
			_playIndex = 0;
			RectTween.Add(this);
			PlayNext();
		}
		
		public void Stop()
		{
			_isPlaying = false;
		}

		public void Kill()
		{
			_isPlaying = false;
			_playIndexs.Clear();
			RectTween.Remove(this);
		}
	}
}