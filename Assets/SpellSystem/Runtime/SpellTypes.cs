using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    public enum VFXKind { Projectile, Impact, Beam, Slash, Explosion }
    public enum StatusKind { DamageOverTime, Slow, Stun, Mark }

    [System.Flags]
    public enum HitFlags { None = 0, Impact = 1, Chain = 2, Effects = 4, All = 7 }

    /// Anything a spell can damage. Health implements this; implement it on your own enemies too.
    public interface IDamageable
    {
        int Team { get; }
        bool IsDead { get; }
        void TakeDamage(float amount, ElementSO element, GameObject source);
    }

    /// Every number a spell can have. Delivery gives the base values; element and modifiers change them.
    [System.Serializable]
    public class SpellStats
    {
        [Header("Core")]
        public float damage = 10f;       // beams: damage PER SECOND
        public float cooldown = 0.5f;
        public float manaCost = 5f;
        public float size = 1f;

        [Header("Movement (projectile / orbit)")]
        public float speed = 12f;        // orbit: degrees per second
        public float lifetime = 2f;      // beam / orbit: duration
        public float homing = 0f;        // degrees per second of steering
        public float spread = 0f;        // total fan angle when firing several

        [Header("Counts")]
        public int projectileCount = 1;
        public int pierce = 0;
        public int bounces = 0;
        public int splitCount = 0;
        public int chainCount = 0;

        [Header("Reach")]
        public float range = 8f;         // beam length, slash reach, cast distance, orbit radius
        public float radius = 2f;        // explosion radius (before size)
        public float tickInterval = 0.2f;// beam damage ticks
        public float chainRange = 5f;

        public SpellStats Clone() { return (SpellStats)MemberwiseClone(); }
    }

    /// "If the target has <tag>, take extra damage" (e.g. Fire vs Chilled).
    [System.Serializable]
    public class ElementInteraction
    {
        public string targetHasTag = "Chilled";
        public float damageMultiplier = 1.5f;
        public bool consumeTag = false;
    }

    /// A spell after element + modifiers have been applied.
    public class CompiledSpell
    {
        public SpellSO source;
        public ElementSO element;
        public DeliverySO delivery;
        public SpellStats stats;
        public List<OnHitEffectSO> onHit = new List<OnHitEffectSO>();
    }

    /// Everything a delivery needs to run one cast.
    public class SpellContext
    {
        public SpellCaster caster;
        public CompiledSpell spell;
        public SpellStats stats;
        public Vector2 origin, direction, targetPoint;

        public ElementSO element { get { return spell != null ? spell.element : null; } }

        public SpellContext Clone()
        {
            var c = (SpellContext)MemberwiseClone();
            c.stats = stats.Clone();
            return c;
        }
    }
}
