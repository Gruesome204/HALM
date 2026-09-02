    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "TurretUpgradeChoice", menuName = "Game/Turret/Upgrade Choice")]
    public class TurretUpgradeChoiceSO : ScriptableObject
    {
        [Header("Turret Configuration")]
        public TurretType turretType;
        public List<int> triggerLevels;

        [Header("Upgrade Options")]
        [Tooltip("Available upgrade options for this turret at trigger levels")]
        public List<UpgradeOption> options;

        [System.Serializable]
        public class UpgradeOption
        {
            [Tooltip("Unique identifier for this upgrade option")]
            public string optionId;

            [Tooltip("Display name shown in UI")]
            public string displayName;  // More explicit naming

            [TextArea(3, 5)]
            [Tooltip("Description shown in UI")]
            public string description;

            [Tooltip("Icon displayed in UI")]
            public Sprite icon;

            [Tooltip("Modifier applied when this upgrade is chosen")]
            public TurretModifier modifier;

            // Helper method to validate option
            public bool IsValid()
            {
                return !string.IsNullOrEmpty(optionId) &&
                       !string.IsNullOrEmpty(displayName) &&
                       modifier != null;
            }
        }
}
