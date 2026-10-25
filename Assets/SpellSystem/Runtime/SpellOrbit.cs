using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    public class SpellOrbit : MonoBehaviour
    {
        SpellContext ctx;
        float angle, angSpeed, orbitRadius, life, hitRadius;
        GameObject vfx;
        readonly Dictionary<IDamageable, float> lastHit = new Dictionary<IDamageable, float>();

        public static void Create(SpellContext c, float startAngle)
        {
            var go = new GameObject("SpellOrbit");
            var o = go.AddComponent<SpellOrbit>();
            o.ctx = c;
            o.angle = startAngle;
            o.angSpeed = c.stats.speed;
            o.orbitRadius = c.stats.range;
            o.life = c.stats.lifetime;
            o.hitRadius = 0.4f * c.stats.size;
            go.transform.position = c.caster.transform.position;
            o.vfx = SpellUtil.SpawnVFX(c.element, VFXKind.Projectile, go.transform.position, 0f, c.stats.size, go.transform);
        }

        void Update()
        {
            if (ctx.caster == null) { Finish(); return; }
            life -= Time.deltaTime;
            if (life <= 0f) { Finish(); return; }

            angle += angSpeed * Time.deltaTime;
            Vector2 center = ctx.caster.transform.position;
            Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * orbitRadius;
            transform.position = center + offset;

            var cols = Physics2D.OverlapCircleAll(transform.position, hitRadius, ctx.caster.targetMask);
            foreach (var col in cols)
            {
                var d = col.GetComponentInParent<IDamageable>();
                if (d == null) continue;
                float last;
                if (lastHit.TryGetValue(d, out last) && Time.time - last < 0.4f) continue;
                Vector2 p = col.ClosestPoint(transform.position);
                var res = SpellUtil.Hit(ctx, col, p, offset.normalized);
                if (res != null) lastHit[res] = Time.time;
            }
        }

        void Finish()
        {
            if (vfx != null)
            {
                vfx.transform.SetParent(null, true);
                foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                Destroy(vfx, 1.2f);
            }
            Destroy(gameObject);
        }
    }
}
