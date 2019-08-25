using System;
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
		[SerializeField]
		private float _duration;
		[SerializeField]
		private float _delay;
		/// <summary>
		/// 前のと一緒に再生するか
		/// </summary>
		[SerializeField]
		public bool IsJoin;
		
		/// <summary>
		/// 操作対象
		/// </summary>
		[SerializeField]
		private RectTransform _targetRect;
		[SerializeField]
		private CanvasGroup _targetGroup;
		[SerializeField]
		private Image _targetImage;

		/// <summary>
		/// 値
		/// </summary>
		[SerializeField]
		private Vector3 _beginVector3;
		[SerializeField]
		private Vector3 _endVector3;
		[SerializeField]
		private Color _beginColor;
		[SerializeField]
		private Color _endColor;
		[SerializeField]
		private float _beginFloat;
		[SerializeField]
		private float _endFloat;

		private float _delayTime;
		private float _time;
		
		internal void Play()
		{
			_delayTime = _delay;
			_time = _duration;
		}
		
		internal bool Update(float delta)
		{
			if (_delayTime > 0)
			{
				_delayTime -= delta;
				return false;
			}

			if (_time > 0)
			{
				Eval(Mathf.InverseLerp(_duration, 0, _time));
				_time -= delta;
				return false;
			}
			
			Eval(1f);
			return true;
		}

		private void Eval(float t)
		{
			switch (_type)
			{
				case RectTweenType.Scale:
					var scale = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
					_targetRect.SetLocalScale(scale);
					break;
				case RectTweenType.AnchoredPosition:
					_targetRect.anchoredPosition = new Vector2(
						Ease.Eval(_easeType, t, _beginVector3.x, _endVector3.x),
						Ease.Eval(_easeType, t, _beginVector3.y, _endVector3.y)
					);
					break;
				case RectTweenType.Rotation:
					break;
				case RectTweenType.ImageColor:
					_targetImage.color = new Color(
							Ease.Eval(_easeType, t, _beginColor.r, _endColor.r),
							Ease.Eval(_easeType, t, _beginColor.g, _endColor.g),
							Ease.Eval(_easeType, t, _beginColor.b, _endColor.b),
							Ease.Eval(_easeType, t, _beginColor.a, _endColor.a)
						);
					break;
				case RectTweenType.CanvasGroupAlpha:
					_targetGroup.alpha = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
					break;
			}
		}
	}
}