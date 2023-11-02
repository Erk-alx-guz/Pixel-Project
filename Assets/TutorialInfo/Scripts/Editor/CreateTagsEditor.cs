//using UnityEngine;
//using UnityEditor;

//public class CreateTagsEditor : EditorWindow
//{
//    [MenuItem("Tools/Create Tags")]
//    public static void ShowWindow()
//    {
//        GetWindow(typeof(CreateTagsEditor));
//    }

//    private void OnGUI()
//    {
//        if (GUILayout.Button("Create Tags"))
//        {
//            CreateTags();
//        }
//    }

//    private void CreateTags()
//    {
//        for (int i = 0; i < 1600; i++)
//        {
//            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
//            SerializedProperty tagsProp = tagManager.FindProperty("tags");

//            bool tagExists = false;
//            for (int j = 0; j < tagsProp.arraySize; j++)
//            {
//                SerializedProperty t = tagsProp.GetArrayElementAtIndex(j);
//                if (t.stringValue.Equals(i.ToString()))
//                {
//                    tagExists = true;
//                    break;
//                }
//            }

//            if (!tagExists)
//            {
//                if (i < 10)
//                {
//                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
//                    SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
//                    newTag.stringValue = '0' + i.ToString();
//                    tagManager.ApplyModifiedProperties();
//                }
//                else
//                {
//                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
//                    SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
//                    newTag.stringValue = i.ToString();
//                    tagManager.ApplyModifiedProperties();
//                }
//            }
//        }

//        EditorUtility.SetDirty(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//        Debug.Log("Tags created successfully.");
//    }
//}
