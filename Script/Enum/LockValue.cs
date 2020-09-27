using System;

namespace Yorozu.ComponentTween
{
	[Flags]
	public enum LockValue
	{
		None = 0,
		X = 1 << 1,
		Y = 1 << 2,
		Z = 1 << 3,
		W = 1 << 4,
		XY = X | Y,
		XYZ = X | Y | Z,
		ALL = X | Y | Z | W,
	}
}
