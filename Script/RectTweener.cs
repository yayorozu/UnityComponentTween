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
		ScaleAll,
		AnchoredPosition,
		Rotation,
		
		ImageColor,
		CanvasGroupAlpha,
	}

	[Flags]
	public enum ControlTarget
	{
		X = 1 << 0,
		Y = 1 << 1,
		Z = 1 << 2,
		W = 1 << 3,
		XYZ = X | Y | Z,
		ALL = X | Y | Z | W, 
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
		private Vector4 _begin;
		[SerializeField]
		private Vector4 _end;
		[SerializeField]
		private ControlTarget _controlTarget; 

		private float _delayTime;
		private float _time;
		
		public float Duration => _duration;
		public float Delay => _delay;
		public bool IsJoin => _isJoin;

		private RectTransform[] _targetRects = new RectTransform[0];
		private CanvasGroup[] _targetCanvases = new CanvasGroup[0];
		private Image[] _targetImages = new Image[0];

		private Vector4 cacheVector4;
		private Vector4 defaultVector4;

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
			EditorSetTarget();
#endif
			CalcValue(_controlTarget, t);
			switch (_type)
			{
				case RectTweenType.Scale:
					foreach (var rect in _targetRects)
						rect.localScale = Vector3.one * cacheVector4.x;
					break;
					
				case RectTweenType.ScaleAll:
					foreach (var rect in _targetRects)
						rect.localScale = cacheVector4;
					break;
				
				case RectTweenType.AnchoredPosition:
					foreach (var rect in _targetRects)
						rect.anchoredPosition = cacheVector4;
					break;
				
				case RectTweenType.Rotation:
					break;
				
				case RectTweenType.ImageColor:
					foreach (var image in _targetImages)
						image.color = cacheVector4;
					break;
				
				case RectTweenType.CanvasGroupAlpha:
					foreach (var canvas in _targetCanvases)
						canvas.alpha = cacheVector4.x;
					break;
			}
		}
		
		private void CalcValue(ControlTarget controlTarget, float t)
		{
			if (controlTarget.HasFlag(ControlTarget.X))
				cacheVector4.x = Ease.Eval(_easeType, t, _begin.x, _end.x);
			
			if (controlTarget.HasFlag(ControlTarget.Y))
				cacheVector4.y = Ease.Eval(_easeType, t, _begin.y, _end.y);
			
			if (controlTarget.HasFlag(ControlTarget.Z))
				cacheVector4.z = Ease.Eval(_easeType, t, _begin.z, _end.z);
			
			if (controlTarget.HasFlag(ControlTarget.W))
				cacheVector4.w = Ease.Eval(_easeType, t, _begin.w, _end.w);
		}
		
#if UNITY_EDITOR

		private void EditorSetTarget()
		{
			bool isSetTarget = false;
			switch (_type)
			{
				case RectTweenType.Scale:
				case RectTweenType.ScaleAll:
				case RectTweenType.AnchoredPosition:
				case RectTweenType.Rotation:
					isSetTarget = _targetRects.Length <= 0;	
					break;
				case RectTweenType.ImageColor:
					isSetTarget = _targetImages.Length <= 0;
					break;
				case RectTweenType.CanvasGroupAlpha:
					isSetTarget = _targetCanvases.Length <= 0;
					break;
			}
			if (isSetTarget)
				SetTarget();
		}

		public void EditorEval(float t)
		{
			Eval(t);
		}
		
#endif
		
	}
}