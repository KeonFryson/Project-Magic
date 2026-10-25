using System.Collections;
using UnityEngine;

namespace SpellSystem
{
    /// Area blast at the aim point (clamped to baseStats.range from the caster).
    [CreateAssetMenu(menuName = "Spells/Delivery/Explosion (Area)", fileName = "ExplosionDelivery")]
    public class ExplosionDelivery : DeliverySO
    {
        public bool atCursor = true;
        public float delay = 0f;

        public override IEnumerator Cast(SpellContext ctx)
        {
            var s = ctx.stats;
            Vector2 p;
            if (atCursor)
            {
                Vector2 to = ctx.targetPoint - ctx.origin;
                Vector2 d = to.sqrMagnitude > 0.0001f ? to.normalized : ctx.direction;
                p = ctx.origin + d * Mathf.Min(to.magnitude, s.range);
            }
            else p = ctx.origin + ctx.direction * s.range;

            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (ctx.caster == null) yield break;

            float radius = s.radius * s.size;
            SpellUtil.SpawnVFX(ctx.element, VFXKind.Explosion, p, 0f, radius / 4.5f);
            SpellUtil.DamageArea(ctx, p, radius, 1f, HitFlags.Effects);
        }
    }
}
