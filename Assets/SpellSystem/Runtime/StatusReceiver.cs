using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Add to anything that can be burned, slowed, stunned or knocked back.
    /// Your movement code should multiply speed by SpeedMultiplier, stop when Stunned,
    /// and add KnockVelocity to its movement.
    public class StatusReceiver : MonoBehaviour
    {
        class Active { public StatusEffectSO so; public float remaining, tick; public GameObject source; }

        readonly List<Active> active = new List<Active>();
        Vector2 knock;
        IDamageable damageable;

        public float SpeedMultiplier { get; private set; }
        public bool Stunned { get; private set; }
        public Vector2 KnockVelocity { get { return knock; } }

        void Awake() { SpeedMultiplier = 1f; damageable = GetComponent<IDamageable>(); }

        public void Apply(StatusEffectSO so, GameObject source)
        {
            if (so == null) return;
            foreach (var a in active)
                if (a.so == so) { a.remaining = so.duration; a.source = source; return; }
            active.Add(new Active { so = so, remaining = so.duration, source = source });
        }

        public bool HasTag(string tag)
        {
            foreach (var a in active) if (a.so.statusTag == tag) return true;
            return false;
        }

        public void RemoveTag(string tag)
        {
            active.RemoveAll(a => a.so.statusTag == tag);
        }

        public void Knockback(Vector2 impulse) { knock += impulse; }

        void Update()
        {
            float dt = Time.deltaTime;
            knock *= Mathf.Exp(-8f * dt);

            float slow = 0f; bool stun = false;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var a = active[i];
                a.remaining -= dt;
                switch (a.so.kind)
                {
                    case StatusKind.DamageOverTime:
                        a.tick += dt;
                        while (a.so.tickInterval > 0.01f && a.tick >= a.so.tickInterval)
                        {
                            a.tick -= a.so.tickInterval;
                            if (damageable != null && !damageable.IsDead)
                                damageable.TakeDamage(a.so.magnitude, a.so.element, a.source);
                        }
                        break;
                    case StatusKind.Slow: slow = Mathf.Max(slow, a.so.magnitude); break;
                    case StatusKind.Stun: stun = true; break;
                }
                if (a.remaining <= 0f) active.RemoveAt(i);
            }
            SpeedMultiplier = 1f - Mathf.Clamp01(slow);
            Stunned = stun;
        }
    }
}
