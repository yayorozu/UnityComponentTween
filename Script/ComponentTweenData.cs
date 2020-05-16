using UnityEngine;

namespace Yorozu.ComponentTween
{
	[CreateAssetMenu(menuName = "UniLib/CreateRectTween", fileName = "RectTween")]
	public class ComponentTweenData : ScriptableObject
	{
		public ComponentTweenParam[] Params = new ComponentTweenParam[0];
	}
}