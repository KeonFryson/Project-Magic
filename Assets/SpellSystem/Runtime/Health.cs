using UnityEngine;

namespace SpellSystem
{
    /// Minimal health for enemies / dummies. Flashes in the element's color when hit.
    public class Health : MonoBehaviour, IDamageable
    {
        public float maxHealth = 50f;
        public int team = 1;
        public bool destroyOnDeath = true;
        public float current;
        public event System.Action<Health> Died;

        SpriteRenderer sr;
        Color baseColor, flashColor = Color.white;
        float flash;

        public int Team { get { return team; } }
        public bool IsDead { get { return current <= 0f; } }

        void Awake()
        {
            current = maxHealth;
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) baseColor = sr.color;
        }

        public void TakeDamage(float amount, ElementSO element, GameObject source)
        {
            if (IsDead) return;
            current -= amount;
            flash = 1f;
            flashColor = element ? Color.Lerp(element.color, Color.white, 0.4f) : Color.white;
            if (current <= 0f)
            {
                if (Died != null) Died(this);
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        void Update()
        {
            if (!sr) return;
            flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 8f);
            sr.color = Color.Lerp(baseColor, flashColor, flash);
        }
    }
}
