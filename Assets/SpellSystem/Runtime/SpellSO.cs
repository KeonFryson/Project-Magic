using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// A spell = Element + Delivery + a list of Modifiers.
    [CreateAssetMenu(menuName = "Spells/Spell", fileName = "NewSpell")]
    public class SpellSO : ScriptableObject
    {
        public string displayName = "New Spell";
        public Sprite icon;
        public ElementSO element;
        public DeliverySO delivery;
        public int maxModifiers = 4;
        public List<ModifierSO> modifiers = new List<ModifierSO>();

        public CompiledSpell Compile()
        {
            if (delivery == null) return null;
            var c = new CompiledSpell { source = this, element = element, delivery = delivery, stats = delivery.baseStats.Clone() };

            if (element != null)
            {
                c.stats.damage *= element.damageMultiplier;
                c.onHit.AddRange(element.defaultOnHit);
            }
            for (int i = 0; i < modifiers.Count && i < maxModifiers; i++)
                if (modifiers[i] != null) modifiers[i].Apply(c.stats, c.onHit);

            c.stats.projectileCount = Mathf.Max(1, c.stats.projectileCount);
            c.stats.cooldown = Mathf.Max(0.05f, c.stats.cooldown);
            return c;
        }

        [ContextMenu("Log Compiled Stats")]
        void LogStats()
        {
            var c = Compile();
            if (c == null) { Debug.LogWarning("Spell has no delivery."); return; }
            Debug.Log(displayName + "\n" + JsonUtility.ToJson(c.stats, true));
        }
    }
}
