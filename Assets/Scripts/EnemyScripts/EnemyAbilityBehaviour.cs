using UnityEngine;
using System.Linq;

public class EnemyAbilityBehaviour : MonoBehaviour
{
    public EnemyAbilityBlueprint[] abilities;
    public Transform target;

    private void Start()
    {
        AbilityManager.Instance.Register(gameObject, abilities);
    }

    private void OnDestroy()
    {
        AbilityManager.Instance.Unregister(gameObject);
    }

    private void Update()
    {
        //var runtimeList = AbilityManager.Instance.GetAbilities(gameObject);
        //if (runtimeList == null) return;

        //// Pick best ability by priority
        //var best = runtimeList
        //    .Where(a => a.CanUse(gameObject, target?.gameObject))
        //    .OrderByDescending(a => a.ability.priority)
        //    .FirstOrDefault();

        //if (best != null)
        //{
        //    best.Use(gameObject, target?.gameObject);
        //}
    }
}
