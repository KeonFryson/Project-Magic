using UnityEngine;

namespace SpellSystem
{
    /// Sample controls: mouse aims, hold left click to cast, keys 1-8 pick a spell, WASD moves.
    [RequireComponent(typeof(SpellCaster))]
    public class SpellCasterInput : MonoBehaviour
    {
        public Camera cam;
        public float moveSpeed = 6f;
        public bool allowMovement = true;

        SpellCaster caster;

        void Awake()
        {
            caster = GetComponent<SpellCaster>();
            if (cam == null) cam = Camera.main;
        }

        void Update()
        {
            if (cam == null) return;

            Vector2 sp = SpellInput.PointerScreen();
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, Mathf.Abs(cam.transform.position.z)));
            caster.AimAt(w);

            if (SpellInput.Fire()) caster.TryCast();

            for (int i = 0; i < caster.loadout.Count && i < 8; i++)
                if (SpellInput.NumberDown(i)) caster.Select(i);

            if (allowMovement)
                transform.position += (Vector3)(SpellInput.Move() * moveSpeed * Time.deltaTime);
        }

        void OnGUI()
        {
            if (caster == null) return;
            var cur = caster.Current;
            string name = cur != null ? cur.displayName : "(no spell)";
            GUI.Label(new Rect(12, 10, 700, 24), "Spell [" + (caster.selected + 1) + "]: " + name + "     Mana: " + Mathf.RoundToInt(caster.mana));
            GUI.Label(new Rect(12, 30, 700, 24), "Aim: mouse   Cast: hold left click   Switch: 1-8   Move: WASD");
        }
    }
}
