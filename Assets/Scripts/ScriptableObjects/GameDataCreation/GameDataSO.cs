using UnityEngine;

[CreateAssetMenu(fileName = "New GameDataSO", menuName = "Game/GameData/New GameDataSO")]
public class GameDataSO : ScriptableObject
{

    public enum Class
    {
        Mechanic,
        Test
    }
    [SerializeField] private Class currentClass;

}
