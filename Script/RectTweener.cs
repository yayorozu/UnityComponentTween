using System.Collections.Generic;
using Yorozu.Easing;
using UnityEngine;
using UnityEngine.UI;

namespace Yorozu.RectTween
{
	internal class RectTweener
	{
		private float _delayTime;
		private float _time;
		
		private RectTransform[] _targetRects = new RectTransform[0];
		private CanvasGroup[] _targetCanvases = new CanvasGroup[0];
		private Image[] _targetImages = new Image[0];

		private Vector4 cacheVector4;
		private Vector4 cacheVector4_2;

		private RectTweenParam _param;
		private RectTweenTarget _target;
		
		internal RectTweenTarget Target => _target;
		internal RectTweenParam Param => _param;
		private bool _isFixed;

		internal RectTweener(RectTweenParam param, RectTweenTarget target)
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
				case RectTweenType.Scale:
				case RectTweenType.ScaleAll:
				case RectTweenType.AnchoredPosition:
				case RectTweenType.EulerAngle:
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
				case RectTweenType.Scale:
					foreach (var rect in _targetRects)
						rect.localScale = Vector3.one * cacheVector4.x;

					break;
					
				case RectTweenType.ScaleAll:
					foreach (var rect in _targetRects)
					{
						cacheVector4_2 = rect.localScale;
						if (_param.ControlTarget.HasFlag(ControlTarget.X))
							cacheVector4_2.x = cacheVector4.x;
						if (_param.ControlTarget.HasFlag(ControlTarget.Y))
							cacheVector4_2.y = cacheVector4.y;
						rect.localScale = cacheVector4_2;
					}

					break;
				
				case RectTweenType.AnchoredPosition:
					foreach (var rect in _targetRects)
					{
						cacheVector4_2 = rect.anchoredPosition;
						if (_param.ControlTarget.HasFlag(ControlTarget.X))
							cacheVector4_2.x = cacheVector4.x;
						if (_param.ControlTarget.HasFlag(ControlTarget.Y))
							cacheVector4_2.y = cacheVector4.y;
						
						rect.anchoredPosition = cacheVector4_2;
					}
					break;
				
				case RectTweenType.EulerAngle:
					foreach (var rect in _targetRects)
					{
						cacheVector4_2 = rect.localEulerAngles;
						if (_param.ControlTarget.HasFlag(ControlTarget.X))
							cacheVector4_2.x = cacheVector4.x;
						if (_param.ControlTarget.HasFlag(ControlTarget.Y))
							cacheVector4_2.y = cacheVector4.y;
						if (_param.ControlTarget.HasFlag(ControlTarget.Z))
							cacheVector4_2.z = cacheVector4.z;

						rect.localEulerAngles = cacheVector4_2;
					}
					break;
				
				case RectTweenType.ImageColor:
					foreach (var image in _targetImages)
					{
						image.color = cacheVector4;
#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(image);
#endif
					}

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
				cacheVector4.x = Ease.Eval(_param.EaseType, t, _param.Begin.x, _param.End.x);

			if (controlTarget.HasFlag(ControlTarget.Y))
				cacheVector4.y = Ease.Eval(_param.EaseType, t, _param.Begin.y, _param.End.y);

			if (controlTarget.HasFlag(ControlTarget.Z))
				cacheVector4.z = Ease.Eval(_param.EaseType, t, _param.Begin.z, _param.End.z);
			
			if (controlTarget.HasFlag(ControlTarget.W))
				cacheVector4.w = Ease.Eval(_param.EaseType, t, _param.Begin.w, _param.End.w);
		}
		
#if UNITY_EDITOR
		
		public float StartTime => _param.StartTime;
		public float EndTime => _param.EndTime;

		private void EditorSetTarget()
		{
			bool isSetTarget = false;
			switch (_param.Type)
			{
				case RectTweenType.Scale:
				case RectTweenType.ScaleAll:
				case RectTweenType.AnchoredPosition:
				case RectTweenType.EulerAngle:
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
		
#endif
		
	}
}