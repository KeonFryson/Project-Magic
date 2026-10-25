using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Held beam that follows the caster's aim. Damage in baseStats is damage PER SECOND.
    [CreateAssetMenu(menuName = "Spells/Delivery/Beam", fileName = "BeamDelivery")]
    public class BeamDelivery : DeliverySO
    {
        [Tooltip("How often on-hit effects (burn, knockback...) are re-applied while the beam stays on a target.")]
        public float effectInterval = 0.5f;

        public override float Duration(SpellStats s) { return s.lifetime; }

        public override IEnumerator Cast(SpellContext ctx)
        {
            var s = ctx.stats;
            var caster = ctx.caster;

            BeamVFX beam = null;
            var prefab = ctx.element != null ? ctx.element.GetVFX(VFXKind.Beam) : null;
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                beam = go.GetComponent<BeamVFX>();
                var tint = go.GetComponent<SpellVFX>();
                if (tint != null) tint.SetTint(ctx.element.color);
                if (beam != null)
                {
                    beam.width = 0.7f * s.size;
                    beam.duration = s.lifetime;
                    beam.SetBeam(ctx.origin, ctx.origin + ctx.direction * s.range);
                }
            }

            float t = 0f, tickT = 0f, fxT = effectInterval;
            var targets = new List<RaycastHit2D>();

            while (t < s.lifetime && caster != null)
            {
                float dt = Time.deltaTime;
                t += dt; tickT += dt; fxT += dt;

                Vector2 origin = caster.muzzle.position;
                Vector2 dir = caster.AimDirection;
                Vector2 end = origin + dir * s.range;
                int mask = caster.targetMask | caster.obstacleMask;
                var hits = Physics2D.CircleCastAll(origin, 0.15f * s.size, dir, s.range, mask);

                targets.Clear();
                int pierce = s.pierce;
                foreach (var h in hits)
                {
                    if (h.collider.transform.IsChildOf(caster.transform)) continue;
                    var dmg = h.collider.GetComponentInParent<IDamageable>();
                    if (dmg != null)
                    {
                        if (dmg.IsDead || dmg.Team == caster.team) continue;
                        targets.Add(h);
                        if (pierce-- <= 0) { end = h.point; break; }
                    }
                    else if (SpellUtil.IsObstacle(caster, h.collider)) { end = h.point; break; }
                }

                if (beam != null) beam.SetBeam(origin, end);

                if (tickT >= s.tickInterval)
                {
                    tickT -= s.tickInterval;
                    bool applyFx = fxT >= effectInterval;
                    if (applyFx) fxT = 0f;
                    foreach (var h in targets)
                        SpellUtil.Hit(ctx, h.collider, h.point, dir, s.tickInterval, applyFx ? HitFlags.Effects : HitFlags.None);
                }
                yield return null;
            }
            if (beam != null) beam.StopBeam();
        }
    }
}
