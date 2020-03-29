using System;
using Yorozu.Easing;
using UnityEngine;

namespace Yorozu.RectTween
{
	public enum RectTweenType
	{
		Scale,
		ScaleAll,
		AnchoredPosition,
		EulerAngle,
		
		ImageColor,
		CanvasGroupAlpha,
		ChangeActive,
	}

	[Flags]
	public enum ControlTarget
	{
		X = 1 << 1,
		Y = 1 << 2,
		Z = 1 << 3,
		W = 1 << 4,
		XY = X | Y,
		XYZ = X | Y | Z,
		ALL = X | Y | Z | W, 
	}
	
	[Serializable]
	public class RectTweenParam
	{
		public RectTweenType Type = RectTweenType.Scale;
		public EaseType EaseType = EaseType.Linear;
		public float StartTime;
		public float EndTime;
		
		/// <summary>
		/// 値
		/// </summary>
		public Vector4 Begin;
		public Vector4 End;
		public ControlTarget ControlTarget = ControlTarget.X;
	}
}