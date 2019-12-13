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
		private bool _isJoin;
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
		
		internal void Prepare()
		{
			_delayTime = _delay;
			_time = _duration;
		}
		
		internal bool Update(float delta, bool isReverse)
		{
			if (isReverse)
			{
				if (CheckTime(delta, isReverse))
					return false;
				
				if (CheckDelay(delta))
					return false;
			}
			else
			{
				if (CheckDelay(delta))
					return false;
				
				if (CheckTime(delta, isReverse))
					return false;
			}

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
			switch (_type)
			{
				case RectTweenType.Scale:
					if (_targetRect != null)
					{
						var scale = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
						_targetRect.SetLocalScale(scale);
					}
					break;
				case RectTweenType.AnchoredPosition:
					if (_targetRect != null)
					{
						_targetRect.anchoredPosition = new Vector2(
							Ease.Eval(_easeType, t, _beginVector3.x, _endVector3.x),
							Ease.Eval(_easeType, t, _beginVector3.y, _endVector3.y)
						);
					}

					break;
				case RectTweenType.Rotation:
					break;
				case RectTweenType.ImageColor:
					if (_targetImage != null)
					{
						// FIXME 色単体だとEditorでは変わらない
						_targetImage.color = new Color(
							Ease.Eval(_easeType, t, _beginColor.r, _endColor.r),
							Ease.Eval(_easeType, t, _beginColor.g, _endColor.g),
							Ease.Eval(_easeType, t, _beginColor.b, _endColor.b),
							Ease.Eval(_easeType, t, _beginColor.a, _endColor.a)
						);
#if UNITY_EDITOR

#endif
					}
					break;
				case RectTweenType.CanvasGroupAlpha:
					if (_targetGroup == null)
					{
						_targetGroup.alpha = Ease.Eval(_easeType, t, _beginFloat, _endFloat);
					}
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