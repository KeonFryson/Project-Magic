using System.Collections;
using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/Delivery/Projectile", fileName = "ProjectileDelivery")]
    public class ProjectileDelivery : DeliverySO
    {
        public override IEnumerator Cast(SpellContext ctx)
        {
            var s = ctx.stats;
            int n = Mathf.Max(1, s.projectileCount);
            float total = n > 1 ? (s.spread > 0f ? s.spread : 12f * (n - 1)) : 0f;
            for (int i = 0; i < n; i++)
            {
                float offset = n > 1 ? Mathf.Lerp(-total * 0.5f, total * 0.5f, i / (float)(n - 1)) : 0f;
                SpellProjectile.Launch(ctx, ctx.origin, SpellUtil.Rotate(ctx.direction, offset));
            }
            yield break;
        }
    }
}
