using System.Collections.Generic;
using Yorozu.Easing;
using UnityEngine;
using UnityEngine.UI;

namespace Yorozu.ComponentTween
{
	internal class ComponentTweener
	{
		private float _delayTime;
		private float _time;
		
		private Transform[] _targetTrans = new Transform[0];
		private RectTransform[] _targetRects = new RectTransform[0];
		private CanvasGroup[] _targetCanvases = new CanvasGroup[0];
		private Image[] _targetImages = new Image[0];

		private Vector4 cacheVector4;
		private Vector4 cacheVector4_2;

		private ComponentTweenParam _param;
		private TweenTarget _target;
		
		internal TweenTarget Target => _target;
		internal ComponentTweenParam Param => _param;
		private bool _isFixed;

		internal ComponentTweener(ComponentTweenParam param, TweenTarget target)
		{
			_param = param;
			_target = target;
		}

		internal void Awake()
		{
			Reset();
			SetTarget();
		}

		private void SetTarget()
		{
			T[] GetTargets<T>()
			{
				var ret = new List<T>(_target.TargetObjects.Count);
				foreach (var obj in _target.TargetObjects)
					if (obj != null)
					{
						var t = obj.GetComponent<T>();
						if (t != null)
							ret.Add(t);
					}

				return ret.ToArray();
			}

			switch (_param.Type)
			{
				case TweenType.Scale:
				case TweenType.ScaleFlags:
				case TweenType.EulerAngle:
				case TweenType.Position:
					_targetTrans = GetTargets<Transform>();
					break;
				case TweenType.AnchoredPosition:
					_targetRects = GetTargets<RectTransform>();
					break;
				case TweenType.ImageColor:
					_targetImages = GetTargets<Image>();
					break;
				case TweenType.CanvasGroupAlpha:
					_targetCanvases = GetTargets<CanvasGroup>();
					break;
			}
		}

		internal void Reset()
		{
			_isFixed = false;
		}
		
		/// <summary>
		/// 値調整
		/// </summary>
		internal void FixValue(float t)
		{
			if (_isFixed)
				return;
			
			_isFixed = true;
			Eval(t);
		}

		internal void Eval(float t)
		{
			t = Mathf.Clamp01(t);
#if UNITY_EDITOR
			EditorSetTarget();
#endif
			CalcValue(_param.ControlTarget, t);
			switch (_param.Type)
			{
				case TweenType.Scale:
					foreach (var rect in _targetTrans)
						rect.localScale = Vector3.one * cacheVector4.x;

					break;
					
				case TweenType.ScaleFlags:
					foreach (var trans in _targetTrans)
					{
						cacheVector4_2 = trans.localScale;
						SetValue();
						trans.localScale = cacheVector4_2;
					}
					break;
					
				case TweenType.Position:
					foreach (var trans in _targetTrans)
					{
						cacheVector4_2 = trans.localPosition;
						SetValue();
						trans.localPosition = cacheVector4_2;
					}
					break;
				
				case TweenType.AnchoredPosition:
					foreach (var rect in _targetRects)
					{
						cacheVector4_2 = rect.anchoredPosition;
						SetValue();
						rect.anchoredPosition = cacheVector4_2;
					}
					break;
				
				case TweenType.EulerAngle:
					foreach (var rect in _targetTrans)
					{
						cacheVector4_2 = rect.localEulerAngles;
						SetValue();
						rect.localEulerAngles = cacheVector4_2;
					}
					break;
				
				case TweenType.ImageColor:
					foreach (var image in _targetImages)
					{
						image.color = cacheVector4;
#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(image);
#endif
					}

					break;
				
				case TweenType.CanvasGroupAlpha:
					foreach (var canvas in _targetCanvases)
						canvas.alpha = cacheVector4.x;
					break;
			}
		}

		private void SetValue(bool isRelative = false)
		{
			for (int i = 0; i < 3; i++)
			{
				if (!_param.ControlTarget.HasFlag((ControlTarget) (1 << (i + 1))))
					continue;

				if (isRelative)
					cacheVector4_2[i] += cacheVector4[i];
				else
					cacheVector4_2[i] = cacheVector4[i];
			}
		}
		
		private void CalcValue(ControlTarget controlTarget, float t)
		{
			for (int i = 0; i < 3; i++)
			{
				if (!_param.ControlTarget.HasFlag((ControlTarget) (1 << (i + 1))))
					continue;
				
				cacheVector4[i] = Ease.Eval(_param.EaseType, t, _param.Begin[i], _param.End[i]);
			}
		}
		
#if UNITY_EDITOR
		
		public float StartTime => _param.StartTime;
		public float EndTime => _param.EndTime;

		private void EditorSetTarget()
		{
			bool isSetTarget = false;
			switch (_param.Type)
			{
				case TweenType.Scale:
				case TweenType.ScaleFlags:
				case TweenType.EulerAngle:
				case TweenType.Position:
					isSetTarget = _targetTrans.Length <= 0;
					break;
					
				case TweenType.AnchoredPosition:
					isSetTarget = _targetRects.Length <= 0;
					break;
				case TweenType.ImageColor:
					isSetTarget = _targetImages.Length <= 0;
					break;
				case TweenType.CanvasGroupAlpha:
					isSetTarget = _targetCanvases.Length <= 0;
					break;
			}
			if (isSetTarget)
				SetTarget();
		}
		
#endif
		
	}
}