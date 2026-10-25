using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// Put on the player (or an enemy). Holds the spell loadout, mana, and cooldowns.
    /// Drive it with AimAt(worldPoint) and TryCast(); SpellCasterInput does this from mouse/keyboard.
    public class SpellCaster : MonoBehaviour
    {
        public int team = 0;
        public List<SpellSO> loadout = new List<SpellSO>();
        public int selected;

        [Header("Mana")]
        public float maxMana = 100f;
        public float manaRegen = 15f;
        public float mana;

        [Header("Targeting")]
        [Tooltip("Layers that contain things spells can hit (enemies).")]
        public LayerMask targetMask = ~0;
        [Tooltip("Layers that block spells (walls). Leave empty if you have none.")]
        public LayerMask obstacleMask;

        public Transform muzzle;
        public event System.Action<SpellSO> OnCast;

        readonly Dictionary<SpellSO, float> readyAt = new Dictionary<SpellSO, float>();

        public Vector2 AimPoint { get; private set; }
        public Vector2 AimDirection { get { return muzzle != null ? (Vector2)muzzle.right : Vector2.right; } }
        public SpellSO Current { get { return loadout.Count > 0 ? loadout[Mathf.Clamp(selected, 0, loadout.Count - 1)] : null; } }

        void Awake()
        {
            mana = maxMana;
            if (muzzle == null)
            {
                var go = new GameObject("Muzzle");
                go.transform.SetParent(transform, false);
                muzzle = go.transform;
            }
        }

        void Update() { mana = Mathf.Min(maxMana, mana + manaRegen * Time.deltaTime); }

        public void Select(int index)
        {
            if (index >= 0 && index < loadout.Count) selected = index;
        }

        public void AimAt(Vector2 worldPoint)
        {
            AimPoint = worldPoint;
            Vector2 d = worldPoint - (Vector2)transform.position;
            if (d.sqrMagnitude > 0.0001f) muzzle.right = d.normalized;
        }

        public float CooldownRemaining(SpellSO spell)
        {
            float t;
            return readyAt.TryGetValue(spell, out t) ? Mathf.Max(0f, t - Time.time) : 0f;
        }

        public bool TryCast(SpellSO spell = null)
        {
            if (spell == null) spell = Current;
            if (spell == null || CooldownRemaining(spell) > 0f) return false;

            var compiled = spell.Compile();
            if (compiled == null || mana < compiled.stats.manaCost) return false;

            mana -= compiled.stats.manaCost;
            float busy = Mathf.Max(compiled.stats.cooldown, compiled.delivery.Duration(compiled.stats));
            readyAt[spell] = Time.time + busy;

            var ctx = new SpellContext
            {
                caster = this,
                spell = compiled,
                stats = compiled.stats.Clone(),
                origin = muzzle.position,
                direction = AimDirection,
                targetPoint = AimPoint
            };
            StartCoroutine(compiled.delivery.Cast(ctx));
            if (OnCast != null) OnCast(spell);
            return true;
        }
    }
}
