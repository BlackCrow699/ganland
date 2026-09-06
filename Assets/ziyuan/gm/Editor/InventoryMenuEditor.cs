#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryMenuController))]
public class InventoryMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        InventoryMenuController menu = (InventoryMenuController)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Create / Find Inventory UI In Scene"))
        {
            menu.SendMessage("BuildMissingUI", SendMessageOptions.DontRequireReceiver);
            EditorUtility.SetDirty(menu);
            if (menu.gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
            }
        }
    }
}
#endif
