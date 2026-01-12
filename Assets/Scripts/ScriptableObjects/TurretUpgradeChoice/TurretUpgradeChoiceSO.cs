    using UnityEngine;
    using System.Collections.Generic;
    using UnityEditor;
    using System;

    [CreateAssetMenu(fileName = "TurretUpgradeChoice", menuName = "Game/Turret/Upgrade Choice")]
    public class TurretUpgradeChoiceSO : ScriptableObject
    {
        public TurretType turretType;
        public List<int> triggerLevels;

        public List<UpgradeOption> options;

        [System.Serializable]
        public class UpgradeOption
        {
            public string optionId;
            public string name;
            [TextArea] public string description;
            public Sprite icon; // for UI
            public TurretModifier modifier;
        }
    }
