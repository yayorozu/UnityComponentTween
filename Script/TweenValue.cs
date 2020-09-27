using System;
using UnityEngine;

namespace Yorozu.ComponentTween
{
	[Serializable]
	public struct TweenValue
	{
		[SerializeField]
		private Vector4 _value;
		public Vector4 Value
		{
			get => _value;
			set => _value = value;
		}

		public float this[int index]
		{
			get => _value[index];
			set => _value[index] = value;
		}

		/// <summary>
		/// 基本的には使わないほうがよき
		/// </summary>
		public void Set<T>(T v) where T : struct
		{
			var v2 = (object) v;
			var t = typeof(T);

			if (t == typeof(bool))
				SetBool((bool) v2);
			else if (t == typeof(int))
				SetInt((int) v2);
			else if (t == typeof(Vector3))
				SetVector3((Vector3) v2);
			else if (t == typeof(Color))
				SetColor((Color) v2);
		}

		/// <summary>
		/// bool
		/// </summary>
		public void SetBool(bool enable)
		{
			_value[0] = enable ? 1f : 0f;
		}

		public bool GetBool()
		{
			return Math.Abs(_value[0] - 1f) < 0.000001f;
		}

		/// <summary>
		/// int
		/// </summary>
		public void SetInt(int value)
		{
			_value[0] = value;
		}

		public int GetInt()
		{
			return (int) _value[0];
		}

		/// <summary>
		/// Vector2
		/// </summary>
		public void SetVector2(Vector3 value)
		{
			for (var i = 0; i < 2; i++)
				_value[i] = value[i];
		}

		public Vector2 GetVector2()
		{
			var value = Vector2.zero;
			for (var i = 0; i < 2; i++)
				value[i] = _value[i];
			return value;
		}

		/// <summary>
		/// Vector3
		/// </summary>
		public void SetVector3(Vector3 value)
		{
			for (var i = 0; i < 3; i++)
				_value[i] = value[i];
		}

		public Vector3 GetVector3()
		{
			var value = Vector3.zero;
			for (var i = 0; i < 3; i++)
				value[i] = _value[i];
			return value;
		}

		/// <summary>
		/// Color
		/// </summary>
		public void SetColor(Color value)
		{
			for (var i = 0; i < 4; i++)
				_value[i] = value[i];
		}

		public Color GetColor()
		{
			var value = Color.white;
			for (var i = 0; i < 4; i++)
				value[i] = _value[i];
			return value;
		}
	}
}
