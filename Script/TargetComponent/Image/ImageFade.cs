using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public class ImageFade : ImageModule
	{
		public override Type ParamType => typeof(float);

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.FloatToVector4(c.color.a))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
			{
				var c = Components[i].color;
				c.a = values[i].GetFloat();
				Components[i].color = c;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(Components[i]);
#endif
			}
		}
	}
}
