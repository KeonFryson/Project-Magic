using UnityEngine;

namespace SpellSystem
{
    public class LingerZone : MonoBehaviour
    {
        SpellContext ctx;
        float radius, life, tick, scale, timer;

        public static void Spawn(SpellContext c, Vector2 pos, float radius, float duration, float tick, float damageScale)
        {
            var go = new GameObject("LingerZone");
            go.transform.position = pos;
            var z = go.AddComponent<LingerZone>();
            z.ctx = c.Clone();
            z.radius = radius; z.life = duration; z.tick = tick; z.scale = damageScale;
        }

        void Update()
        {
            if (ctx.caster == null) { Destroy(gameObject); return; }
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = tick;
            Vector2 p = transform.position;
            SpellUtil.SpawnVFX(ctx.element, VFXKind.Impact, p, 0f, radius * 0.5f);
            SpellUtil.DamageArea(ctx, p, radius, scale, HitFlags.Effects);
        }
    }
}
