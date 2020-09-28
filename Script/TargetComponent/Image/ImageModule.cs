using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public abstract class ImageModule : ModuleAbstract
	{
		[NonSerialized]
		protected Image[] Components;

		protected override void GetComponent(GameObject[] objs)
		{
			Components = GetComponentsToArray<Image>(objs);
		}
	}
}
