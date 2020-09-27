using UnityEngine;

namespace Yorozu.ComponentTween
{
	internal class ComponentTweener
	{
		private float _delayTime;
		private float _time;

		private Vector4 cacheVector4;
		private Vector4 cacheVector4_2;

		private ComponentTweenParam _param;
		private TweenTarget _target;

		internal ComponentTweenParam Param => _param;
		private bool _isFixed;

		internal ComponentTweener(ComponentTweenParam param, TweenTarget target)
		{
			_param = param;
			_target = target;
		}

		internal void Initialize()
		{
			_param.Module.Initialize(_param, _target.TargetObjects);
			Reset();
		}

		internal void Reset()
		{
			_isFixed = false;
		}

		/// <summary>
		/// 値固定
		/// </summary>
		internal void FixValue(float t)
		{
			if (_isFixed)
				return;

			_isFixed = true;
			Eval(t);
		}

		internal void PreEval()
		{
			_param.Module.PreEval();
		}

		internal void Eval(float t)
		{
			_param.Module.Eval(t);
		}
	}
}
