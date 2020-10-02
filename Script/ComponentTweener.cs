using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	internal class ComponentTweener
	{
		private float _delayTime;
		private float _time;

		private ComponentTweenParam _param;
		private TweenTarget _target;

		private bool _isFixed;
		private bool _isCacheDefaultValue;

		internal Type ModuleType => _param.Module.GetType();
		internal float Start => _param.Start;

		internal ComponentTweener(ComponentTweenParam param, TweenTarget target)
		{
			_param = param;
			_target = target;
		}

		internal void Initialize()
		{
			if (_param.Module == null)
			{
				throw new Exception("Module is null");
			}
			_param.Module.Initialize(_param, _target.TargetObjects);
			Reset();
		}

		internal void Reset()
		{
			_isFixed = false;
		}

		internal void Eval(float t, bool isReverse)
		{
			if (_isFixed)
				return;

			// 反転の場合
			if (isReverse)
			{
				if (t <= _param.Start)
				{
					Eval(0f);
					_isFixed = true;
				}
				else if (t <= _param.End && _param.Length > 0f)
				{
					Eval((t - _param.Start) / _param.Length);
				}
			}
			else
			{
				if (t >= _param.End)
				{
					Eval(1f);
					_isFixed = true;
				}
				else if (t >= _param.Start && _param.Length > 0f)
				{
					Eval((t - _param.Start) / _param.Length);
				}
			}
		}

		/// <summary>
		/// キャッシュ処理があるためメソッド分割
		/// </summary>
		/// <param name="t"></param>
		private void Eval(float t)
		{
			_param.Module.Eval(t);
		}

		internal void Undo()
		{
			_param.Module.UndoValue();
		}
	}
}
