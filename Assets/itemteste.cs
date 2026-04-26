using UnityEngine;
using System.Reflection;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.ThirdPersonController.Items;

public class WeaponHitboxDebugger : MonoBehaviour
{
    private MeleeWeapon _meleeWeapon;
    private ThirdPersonMeleeWeaponProperties _props;
    private FieldInfo _attackingField;
    private FieldInfo _attackTimeField;
    private bool _wasAttacking = false;

    private void Start()
    {
        _meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        _props = GetComponentInChildren<ThirdPersonMeleeWeaponProperties>();

        if (_meleeWeapon == null) { Debug.LogError("[WEAPON] MeleeWeapon não encontrado!"); return; }
        if (_props == null) { Debug.LogError("[WEAPON] Props não encontrado!"); return; }

        var type = typeof(MeleeWeapon);
        _attackingField = type.GetField("m_Attacking", BindingFlags.NonPublic | BindingFlags.Instance);
        _attackTimeField = type.GetField("m_AttackTime", BindingFlags.NonPublic | BindingFlags.Instance);

        if (_attackingField == null) Debug.LogError("[WEAPON] Campo m_Attacking não encontrado via Reflection!");

        // Log das hitboxes
        var hitboxes = _props.Hitboxes;
        Debug.Log($"[WEAPON] Hitboxes: {hitboxes?.Length ?? 0}");
        for (int i = 0; i < hitboxes.Length; i++)
        {
            var hb = hitboxes[i];
            var col = hb.Collider;
            float radius = col is SphereCollider sc ? sc.radius : -1f;
            Debug.Log($"[WEAPON] Hitbox[{i}]: {col?.name ?? "NULL"} | " +
                      $"IsTrigger: {col?.isTrigger} | " +
                      $"Radius: {radius} | " +
                      $"Layer: {LayerMask.LayerToName(col?.gameObject.layer ?? 0)}");
        }

        // Log de scale — sobe toda a hierarquia da hitbox
        if (_props.Hitboxes.Length > 0)
        {
            var hb = _props.Hitboxes[0];
            var t = hb.Transform;

            if (t != null)
            {
                Debug.Log($"[SCALE] Hitbox '{t.name}' | " +
                          $"localScale={t.localScale} | " +
                          $"lossyScale={t.lossyScale}");

                var parent = t.parent;
                int depth = 0;
                while (parent != null && depth < 20)
                {
                    Debug.Log($"[SCALE] PAI[{depth}]: '{parent.name}' | " +
                              $"localScale={parent.localScale}");
                    parent = parent.parent;
                    depth++;
                }
            }
        }
    }

    private void Update()
    {
        if (_meleeWeapon == null || _attackingField == null) return;

        bool isAttacking = (bool)_attackingField.GetValue(_meleeWeapon);

        if (isAttacking != _wasAttacking)
        {
            float attackTime = (float)_attackTimeField.GetValue(_meleeWeapon);
            Debug.Log($"[WEAPON] m_Attacking mudou para: {isAttacking} | AttackTime: {attackTime:F3}");
            _wasAttacking = isAttacking;

            if (isAttacking && _props.Hitboxes.Length > 0)
            {
                var hb = _props.Hitboxes[0];
                if (hb.Collider is SphereCollider sc)
                {
                    var center = hb.Transform.TransformPoint(sc.center);
                    Debug.Log($"[WEAPON] Hitbox center world pos: {center} | " +
                              $"Radius world: {sc.radius * hb.Transform.lossyScale.x:F3}");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_props?.Hitboxes == null) return;
        foreach (var hb in _props.Hitboxes)
        {
            if (hb.Collider == null) continue;
            Gizmos.color = Color.red;
            if (hb.Collider is SphereCollider sc)
                Gizmos.DrawWireSphere(
                    hb.Transform.TransformPoint(sc.center),
                    sc.radius * hb.Transform.lossyScale.x
                );
        }
    }
}