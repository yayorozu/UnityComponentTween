using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public class ImageColor : ImageModule
	{
		public override Type ParamType => typeof(Color);

		protected override Vector4[] GetValue()
		{
			return Components.Select(c => TweenValue.ColorToVector4(c.color))
				.ToArray();
		}

		protected override void SetValue(TweenValue[] values)
		{
			for (var i = 0; i < values.Length; i++)
			{
				Components[i].color = values[i].GetColor();
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(Components[i]);
#endif
			}
		}
	}
}
