using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public class TransformPosition : TransformModule
	{
		public override Type ParamType => typeof(Vector3);

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.Vector3ToVector4(c.position))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
				Components[i].position = values[i].GetVector3();
		}
	}
}
