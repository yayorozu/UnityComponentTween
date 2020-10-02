using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public partial class ComponentTweenParam
	{
		[SerializeReference]
		public ModuleAbstract Module;
		public EaseType EaseType = EaseType.Linear;
		/// <summary>
		///     値
		/// </summary>
		public TweenValue BeginValue;
		public TweenValue EndValue;
		public LockValue Lock = LockValue.None;
		/// <summary>
		///     相対的か
		/// </summary>
		public bool IsRelative;
		/// <summary>
		///     開始時間
		/// </summary>
		public float Start;
		/// <summary>
		///     長さ
		/// </summary>
		public float Length;
		public float End => Start + Length;

		public AnimationCurve Curve = new AnimationCurve();
	}
}
