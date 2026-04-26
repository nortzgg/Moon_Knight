using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Physics Trigger")]
    [Version(1,0,0)]
    [Category("Physics/Physics Trigger")]
    [Description("Executed when a game object enters or exits the Trigger collider (MORE OPTIMIZED THAN THE DEFAULT GC2 TRIGGER ENTER AND EXIT)")]
    [Parameter("Fire When", "Defines whether to fire the event on Enter, Exit or Both")]
    [Parameter("Target Mode", "Defines how to group the target objects: by Rigidbody, Collider or Transform Root")]
    [Parameter("Layers", "The physics layers that the other object must be in to trigger the event")]
    [Parameter("Tag Filter", "An optional tag filter that the other object must match to trigger the event")]
    [Parameter("Object Filter", "A filter that the other object must match to trigger the event")]
    [Keywords("Pass", "Through", "Touch", "Collision", "Collide","Physics", "Trigger")]
    [Image(typeof(IconTriggerEnter), ColorTheme.Type.Green)]
    [Serializable]
    public class EventPhysicsTrigger : Event // generic, not TEventPhysics
    {
        public enum FireWhen { Enter, Exit, Both }
        public enum TargetMode
        {
            Rigidbody, // default: groups by rigidbody
            Collider,          // per-collider
            TransformRoot        // groups by root transform
        }

        [Header("Behavior")]
        [SerializeField] private FireWhen m_FireWhen = FireWhen.Both;
        [SerializeField] private TargetMode m_TargetMode = TargetMode.Rigidbody;

        [Header("Filters")]
        [SerializeField] private LayerMask m_Layers = ~0;
        [SerializeField] private string m_OptionalTagFilter = "";

        [Header("Object Filter")]
        [SerializeField] private CompareGameObjectOrAny m_ObjectFilter = new CompareGameObjectOrAny();

        // Tracks "root targets" currently inside (dedup multi-collider bodies)
        private readonly HashSet<int> m_Inside = new HashSet<int>(64);

        // Reused Args for CompareGameObjectOrAny (like TEventPhysics)
        private Args m_ArgsForMatch;

        // -------- Lifecycle --------

        protected override void OnAwake(Trigger trigger)
        {
            base.OnAwake(trigger);

            // Create Args once for Match()
            m_ArgsForMatch = new Args(trigger.gameObject);

            // Ensure the TRIGGER has a kinematic RB so triggers work with CC/no-RB movers
            var rb = trigger.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = trigger.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.hideFlags = HideFlags.HideInInspector;
            }

            // Ensure there is a trigger collider
            var col = trigger.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            m_Inside.Clear();
        }

        protected override void OnDestroy(Trigger trigger)
        {
            m_Inside.Clear();
            base.OnDestroy(trigger);
        }

        // -------- Helpers --------

        private bool PassesLayerAndTag(GameObject go)
        {
            if (((1 << go.layer) & m_Layers) == 0) return false;
            if (!string.IsNullOrEmpty(m_OptionalTagFilter) && !go.CompareTag(m_OptionalTagFilter)) return false;
            return true;
        }

        private bool Match(GameObject go)
        {
            // Same semantics as TEventPhysics.Match
            return m_ObjectFilter.Match(go, m_ArgsForMatch);
        }

        private GameObject ResolveTarget(GameObject other)
        {
            if (other == null) return null;

            switch (m_TargetMode)
            {
                case TargetMode.Rigidbody:
                    // Prefer attached rigidbody root
                    if (other.TryGetComponent<Collider>(out var col) && col.attachedRigidbody != null)
                        return col.attachedRigidbody.gameObject;
                    if (other.TryGetComponent<Rigidbody>(out var rb)) return rb.gameObject;
                    return other; // fallback

                case TargetMode.TransformRoot:
                    return other.transform.root.gameObject;

                case TargetMode.Collider:
                default:
                    return other;
            }
        }

        private bool MarkEntered(GameObject root)
        {
            int id = root.GetInstanceID();
            if (m_Inside.Contains(id)) return false;
            m_Inside.Add(id);
            return true;
        }

        private bool MarkExited(GameObject root)
        {
            int id = root.GetInstanceID();
            if (!m_Inside.Contains(id)) return false;
            m_Inside.Remove(id);
            return true;
        }

        // -------- Physics 3D --------

        protected override void OnTriggerEnter3D(Trigger trigger, Collider other)
        {
            var go = other.gameObject;

            // Early, cheap filters first:
            if (!PassesLayerAndTag(go)) return;

            var root = ResolveTarget(go);
            if (root == null) return;

            // GC2-style object filter:
            if (!Match(root)) return;

            // Always track, so Exit-only works later
            bool first = MarkEntered(root);

            if (m_FireWhen == FireWhen.Exit) return;                 // exit-only: just track
            if (!first && m_TargetMode != TargetMode.Collider) return; // dedupe unless per-collider

            GetGameObjectLastTriggerEnter.Instance = root;
            _ = trigger.Execute(root);
        }

        protected override void OnTriggerExit3D(Trigger trigger, Collider other)
        {
            var go = other.gameObject;
            if (!PassesLayerAndTag(go)) return;

            var root = ResolveTarget(go);
            if (root == null) return;

            // Must pass Match to be considered (mirrors Enter)
            if (!Match(root)) return;

            bool wasInside = MarkExited(root);
            if (!wasInside) return;

            if (m_FireWhen == FireWhen.Enter) return; // enter-only: don't execute on exit

            GetGameObjectLastTriggerEnter.Instance = root;
            _ = trigger.Execute(root);
        }

        // -------- Physics 2D (parity) --------

        protected override void OnTriggerEnter2D(Trigger trigger, Collider2D other)
        {
            var go = other.gameObject;
            if (!PassesLayerAndTag(go)) return;

            var root = ResolveTarget(go);
            if (root == null) return;
            if (!Match(root)) return;

            bool first = MarkEntered(root);

            if (m_FireWhen == FireWhen.Exit) return;
            if (!first && m_TargetMode != TargetMode.Collider) return;

            GetGameObjectLastTriggerEnter.Instance = root;
            _ = trigger.Execute(root);
        }

        protected override void OnTriggerExit2D(Trigger trigger, Collider2D other)
        {
            var go = other.gameObject;
            if (!PassesLayerAndTag(go)) return;

            var root = ResolveTarget(go);
            if (root == null) return;
            if (!Match(root)) return;

            bool wasInside = MarkExited(root);
            if (!wasInside) return;

            if (m_FireWhen == FireWhen.Enter) return;

            GetGameObjectLastTriggerEnter.Instance = root;
            _ = trigger.Execute(root);
        }

        // -------- No-op Stay --------
        protected override void OnTriggerStay3D(Trigger trigger, Collider other) { }
        protected override void OnTriggerStay2D(Trigger trigger, Collider2D other) { }
    }
}
