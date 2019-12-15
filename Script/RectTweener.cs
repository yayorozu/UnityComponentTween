using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UniLib.RectTween
{
	public enum RectTweenType
	{
		Scale,
		AnchoredPosition,
		Rotation,
		
		ImageColor,
		CanvasGroupAlpha,
	}
	
	[Serializable]
	public class RectTweener
	{
		[SerializeField]
		private RectTweenType _type;
		[SerializeField]
		private EaseType _easeType = EaseType.Linear;
		[SerializeField, Range(0.01f, 20f)]
		private float _duration;
		[SerializeField, Range(0f, 20f)]
		private float _delay;
		/// <summary>
		/// 前のと一緒に再生するか
		/// </summary>
		[SerializeField]
		private bool _isJoin;
		/// <summary>
		/// 操作対象
		/// </summary>
		[SerializeField]
		private GameObject[] _targets = new GameObject[1];

		/// <summary>
		/// 値
		/// </summary>
		[SerializeField]
		private Vector3 _beginVector3;
		[SerializeField]
		private Vector3 _endVector3;
		[SerializeField]
		private Color _beginColor = Color.white;
		[SerializeField]
		private Color _endColor = Color.white;
		[SerializeField]
		private float _beginFloat;
		[SerializeField]
		private float _endFloat;

		private float _delayTime;
		private float _time;
		
		public float Duration => _duration;
		public float Delay => _delay;
		public bool IsJoin => _isJoin;

		private RectTransform[] _targetRects = new RectTransform[0];
		private CanvasGroup[] _targetCanvases = new CanvasGroup[0];
		private Image[] _targetImages = new Image[0];

		internal void Awake()
		{
			SetTarget();
		}

		private void SetTarget()
		{
			T[] GetTargets<T>()
			{
				var ret = new List<T>(_targets.Length);
				foreach (var t in _targets)
					ret.AddGetComponent(t);

				return ret.ToArray();
			}

			switch (_type)
			{
				case RectTweenType.Scale:
				case RectTweenType.AnchoredPosition:
				case RectTweenType.Rotation:
					_targetRects = GetTargets<RectTransform>();
					break;
				case RectTweenType.ImageColor:
					_targetImages = GetTargets<Image>();
					break;
				case RectTweenType.CanvasGroupAlpha:
					_targetCanvases = GetTargets<CanvasGroup>();
					break;
			}			
		}
		
		internal void Prepare()
		{
			_delayTime = _delay;
			_time = _duration;
		}
		
		internal bool Update(float delta, bool isReverse)
		{
			if (isReverse && CheckTime(delta, isReverse))
				return false;
				
			if (CheckDelay(delta))
				return false;
			
			if (!isReverse && CheckTime(delta, isReverse))
				return false;

			Eval(isReverse ? 0f : 1f);
			return true;
		}

		private bool CheckDelay(float delta)
		{
			if (_delayTime <= 0)
				return false;

			_delayTime -= delta;
			return true;	
		}

		private bool CheckTime(float delta, bool isReverse)
		{
			if (_time <= 0)
				return false;
			
			Eval(isReverse ?
				Mathf.InverseLerp(0, _duration, _time) : 
				Mathf.InverseLerp(_duration, 0, _time));
			_time -= delta;
			return true;
		}

		private void Eval(float t)
		{
#if UNITY_EDITOR

			SetTarget();
#endif
			
			switch (_type)
			{
				case RectTweenType.Scale:
					var scale = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
					foreach (var rect in _targetRects)
						rect.localScale = Vector3.one * scale;
					break;
				case RectTweenType.AnchoredPosition:
					var position = new Vector2(
						Ease.Eval(_easeType, t, _beginVector3.x, _endVector3.x),
						Ease.Eval(_easeType, t, _beginVector3.y, _endVector3.y)
					);
					foreach (var rect in _targetRects)
						rect.anchoredPosition = position;
					break;
				case RectTweenType.Rotation:
					break;
				case RectTweenType.ImageColor:
					var color = new Color(
						Ease.Eval(_easeType, t, _beginColor.r, _endColor.r),
						Ease.Eval(_easeType, t, _beginColor.g, _endColor.g),
						Ease.Eval(_easeType, t, _beginColor.b, _endColor.b),
						Ease.Eval(_easeType, t, _beginColor.a, _endColor.a)
					);
					foreach (var image in _targetImages)
						image.color = color;

					break;
				case RectTweenType.CanvasGroupAlpha:
					float alpha = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
					foreach (var canvas in _targetCanvases)
						canvas.alpha = alpha;
					break;
			}
		}
		
#if UNITY_EDITOR

		public void EditorEval(float t)
		{
			Eval(t);
		}

#endif
		
	}
}