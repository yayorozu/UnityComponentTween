using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public class BehaviourEnable : ModuleAbstract
	{
		public override Type ParamType => typeof(bool);

		private Behaviour[] Components;

		protected override int GetComponent(GameObject[] objs)
		{
			Components = GetComponentsToArray<Behaviour>(objs);

			return Components.Length;
		}

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.BoolToVector4(c.enabled))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
				Components[i].enabled = values[i].GetBool();
		}
	}
}
