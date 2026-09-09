using System.Collections.Generic;

public static class RuntimeDataHelpers
{
    public static List<SkillDefinition> ToRuntimeSkills(List<SkillData> source)
    {
        var result = new List<SkillDefinition>();
        if (source == null) return result;
        for (int i = 0; i < source.Count; i++)
            if (source[i] != null) result.Add(source[i].ToRuntimeDefinition());
        return result;
    }
}
