using UnityEngine;

namespace Yorozu.ComponentTween
{
	[CreateAssetMenu(menuName = "ComponentTween/Create", fileName = "ComponentTweenData")]
	public class ComponentTweenData : ScriptableObject
	{
		[SerializeField]
		public ComponentTweenParam[] Params = new ComponentTweenParam[0];
	}
}
