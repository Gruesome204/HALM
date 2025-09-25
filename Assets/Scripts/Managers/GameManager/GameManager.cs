using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    void Awake() => Instance = this;
}
