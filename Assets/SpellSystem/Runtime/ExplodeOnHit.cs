using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/On-Hit/Explode", fileName = "Explode")]
    public class ExplodeOnHit : OnHitEffectSO
    {
        public float radius = 1.6f;
        public float damageMultiplier = 0.5f;

        public override void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 direction)
        {
            SpellUtil.SpawnVFX(ctx.element, VFXKind.Explosion, point, 0f, radius / 4.5f);
            // No effects/chain on the blast itself, so explosions can't trigger more explosions.
            SpellUtil.DamageArea(ctx, point, radius, damageMultiplier, HitFlags.None);
        }
    }
}
