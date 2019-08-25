using System;
using UniLib.UniEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniLib.RectTween.Editor
{
	[CustomEditor(typeof(RectTweenSequence))]
	public class RectTweenSequenceEditor : UnityEditor.Editor
	{
		private SerializedProperty _playOnAwake;
		private SerializedProperty _id;
		private SerializedProperty _isIgnoreTimeScale;

		private ReorderableList _reorderableList;
		
		private void OnEnable()
		{
			_playOnAwake = serializedObject.FindProperty("_playOnAwake");
			_id = serializedObject.FindProperty("ID");
			_isIgnoreTimeScale = serializedObject.FindProperty("_isIgnoreTimeScale");

			_reorderableList = GetReorderableList(serializedObject.FindProperty("_tweeners"));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_playOnAwake);
			EditorGUILayout.PropertyField(_id);
			EditorGUILayout.PropertyField(_isIgnoreTimeScale);
			
			_reorderableList.DoList(UniEditorUtility.GetRect());

			serializedObject.ApplyModifiedProperties();
		}
		
		private ReorderableList GetReorderableList(SerializedProperty property)
		{
			return new ReorderableList(serializedObject, property, true, true, false, false)
			{
				drawHeaderCallback = rect =>
				{
					rect.width -= 20f;
					EditorGUI.LabelField(rect, $"{property.displayName}: {property.arraySize}", EditorStyles.boldLabel);
					var position = new Rect(rect.width + 20f, rect.y - 2f, 25f, 13f);
					if (GUI.Button(position, EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
						property.InsertArrayElementAtIndex(property.arraySize);
				},
				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					DrawTweener(rect, index, property.GetArrayElementAtIndex(index));
					
					var position = new Rect(rect.width + 15f, rect.y, 25f, 13f);
					if (GUI.Button(position, EditorGUIUtility.TrIconContent("Toolbar Minus"), "RL FooterButton"))
						property.DeleteArrayElementAtIndex(index);
				},
				footerHeight = 0f,
				drawFooterCallback = rect => EditorGUI.LabelField(rect, string.Empty),
				elementHeightCallback = index => index == 0 ? EditorGUIUtility.singleLineHeight * 8 : EditorGUIUtility.singleLineHeight * 9,
			};
		}
		
		private void DrawTweener(Rect rect, int index, SerializedProperty property)
		{
			var typeProperty = property.serializedObject.FindProperty(property.propertyPath + "._type");
			var easeTypeProperty = property.serializedObject.FindProperty(property.propertyPath + "._easeType");
			var durationProperty = property.serializedObject.FindProperty(property.propertyPath + "._duration");
			var delayProperty = property.serializedObject.FindProperty(property.propertyPath + "._delay");
			var joinProperty = property.serializedObject.FindProperty(property.propertyPath + ".IsJoin");
			var rectProperty = property.serializedObject.FindProperty(property.propertyPath + "._targetRect");
			var groupProperty = property.serializedObject.FindProperty(property.propertyPath + "._targetGroup");
			var imageProperty = property.serializedObject.FindProperty(property.propertyPath + "._targetImage");
			
			var beginVector3Property = property.serializedObject.FindProperty(property.propertyPath + "._beginVector3");
			var endVector3Property = property.serializedObject.FindProperty(property.propertyPath + "._endVector3");
			
			var beginColorProperty = property.serializedObject.FindProperty(property.propertyPath + "._beginColor");
			var endColorProperty = property.serializedObject.FindProperty(property.propertyPath + "._endColor");
			
			var beginFloatProperty = property.serializedObject.FindProperty(property.propertyPath + "._beginFloat");
			var endFloatProperty = property.serializedObject.FindProperty(property.propertyPath + "._endFloat");
			
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(rect, $"[{index}]");
			
			if (index > 0)
			{
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, joinProperty);
			}

			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, typeProperty);
			
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, easeTypeProperty);
			
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, durationProperty);
			
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, delayProperty);
			
			rect.y += EditorGUIUtility.singleLineHeight;
			switch (typeProperty.intValue)
			{
				case (int)RectTweenType.Scale:
				case (int)RectTweenType.AnchoredPosition:
				case (int)RectTweenType.Rotation:
					EditorGUI.PropertyField(rect, rectProperty);
					break;
					
				case (int)RectTweenType.ImageColor:
					EditorGUI.PropertyField(rect, imageProperty);
					break;
					
				case (int)RectTweenType.CanvasGroupAlpha:
					EditorGUI.PropertyField(rect, groupProperty);
					break;
			}
			
			rect.y += EditorGUIUtility.singleLineHeight;
			switch (typeProperty.intValue)
			{
				case (int)RectTweenType.Scale:
				case (int)RectTweenType.CanvasGroupAlpha:
					EditorGUI.PropertyField(rect, beginFloatProperty);
					rect.y += EditorGUIUtility.singleLineHeight;
					EditorGUI.PropertyField(rect, endFloatProperty);
					break;
					
				case (int)RectTweenType.AnchoredPosition:
				case (int)RectTweenType.Rotation:
					EditorGUI.PropertyField(rect, beginVector3Property);
					rect.y += EditorGUIUtility.singleLineHeight;
					EditorGUI.PropertyField(rect, endVector3Property);
					break;

				case (int)RectTweenType.ImageColor:
					EditorGUI.PropertyField(rect, beginColorProperty);
					rect.y += EditorGUIUtility.singleLineHeight;
					EditorGUI.PropertyField(rect, endColorProperty);
					break;
			}
		}
	}
}