using UnityEngine;

[CreateAssetMenu(fileName = "SettingDataSO", menuName = "Scriptable Objects/SettingDataSO")]
public class SettingDataSO : ScriptableObject
{
    public string localSelected;

    public int musicTrack;
    public float musicVolume;
    public float soundVolume;
}
