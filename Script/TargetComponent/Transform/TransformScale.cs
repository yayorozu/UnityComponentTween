using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public class TransformScale : TransformModule
	{
		public override Type ParamType => typeof(Vector3);

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.Vector3ToVector4(c.lossyScale))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
			{
				var scale = values[i].GetVector3();
				Components[i].localScale = new Vector3(
					scale.x / Components[i].localScale.x * scale.x,
					scale.y / Components[i].localScale.y * scale.y,
					scale.z / Components[i].localScale.z  * scale.z
				);
			}
		}
	}
}
