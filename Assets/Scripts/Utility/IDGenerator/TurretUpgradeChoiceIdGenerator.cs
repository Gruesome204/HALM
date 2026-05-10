#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

public static class TurretUpgradeChoiceIdGenerator
{
    [MenuItem("Tools/Turret Upgrades/Generate Missing Option IDs")]
    public static void GenerateMissingIds()
    {
        // Find all TurretUpgradeChoiceSO assets
        string[] guids = AssetDatabase.FindAssets("t:TurretUpgradeChoiceSO");
        int totalGenerated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TurretUpgradeChoiceSO choiceSO = AssetDatabase.LoadAssetAtPath<TurretUpgradeChoiceSO>(path);
            if (choiceSO == null) continue;

            bool dirty = false;

            foreach (var option in choiceSO.options)
            {
                if (string.IsNullOrEmpty(option.optionId))
                {
                    option.optionId = Guid.NewGuid().ToString();
                    totalGenerated++;
                    dirty = true;
                    Debug.Log($"Generated ID for option '{option.name}' in '{choiceSO.name}'");
                }
            }

            if (dirty)
            {
                EditorUtility.SetDirty(choiceSO);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Finished generating IDs. Total generated: {totalGenerated}");
    }
}
#endif
