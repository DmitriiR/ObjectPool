/*  ╔═════════════════════════════╡  DinoTank 2018 ╞════════════════════════════╗            
    ║ Authors:  Dmitrii Roets                       Email:    roetsd@icloud.com ║
    ╟───────────────────────────────────────────────────────────────────────────╢░ 
    ║ Purpose:  Extends/overrides the functionality of ObjectPool script        ║░
    ║ Usage:    This script must be in Editor folder                            ║░
    ╚═══════════════════════════════════════════════════════════════════════════╝░
       ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
*/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor
{
    ObjectPool objectPool;
    Color defBackgroundColor;

    private bool showInheritedvars = false;
    float labelWidth;
    float fieldWidth;

    public override void OnInspectorGUI()
    {
        objectPool = (ObjectPool)target;
        serializedObject.Update();
        defBackgroundColor = GUI.backgroundColor;
        labelWidth = EditorGUIUtility.labelWidth;
        fieldWidth = EditorGUIUtility.fieldWidth;

        // ----------------------Begin Logic ---------------------------
        objectPool.debugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "Log names and usage of objects?"), objectPool.debugMode);
     
        Heading1("Preloaded Objects", Color.yellow);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prewarmedObjects"), new GUIContent("Prewarmed Objects", "List of objects to create at load"), true);
        EditorGUILayout.BeginVertical("box");
        Heading1("Current Pooled Objects", Color.cyan);   
        for (int i = 0; i < objectPool.objects.Count; i++)
        {
            ShowObjectEntry(i, objectPool.objects[i], objectPool.objectsNames[i]);
        }    
        EditorGUILayout.EndVertical();

        // ----------------------End Logic ---------------------------
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUIUtility.fieldWidth = fieldWidth;
        EditorGUILayout.Space();
        GUI.color = Color.magenta;
        showInheritedvars = EditorGUILayout.Toggle("Show Full Script", showInheritedvars);
        EditorGUILayout.LabelField("__________________________________________________");
        GUI.color = defBackgroundColor;

        if (showInheritedvars)
            DrawDefaultInspector();

        EditorUtility.SetDirty(target);     
        serializedObject.ApplyModifiedProperties();

    }

    void ShowObjectEntry(int _index, GameObject _object, string _corespondingName)
    {
        if (!_object)
        {
            EditorGUIUtility.labelWidth = 200;
            GUI.color = CustomColors.LightRED;
            EditorGUILayout.BeginHorizontal("box");

            if (objectPool.debugMode)
                EditorGUILayout.LabelField(objectPool.objectsNames[_index]);

            _object = EditorGUILayout.ObjectField(_index.ToString(), _object, typeof(GameObject), true) as GameObject;

            EditorGUILayout.EndHorizontal();
            return;
        }
        else
        {
            EditorGUIUtility.labelWidth = 50;
            if (!_object.activeInHierarchy) // object ready to be recycled
            {
                GUI.color = defBackgroundColor;
                EditorGUILayout.BeginHorizontal("button");
                EditorGUILayout.ObjectField("READY ", _object, typeof(GameObject), true);

                if (objectPool.debugMode && _index < objectPool.used.Count)
                {
                    EditorGUILayout.LabelField(objectPool.objectsNames[_index]);
                    EditorGUILayout.LabelField("Used: ", objectPool.used[_index].ToString());
                }
                EditorGUILayout.EndHorizontal();
            }
            else // object in use
            {
                GUI.color = Color.gray;
                EditorGUILayout.BeginHorizontal("button");
                EditorGUILayout.ObjectField("IN USE ", _object, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

        }

    }

    void Heading1(string _heading, Color _color, bool _AddSpaces = false)
    {
        if (_AddSpaces) EditorGUILayout.Space();
        GUI.color = _color;
        EditorGUILayout.HelpBox(_heading, MessageType.None);
        GUI.color = defBackgroundColor;
        if (_AddSpaces) EditorGUILayout.Space();
    }


}
