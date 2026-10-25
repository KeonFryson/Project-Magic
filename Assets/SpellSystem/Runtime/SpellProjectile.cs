using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Runtime projectile. Handles homing, piercing, bouncing, splitting. Chaining happens in SpellUtil.Hit.
    public class SpellProjectile : MonoBehaviour
    {
        SpellContext ctx;
        Vector2 dir;
        float speed, life, radius, retarget;
        int pierceLeft, bouncesLeft;
        readonly HashSet<IDamageable> hitSet = new HashSet<IDamageable>();
        GameObject vfx;
        Collider2D homingTarget;

        public static SpellProjectile Launch(SpellContext ctx, Vector2 pos, Vector2 dir)
        {
            var go = new GameObject("SpellProjectile");
            go.transform.position = pos;
            var p = go.AddComponent<SpellProjectile>();
            p.Init(ctx, dir);
            return p;
        }

        void Init(SpellContext c, Vector2 d)
        {
            ctx = c;
            dir = d.normalized;
            var s = c.stats;
            speed = s.speed; life = s.lifetime;
            pierceLeft = s.pierce; bouncesLeft = s.bounces;
            radius = 0.25f * s.size;
            vfx = SpellUtil.SpawnVFX(c.element, VFXKind.Projectile, transform.position, SpellUtil.Angle(dir), s.size, transform);
        }

        void Update()
        {
            if (ctx.caster == null) { End(transform.position, false); return; }

            float dt = Time.deltaTime;
            life -= dt;
            if (life <= 0f) { End(transform.position, false); return; }

            if (ctx.stats.homing > 0f) Steer(dt);

            float dist = speed * dt;
            int mask = ctx.caster.targetMask | ctx.caster.obstacleMask;
            var hits = Physics2D.CircleCastAll(transform.position, radius, dir, dist, mask);

            foreach (var h in hits)
            {
                var col = h.collider;
                if (col.transform.IsChildOf(ctx.caster.transform)) continue;

                var target = col.GetComponentInParent<IDamageable>();
                if (target != null)
                {
                    if (hitSet.Contains(target)) continue;
                    var res = SpellUtil.Hit(ctx, col, h.point, dir);
                    if (res == null) continue;
                    hitSet.Add(res);
                    if (pierceLeft > 0) { pierceLeft--; continue; }
                    if (bouncesLeft > 0) { Bounce(h); return; }
                    End(h.point, true);
                    return;
                }
                if (SpellUtil.IsObstacle(ctx.caster, col))
                {
                    if (bouncesLeft > 0) { Bounce(h); return; }
                    End(h.point, false);
                    return;
                }
            }

            transform.position += (Vector3)(dir * dist);
        }

        void Steer(float dt)
        {
            retarget -= dt;
            if (retarget <= 0f || homingTarget == null)
            {
                retarget = 0.15f;
                homingTarget = SpellUtil.FindNearest(transform.position, 8f, ctx.caster, hitSet);
            }
            if (homingTarget == null) return;
            Vector2 to = (Vector2)homingTarget.bounds.center - (Vector2)transform.position;
            float ang = Vector2.SignedAngle(dir, to);
            float step = Mathf.Clamp(ang, -ctx.stats.homing * dt, ctx.stats.homing * dt);
            dir = SpellUtil.Rotate(dir, step);
        }

        void Bounce(RaycastHit2D h)
        {
            dir = Vector2.Reflect(dir, h.normal).normalized;
            transform.position = h.point + h.normal * (radius * 1.05f);
            bouncesLeft--;
        }

        void End(Vector2 pos, bool impacted)
        {
            if (impacted && ctx.stats.splitCount > 0) Split(pos);

            if (vfx != null)
            {
                vfx.transform.SetParent(null, true);
                foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                Destroy(vfx, 1.2f);
            }
            Destroy(gameObject);
        }

        void Split(Vector2 pos)
        {
            int n = ctx.stats.splitCount;
            float baseAngle = SpellUtil.Angle(dir);
            for (int i = 0; i < n; i++)
            {
                var c = ctx.Clone();
                var s = c.stats;
                s.damage *= 0.5f; s.size *= 0.6f; s.lifetime *= 0.6f;
                s.splitCount = 0; s.chainCount = 0; s.projectileCount = 1; s.pierce = 0; s.bounces = 0;

                Vector2 d = SpellUtil.Rotate(Vector2.right, baseAngle + 360f / n * i + 180f / n);
                var child = Launch(c, pos + d * 0.3f, d);
                child.hitSet.UnionWith(hitSet);
            }
        }
    }
}
