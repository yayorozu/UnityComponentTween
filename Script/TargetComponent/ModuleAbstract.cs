using System;
using System.Linq;
using UnityEngine;
using Yorozu.Easing;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public abstract class ModuleAbstract
	{
		public abstract string Name { get; }
		/// <summary>
		/// Editorのパラメータセットに使う
		/// </summary>
		public abstract Type ParamType { get; }

		private ComponentTweenParam _param;
		private bool _isFixed;

		private TweenValue[] _begins;
		private TweenValue[] _caches;

		internal void Initialize(ComponentTweenParam param, GameObject[] objs)
		{
			if (objs == null || objs.Length <= 0)
			{
				throw new Exception("GameObject is null");
			}

			_param = param;
			_begins = new TweenValue[objs.Length];
			_caches = new TweenValue[objs.Length];
			for (var i = 0; i < objs.Length; i++)
			{
				_caches[i] = new TweenValue();
				_begins[i] = new TweenValue();
			}

			GetComponent(objs);
		}

		protected abstract void GetComponent(GameObject[] objs);

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

		/// <summary>
		/// 事前にパラメータをキャッシュ
		/// </summary>
		internal void PreEval()
		{
			var values = GetValue();
			for (var i = 0; i < values.Length; i++)
				_begins[i].Value = values[i];
		}

		internal void Eval(float t)
		{
			t = Mathf.Clamp01(t);
			for (var i = 0; i < _caches.Length; i++)
			{
				for (var j = 0; j < 4; j++)
				{
					if (!_param.Lock.HasFlag((LockValue) (1 << (j + 1))))
						continue;

					_caches[i][j] = Ease.Eval(_param.EaseType, t, _param.BeginValue[j], _param.EndValue[j]);
				}
			}

			SetValue(_caches);
		}

		/// <summary>
		/// 開始時の値を取得
		/// </summary>
		protected abstract Vector4[] GetValue();

		protected abstract void SetValue(TweenValue[] values);

		protected T[] GetComponentsToArray<T>(GameObject[] objs) where T : Component
		{
			return objs.Select(o => o.GetComponent<T>()).ToArray();
		}
	}
}
