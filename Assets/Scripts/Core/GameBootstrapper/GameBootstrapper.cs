using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> systems; // assign in inspector

    private List<IGameSystem> initializedSystems = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var gameSystems = new List<IGameSystem>();

        foreach (var mb in systems)
        {
            if (mb is IGameSystem system)
            {
                gameSystems.Add(system);
            }
            else
            {
                Debug.LogError($"{mb.name} does not implement IGameSystem but is in Bootstrapper!");
            }
        }

        if (gameSystems.Count == 0)
        {
            Debug.LogError("No valid IGameSystem found in Bootstrapper!");
            return;
        }

        gameSystems = gameSystems
            .OrderBy(s => s.InitializePriority)
            .ToList();

        // Initialize all systems in priority order
        foreach (var system in gameSystems)
        {
            if (system == null)
            {
                Debug.LogError("Null system in Bootstrapper!");
                continue;
            }

            Debug.Log($"Initializing: {system.GetType().Name}");
            system.Initialize();
            initializedSystems.Add(system);
        }

        // Post-initialize all systems
        foreach (var system in gameSystems)
        {
            Debug.Log($"PostInitialize: {system.GetType().Name}");
            system.PostInitialize();
        }

        // Verify critical systems are ready
        if (GameManager.Instance == null)
        {
            Debug.LogError("[GameBootstrapper] GameManager not initialized properly!");
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[GameBootstrapper] SaveManager not initialized properly!");
        }
    }

    private void OnValidate()
    {
        systems = systems
            .Where(s => s != null)
            .OrderBy(s =>
            {
                if (s is IGameSystem gs)
                    return gs.InitializePriority;
                return int.MaxValue;
            })
            .ToList();
    }
}