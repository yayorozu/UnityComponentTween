using UnityEngine;

namespace UniLib.RectTween
{
	[CreateAssetMenu(menuName = "UniLib/CreateRectTween", fileName = "RectTween")]
	public class RectTweenObject : ScriptableObject
	{
		public RectTweenParam[] Params = new RectTweenParam[0];
	}
}