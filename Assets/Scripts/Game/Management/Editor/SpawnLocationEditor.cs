using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpawnLocations))]
public class SpawnLocationEditor : Editor
{
    private bool m_EditMode = false;

    public override void OnInspectorGUI()
    {
        SpawnLocations SpawnLocations = target as SpawnLocations;

        m_EditMode = GUILayout.Toggle(m_EditMode, "Edit Mode");

        if (GUILayout.Button("Remove Location") && SpawnLocations.Locations.Count > 0)
        {
            SpawnLocations.Locations.RemoveAt(SpawnLocations.Locations.Count - 1);
        }

        if (GUILayout.Button("Reset All"))
        {
            SpawnLocations.Locations = new List<Vector3>();
        }

        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        // Handle stuff
        SpawnLocations SpawnLocations = target as SpawnLocations;
        Transform handleTransform = SpawnLocations.transform;
        Quaternion handleRotation = handleTransform.rotation;

        Handles.color = Color.white;

        for (int i = 0; i < SpawnLocations.Locations.Count; i++)
        {
            Vector3 p = SpawnLocations.Locations[i];//handleTransform.TransformPoint(SpawnLocations.Locations[i]);

            EditorGUI.BeginChangeCheck();
            p = Handles.DoPositionHandle(p, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SpawnLocations, "Move Point");
                EditorUtility.SetDirty(SpawnLocations);
                SpawnLocations.Locations[i] = handleTransform.InverseTransformPoint(p);
            }
        }

        // Spawn objects
        if (m_EditMode)
        {
            Event e = Event.current;

            // We use hotControl to lock focus onto the editor (to prevent deselection)
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID;

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        AddSpawnPoint(hit.point);
                    }

                    Event.current.Use();
                    break;

                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    break;

            }
        }
    }

    private void AddSpawnPoint(Vector3 location)
    {
        SpawnLocations SpawnLocations = target as SpawnLocations;

        Vector3 pos = location;
        pos.y += SpawnLocations.Offset / 2;

        SpawnLocations.Locations.Add(pos);
    }
}
