using System;
using System.Linq;
using UnityEngine;
using Yorozu.Easing;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public abstract class ModuleAbstract
	{
		/// <summary>
		/// Editorのパラメータセットに使う
		/// </summary>
		public abstract Type ParamType { get; }

		private ComponentTweenParam _param;
		private bool _isFixed;

		private TweenValue[] _begins;
		private TweenValue[] _caches;
		private bool _isCacheDefaultValue;

		internal void Initialize(ComponentTweenParam param, GameObject[] objs)
		{
			if (objs == null || objs.Length <= 0)
			{
				throw new Exception("GameObject is null");
			}

			var currentCount = GetComponent(objs);
			_param = param;
			_isCacheDefaultValue = false;
			_begins = new TweenValue[currentCount];
			_caches = new TweenValue[currentCount];
			for (var i = 0; i < currentCount; i++)
			{
				_caches[i] = new TweenValue();
				_begins[i] = new TweenValue();
			}
		}

		internal void Reset()
		{
			_isFixed = false;
		}

		/// <summary>
		/// 値をこれ以上いじれないように固定
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
		private void CacheDefaultValue()
		{
			var values = GetValue();
			for (var i = 0; i < values.Length; i++)
			{
				_begins[i].Value = values[i];
			}
		}

		internal void Eval(float t)
		{
			if (_caches.Length <= 0)
				return;

			// 一番最初に呼ばれるタイミングで値をキャッシュする
			if (!_isCacheDefaultValue)
			{
				CacheDefaultValue();
				_isCacheDefaultValue = true;
			}

			t = Mathf.Clamp01(t);
			for (var i = 0; i < _caches.Length; i++)
			{
				for (var j = 0; j < 4; j++)
				{
					if (_param.Lock.HasFlag((LockValue) (1 << (j + 1))))
					{
						_caches[i][j] = _begins[i][j];
						continue;
					}

					_caches[i][j] = Ease.Eval(_param.EaseType, t, _param.BeginValue[j], _param.EndValue[j]);
					// 相対的ならデフォ値に加算
					if (_param.IsRelative)
					{
						_caches[i][j] += _begins[i][j];
					}
				}
			}

			SetValue(_caches);
		}

		/// <summary>
		/// 開始時にキャッシュした値を適応
		/// </summary>
		internal void UndoValue()
		{
			SetValue(_begins);
		}

		protected abstract int GetComponent(GameObject[] objs);
		/// <summary>
		/// 開始時の値を取得
		/// </summary>
		protected abstract Vector4[] GetValue();

		protected abstract void SetValue(TweenValue[] values);

		/// <summary>
		/// コンポーネント取得用
		/// </summary>
		protected T[] GetComponentsToArray<T>(GameObject[] objs) where T : Component
		{
			var gc = objs.Where(o => o != null)
				.Select(o => o.GetComponent<T>())
				.Where(c => c != null)
				.ToArray();

			if (gc.Length <= 0)
				Debug.LogError(typeof(T).Name + " Not Found");

			return gc;
		}
	}
}
