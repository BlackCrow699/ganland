using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillData))]
public sealed class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillData skill = (SkillData)target;
        if (skill.EnsureMigrated())
        {
            EditorUtility.SetDirty(skill);
        }

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manaCost"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("unlockLevel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aiPriority"));

        SerializedProperty effects = serializedObject.FindProperty("effects");
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        int removeIndex = -1;
        for (int i = 0; i < effects.arraySize; i++)
        {
            SerializedProperty effect = effects.GetArrayElementAtIndex(i);
            if (DrawEffect(effect, i))
            {
                removeIndex = i;
            }
            EditorGUILayout.Space(2);
        }

        if (removeIndex >= 0)
        {
            effects.DeleteArrayElementAtIndex(removeIndex);
        }

        if (GUILayout.Button("Add Effect"))
        {
            effects.arraySize++;
            ResetEffect(effects.GetArrayElementAtIndex(effects.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();

        ValidateTargeting(skill);
    }

    bool DrawEffect(SerializedProperty effect, int index)
    {
        SerializedProperty targetMode = effect.FindPropertyRelative("targetMode");
        SerializedProperty effectType = effect.FindPropertyRelative("effectType");
        SerializedProperty stat = effect.FindPropertyRelative("stat");
        SerializedProperty valueMode = effect.FindPropertyRelative("valueMode");
        SerializedProperty value = effect.FindPropertyRelative("value");
        SerializedProperty duration = effect.FindPropertyRelative("duration");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("#" + index, GUILayout.Width(28));
        EditorGUILayout.PropertyField(targetMode, new GUIContent("目标"));
        bool remove = GUILayout.Button("X", GUILayout.Width(24));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(effectType, new GUIContent("效果"));

        SkillEffectType type = (SkillEffectType)effectType.enumValueIndex;
        if (type == SkillEffectType.BuffStat || type == SkillEffectType.DebuffStat)
        {
            EditorGUILayout.PropertyField(stat, new GUIContent("属性"));
            EditorGUILayout.PropertyField(value, new GUIContent("百分比"));
            EditorGUILayout.PropertyField(duration, new GUIContent("持续回合"));
        }
        else if (type == SkillEffectType.Damage)
        {
            EditorGUILayout.PropertyField(value, new GUIContent("倍率"));
        }
        else
        {
            EditorGUILayout.PropertyField(valueMode, new GUIContent("数值模式"));
            EditorGUILayout.PropertyField(value, new GUIContent("数值"));
        }

        EditorGUILayout.EndVertical();
        return remove;
    }

    void ResetEffect(SerializedProperty effect)
    {
        effect.FindPropertyRelative("targetMode").enumValueIndex = (int)SkillTargetMode.SingleEnemy;
        effect.FindPropertyRelative("effectType").enumValueIndex = (int)SkillEffectType.Damage;
        effect.FindPropertyRelative("stat").enumValueIndex = (int)StatType.Attack;
        effect.FindPropertyRelative("valueMode").enumValueIndex = (int)ValueMode.Multiplier;
        effect.FindPropertyRelative("value").floatValue = 1f;
        effect.FindPropertyRelative("duration").intValue = 3;
    }

    void ValidateTargeting(SkillData skill)
    {
        if (skill.effects == null) return;

        bool hasEnemyTarget = false;
        bool hasAllyTarget = false;

        for (int i = 0; i < skill.effects.Count; i++)
        {
            SkillEffect effect = skill.effects[i];
            if (effect == null) continue;
            if (effect.targetMode == SkillTargetMode.SingleEnemy) hasEnemyTarget = true;
            else if (effect.targetMode == SkillTargetMode.SingleAlly) hasAllyTarget = true;
        }

        if (hasEnemyTarget && hasAllyTarget)
        {
            EditorGUILayout.HelpBox("v1 暂不支持同一技能同时选敌人和队友。", MessageType.Warning);
        }
    }
}
