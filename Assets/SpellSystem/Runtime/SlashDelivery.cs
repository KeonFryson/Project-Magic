using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Instant melee arc in front of the caster. baseStats.range = reach.
    [CreateAssetMenu(menuName = "Spells/Delivery/Slash", fileName = "SlashDelivery")]
    public class SlashDelivery : DeliverySO
    {
        public float arcDegrees = 120f;
        public float hitDelay = 0.06f;

        public override IEnumerator Cast(SpellContext ctx)
        {
            var s = ctx.stats;
            Vector2 c = ctx.origin, dir = ctx.direction;
            float reach = s.range * s.size;

            SpellUtil.SpawnVFX(ctx.element, VFXKind.Slash, c, SpellUtil.Angle(dir), reach / 2.5f);
            if (hitDelay > 0f) yield return new WaitForSeconds(hitDelay);
            if (ctx.caster == null) yield break;

            var cols = Physics2D.OverlapCircleAll(c, reach, ctx.caster.targetMask);
            var done = new HashSet<IDamageable>();
            foreach (var col in cols)
            {
                var d = col.GetComponentInParent<IDamageable>();
                if (d == null || done.Contains(d)) continue;
                Vector2 p = col.ClosestPoint(c);
                Vector2 to = p - c;
                if (to.sqrMagnitude > 0.0001f && Vector2.Angle(dir, to) > arcDegrees * 0.5f) continue;
                if (SpellUtil.Hit(ctx, col, p, dir) != null) done.Add(d);
            }
        }
    }
}
