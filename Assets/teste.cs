using UnityEngine;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.Traits;

public class MeleeDamageFullDebugger : MonoBehaviour
{
    private Health _health;

    private void Start()
    {
        _health = GetComponent<Health>();
        Debug.Log($"[DEBUG] Health encontrado: {_health != null}");

        if (_health != null)
            Debug.Log($"[DEBUG] HP: {_health.Value} | Invincible: {_health.Invincible}");

        // Escuta impacto NO OBJETO (antes do dano)
        EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, object, Collider>(
            gameObject, "OnObjectImpact", OnImpact);

        // Escuta dano real no Health
        EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(
            gameObject, "OnHealthDamage", OnHealthDamage);
    }

    private void OnImpact(float amount, Vector3 pos, Vector3 force,
                          GameObject attacker, object attackerObj, Collider hitCollider)
    {
        Debug.Log($"[IMPACT] Objeto atingido! Dano: {amount} | Atacante: {attacker?.name}");
    }

    private void OnHealthDamage(float amount, Vector3 pos, Vector3 force,
                                 GameObject attacker, Collider hitCollider)
    {
        Debug.Log($"[HEALTH] Dano aplicado: {amount} | HP restante: {_health?.Value}");
    }

    private void OnDestroy()
    {
        EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, object, Collider>(
            gameObject, "OnObjectImpact", OnImpact);
        EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(
            gameObject, "OnHealthDamage", OnHealthDamage);
    }
}