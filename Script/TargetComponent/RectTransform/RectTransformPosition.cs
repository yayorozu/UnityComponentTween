using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public class RectTransformPosition : RectTransformModule
	{
		public override Type ParamType => typeof(Vector2);

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.Vector2ToVector4(c.anchoredPosition))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
				Components[i].anchoredPosition = values[i].GetVector2();
		}
	}
}
