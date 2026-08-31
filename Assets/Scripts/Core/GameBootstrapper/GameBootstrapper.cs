using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> systems;

    private List<IGameSystem> initializedSystems = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var gameSystems = new List<IGameSystem>();

        // First, try to use Inspector-assigned systems
        foreach (var mb in systems)
        {
            if (mb is IGameSystem system)
            {
                gameSystems.Add(system);
            }
            else if (mb != null)
            {
                Debug.LogWarning($"{mb.name} does not implement IGameSystem but is in Bootstrapper!");
            }
        }

        // If no systems assigned in Inspector, auto-find them
        if (gameSystems.Count == 0)
        {
            Debug.Log("[GameBootstrapper] No systems assigned in Inspector. Auto-detecting...");

            // Find all MonoBehaviours that implement IGameSystem
            var foundSystems = FindObjectsOfType<MonoBehaviour>(true)
                .Where(mb => mb is IGameSystem)
                .Cast<IGameSystem>()
                .ToList();

            if (foundSystems.Count > 0)
            {
                gameSystems.AddRange(foundSystems);
                Debug.Log($"[GameBootstrapper] Auto-detected {foundSystems.Count} systems");

                // Optional: Auto-populate the Inspector list for future use
                systems = foundSystems.Cast<MonoBehaviour>().ToList();
            }
            else
            {
                Debug.LogError("No valid IGameSystem found in Bootstrapper!");
                return;
            }
        }

        // Rest of your initialization code...
        gameSystems = gameSystems
            .OrderBy(s => s.InitializePriority)
            .ToList();

        // Initialize all systems in priority order
        foreach (var system in gameSystems)
        {
            if (system == null) continue;

            Debug.Log($"Initializing: {system.GetType().Name}");
            system.Initialize();
            initializedSystems.Add(system);
        }

        // Post-initialize all systems
        foreach (var system in gameSystems)
        {
            if (system == null) continue;

            Debug.Log($"PostInitialize: {system.GetType().Name}");
            system.PostInitialize();
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