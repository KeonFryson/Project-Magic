using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/On-Hit/Lingering Zone", fileName = "LingerZone")]
    public class LingerZoneOnHit : OnHitEffectSO
    {
        public float radius = 1.6f;
        public float duration = 3f;
        public float tickInterval = 0.5f;
        [Tooltip("Damage per tick as a fraction of the spell's damage.")]
        public float damageMultiplier = 0.3f;

        public override void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 direction)
        {
            LingerZone.Spawn(ctx, point, radius, duration, tickInterval, damageMultiplier);
        }
    }
}
