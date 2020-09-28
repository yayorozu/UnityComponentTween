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
		/// float
		/// </summary>
		public void SetFloat(float value)
		{
			_value[0] = value;
		}

		public float GetFloat()
		{
			return _value[0];
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

		public static Vector4 BoolToVector4(bool v)
		{
			return new Vector4(v ? 1f : 0f, 0, 0, 0);
		}

		public static Vector4 Vector2ToVector4(Vector2 v)
		{
			return new Vector4(v.x, v.y, 0, 0);
		}

		public static Vector4 FloatToVector4(float v)
		{
			return new Vector4(v, 0, 0, 0);
		}

		public static Vector4 ColorToVector4(Color v)
		{
			return new Vector4(v.r, v.g, v.b, v.a);
		}

	}
}
