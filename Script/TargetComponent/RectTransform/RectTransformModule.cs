using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public abstract class RectTransformModule : ModuleAbstract
	{
		[NonSerialized]
		protected RectTransform[] Components;

		protected override int GetComponent(GameObject[] objs)
		{
			Components = GetComponentsToArray<RectTransform>(objs);
			return Components.Length;
		}
	}
}
