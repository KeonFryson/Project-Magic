using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Shared helpers: damage, VFX spawning, targeting, chaining.
    public static class SpellUtil
    {
        public static Vector2 Rotate(Vector2 v, float deg)
        {
            float r = deg * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        public static float Angle(Vector2 d) { return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; }

        public static bool IsObstacle(SpellCaster caster, Collider2D col)
        {
            return (caster.obstacleMask.value & (1 << col.gameObject.layer)) != 0;
        }

        // ---------------- VFX ----------------

        public static GameObject SpawnVFX(ElementSO e, VFXKind kind, Vector2 pos, float angle,
            float scale = 1f, Transform parent = null)
        {
            if (e == null) return null;
            var prefab = e.GetVFX(kind);
            if (prefab == null) return null;
            var go = UnityEngine.Object.Instantiate(prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.Euler(0f, 0f, angle));
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.localScale = Vector3.one * scale;
            var v = go.GetComponent<SpellVFX>();
            if (v != null) v.SetTint(e.color);
            return go;
        }

        public static void BeamBetween(ElementSO e, Vector2 a, Vector2 b, float width, float time)
        {
            var prefab = e != null ? e.GetVFX(VFXKind.Beam) : null;
            if (prefab == null) return;
            var go = UnityEngine.Object.Instantiate(prefab);
            var beam = go.GetComponent<BeamVFX>();
            if (beam == null) { UnityEngine.Object.Destroy(go); return; }
            beam.width = width;
            beam.duration = time;
            beam.SetBeam(a, b);
            var v = go.GetComponent<SpellVFX>();
            if (v != null) v.SetTint(e.color);
        }

        // ---------------- Damage ----------------

        public static StatusReceiver GetReceiver(Collider2D col, bool create)
        {
            var r = col.GetComponentInParent<StatusReceiver>();
            if (r != null || !create) return r;
            var d = col.GetComponentInParent<IDamageable>() as Component;
            return d != null ? d.gameObject.AddComponent<StatusReceiver>() : null;
        }

        /// Damages one target. Returns the IDamageable that was hit, or null if it wasn't a valid target.
        public static IDamageable Hit(SpellContext ctx, Collider2D col, Vector2 point, Vector2 dir,
            float damageScale = 1f, HitFlags flags = HitFlags.All)
        {
            var dmg = col.GetComponentInParent<IDamageable>();
            if (dmg == null || dmg.IsDead || dmg.Team == ctx.caster.team) return null;

            float amount = ctx.stats.damage * damageScale;

            var receiver = GetReceiver(col, false);
            if (receiver != null && ctx.element != null)
            {
                foreach (var it in ctx.element.interactions)
                {
                    if (!receiver.HasTag(it.targetHasTag)) continue;
                    amount *= it.damageMultiplier;
                    if (it.consumeTag) receiver.RemoveTag(it.targetHasTag);
                }
            }

            dmg.TakeDamage(amount, ctx.element, ctx.caster.gameObject);

            if ((flags & HitFlags.Impact) != 0)
                SpawnVFX(ctx.element, VFXKind.Impact, point, 0f, 0.5f * Mathf.Max(0.5f, ctx.stats.size));

            if ((flags & HitFlags.Effects) != 0)
                foreach (var fx in ctx.spell.onHit)
                    if (fx != null) fx.Apply(ctx, col, point, dir);

            if ((flags & HitFlags.Chain) != 0 && ctx.stats.chainCount > 0)
                Chain(ctx, point, dmg);

            return dmg;
        }

        public static void DamageArea(SpellContext ctx, Vector2 center, float radius, float damageScale, HitFlags flags)
        {
            var cols = Physics2D.OverlapCircleAll(center, radius, ctx.caster.targetMask);
            var done = new HashSet<IDamageable>();
            foreach (var c in cols)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d == null || done.Contains(d)) continue;
                Vector2 p = c.ClosestPoint(center);
                Vector2 dir = p - center;
                dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up;
                if (Hit(ctx, c, p, dir, damageScale, flags) != null) done.Add(d);
            }
        }

        // ---------------- Targeting ----------------

        public static Collider2D FindNearest(Vector2 pos, float range, SpellCaster caster, HashSet<IDamageable> exclude)
        {
            var cols = Physics2D.OverlapCircleAll(pos, range, caster.targetMask);
            Collider2D best = null;
            float bestDist = float.MaxValue;
            foreach (var c in cols)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d == null || d.IsDead || d.Team == caster.team) continue;
                if (exclude != null && exclude.Contains(d)) continue;
                float dist = ((Vector2)c.bounds.center - pos).sqrMagnitude;
                if (dist < bestDist) { bestDist = dist; best = c; }
            }
            return best;
        }

        /// Jumps to nearby enemies after a hit, drawing a lightning beam between them.
        static void Chain(SpellContext ctx, Vector2 from, IDamageable first)
        {
            var visited = new HashSet<IDamageable> { first };
            Vector2 pos = from;
            for (int i = 0; i < ctx.stats.chainCount; i++)
            {
                var next = FindNearest(pos, ctx.stats.chainRange, ctx.caster, visited);
                if (next == null) break;
                Vector2 target = next.bounds.center;
                BeamBetween(ctx.element, pos, target, 0.3f, 0.15f);
                var hit = Hit(ctx, next, target, (target - pos).normalized, 0.7f, HitFlags.Impact | HitFlags.Effects);
                visited.Add(next.GetComponentInParent<IDamageable>());
                if (hit == null) break;
                pos = target;
            }
        }
    }
}
