using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleEasing;

[CustomEditor(typeof(TransformAnimation))]
public class TransformAnimationEditor : Editor
{
	override public void OnInspectorGUI()
	{
		var anim = target as TransformAnimation;

		GUIStyle header = new GUIStyle();
		header.fontStyle = FontStyle.Bold;

        #region Translate
        GUILayout.Label("Translate X", header);
		anim.TXActive = GUILayout.Toggle(anim.TXActive, "Is Active");

		if (anim.TXActive)
		{
			anim.TXEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.TXEaseType);
			anim.TXStart = EditorGUILayout.FloatField("Start:", anim.TXStart);
			anim.TXChange = EditorGUILayout.FloatField("Change:", anim.TXChange);
			anim.TXDuration = EditorGUILayout.FloatField("Duration:", anim.TXDuration);
		}

		GUILayout.Label("Translate Y", header);
		anim.TYActive = GUILayout.Toggle(anim.TYActive, "Is Active");

		if (anim.TYActive)
		{
			anim.TYEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.TYEaseType);
			anim.TYStart = EditorGUILayout.FloatField("Start:", anim.TYStart);
			anim.TYChange = EditorGUILayout.FloatField("Change:", anim.TYChange);
			anim.TYDuration = EditorGUILayout.FloatField("Duration:", anim.TYDuration);
		}

		GUILayout.Label("Translate Z", header);
		anim.TZActive = GUILayout.Toggle(anim.TZActive, "Is Active");

		if (anim.TZActive)
		{
			anim.TZEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.TZEaseType);
			anim.TZStart = EditorGUILayout.FloatField("Start:", anim.TZStart);
			anim.TZChange = EditorGUILayout.FloatField("Change:", anim.TZChange);
			anim.TZDuration = EditorGUILayout.FloatField("Duration:", anim.TZDuration);
		}
		#endregion

		#region Rotate
		GUILayout.Label("Rotate X", header);
		anim.RXActive = GUILayout.Toggle(anim.RXActive, "Is Active");

		if (anim.RXActive)
		{
			anim.RXEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.RXEaseType);
			anim.RXStart = EditorGUILayout.FloatField("Start:", anim.RXStart);
			anim.RXChange = EditorGUILayout.FloatField("Change:", anim.RXChange);
			anim.RXDuration = EditorGUILayout.FloatField("Duration:", anim.RXDuration);
		}

		GUILayout.Label("Rotate Y", header);
		anim.RYActive = GUILayout.Toggle(anim.RYActive, "Is Active");

		if (anim.RYActive)
		{
			anim.RYEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease RYpe:", anim.RYEaseType);
			anim.RYStart = EditorGUILayout.FloatField("Start:", anim.RYStart);
			anim.RYChange = EditorGUILayout.FloatField("Change:", anim.RYChange);
			anim.RYDuration = EditorGUILayout.FloatField("Duration:", anim.RYDuration);
		}

		GUILayout.Label("Rotate Z", header);
		anim.RZActive = GUILayout.Toggle(anim.RZActive, "Is Active");

		if (anim.RZActive)
		{
			anim.RZEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.RZEaseType);
			anim.RZStart = EditorGUILayout.FloatField("Start:", anim.RZStart);
			anim.RZChange = EditorGUILayout.FloatField("Change:", anim.RZChange);
			anim.RZDuration = EditorGUILayout.FloatField("Duration:", anim.RZDuration);
		}
		#endregion

		#region Scale
		GUILayout.Label("Scale X", header);
		anim.SXActive = GUILayout.Toggle(anim.SXActive, "Is Active");

		if (anim.SXActive)
		{
			anim.SXEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.SXEaseType);
			anim.SXStart = EditorGUILayout.FloatField("Start:", anim.SXStart);
			anim.SXChange = EditorGUILayout.FloatField("Change:", anim.SXChange);
			anim.SXDuration = EditorGUILayout.FloatField("Duration:", anim.SXDuration);
		}

		GUILayout.Label("Scale Y", header);
		anim.SYActive = GUILayout.Toggle(anim.SYActive, "Is Active");

		if (anim.SYActive)
		{
			anim.SYEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.SYEaseType);
			anim.SYStart = EditorGUILayout.FloatField("Start:", anim.SYStart);
			anim.SYChange = EditorGUILayout.FloatField("Change:", anim.SYChange);
			anim.SYDuration = EditorGUILayout.FloatField("Duration:", anim.SYDuration);
		}

		GUILayout.Label("Scale Z", header);
		anim.SZActive = GUILayout.Toggle(anim.SZActive, "Is Active");

		if (anim.SZActive)
		{
			anim.SZEaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type:", anim.SZEaseType);
			anim.SZStart = EditorGUILayout.FloatField("Start:", anim.SZStart);
			anim.SZChange = EditorGUILayout.FloatField("Change:", anim.SZChange);
			anim.SZDuration = EditorGUILayout.FloatField("Duration:", anim.SZDuration);
		}
		#endregion

		EditorUtility.SetDirty(anim);
	}
}
