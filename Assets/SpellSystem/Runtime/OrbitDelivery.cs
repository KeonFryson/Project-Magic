using System.Collections;
using UnityEngine;

namespace SpellSystem
{
    /// Orbs that circle the caster. speed = degrees/sec, range = orbit radius, lifetime = duration.
    [CreateAssetMenu(menuName = "Spells/Delivery/Orbit", fileName = "OrbitDelivery")]
    public class OrbitDelivery : DeliverySO
    {
        public override float Duration(SpellStats s) { return s.lifetime; }

        public override IEnumerator Cast(SpellContext ctx)
        {
            int n = Mathf.Max(1, ctx.stats.projectileCount);
            for (int i = 0; i < n; i++) SpellOrbit.Create(ctx, 360f / n * i);
            yield break;
        }
    }
}
