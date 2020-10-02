using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	public abstract class TransformModule : ModuleAbstract
	{
		protected Transform[] Components;

		protected override int GetComponent(GameObject[] objs)
		{
			Components = GetComponentsToArray<Transform>(objs);

			return Components.Length;
		}
	}
}
