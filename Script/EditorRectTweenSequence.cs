using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UniLib.RectTween
{
	
#if UNITY_EDITOR
	public partial class RectTweenSequence
	{
		public RectTweenLoopType LoopType => _loopType;

		public float TotalTime => _totalTime;

		private Dictionary<GameObject, CacheObjectParam> _cacheDic;

		/// <summary>
		/// 停止時に再生時のパラメータを再度適応
		/// </summary>
		public void UndoParam()
		{
			if (_cacheDic == null)
				return;
			
			foreach (var pair in _cacheDic)
				pair.Value.Undo();
			
			_cacheDic.Clear();
		}
		
		public void ResetSimulate(float t = 0)
		{
			InitTweener();
			CacheParam();
			for (int i = _tweeners.Length - 1; i >= 0; i--)
				_tweeners[i].EditorEval(t);
		}

		public void Simulate(float t, bool isReverse)
		{
			if (isReverse)
				t = _totalTime - t;
			
			foreach (var tweener in _tweeners)
			{
				if (t < tweener.StartTime || tweener.EndTime < t)
					continue;
				
				tweener.EditorEval((t - tweener.StartTime) / (tweener.EndTime  - tweener.StartTime));
			}
		}

		private void CacheParam()
		{
			_cacheDic = new Dictionary<GameObject, CacheObjectParam>();
			foreach (var tweener in _tweeners)
			{
				foreach (var target in tweener.Target.TargetObjects)
				{
					if (!_cacheDic.ContainsKey(target))
						_cacheDic.Add(target, new CacheObjectParam(target));
					
					_cacheDic[target].Cache(tweener.Param);
				}
			}
		}
		
		private class CacheObjectParam
		{
			private readonly GameObject _target;

			private Vector2? _cachePosition;
			private Vector3? _cacheRotation;
			private Vector3? _cacheScale;
			private Color? _cacheColor;
			private float? _cacheCanvasAlpha;
			private bool? _cacheActive;
			
			public CacheObjectParam(GameObject target)
			{
				_target = target;
			}
			
			public void Cache(RectTweenParam param)
			{
				switch (param.Type)
				{
					case RectTweenType.Scale:
					case RectTweenType.ScaleAll:
						_cacheScale = (_target.transform as RectTransform).localScale;
						break;
					case RectTweenType.AnchoredPosition:
						_cachePosition = (_target.transform as RectTransform).anchoredPosition;
						break;
					case RectTweenType.EulerAngle:
						_cacheRotation = (_target.transform as RectTransform).localEulerAngles;
						break;
					case RectTweenType.ImageColor:
						_cacheColor = _target.GetComponent<Image>().color;
						break;
					case RectTweenType.CanvasGroupAlpha:
						_cacheCanvasAlpha = _target.GetComponent<CanvasGroup>().alpha;
						break;
					case RectTweenType.ChangeActive:
						_cacheActive = _target.activeSelf;
						break;
				}
			}

			public void Undo()
			{
				if (_cacheScale.HasValue)
					(_target.transform as RectTransform).localScale = _cacheScale.Value;
				
				if (_cachePosition.HasValue)
					(_target.transform as RectTransform).anchoredPosition = _cachePosition.Value;
				
				if (_cacheRotation.HasValue)
					(_target.transform as RectTransform).localEulerAngles = _cacheRotation.Value;
				
				if (_cacheColor.HasValue)
					_target.GetComponent<Image>().color = _cacheColor.Value;
				
				if (_cacheCanvasAlpha.HasValue)
					_target.GetComponent<CanvasGroup>().alpha = _cacheCanvasAlpha.Value;

				if (_cacheActive.HasValue)
					_target.SetActive(_cacheActive.Value);
			}
		}
	}
	
#endif
}