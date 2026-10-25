using System.Collections;
using UnityEngine;

/// Preview tool: drop on an empty GameObject in a 2D scene (with Bloom on) and press Play.
/// It cycles through every element and effect type at this object's position.
public class SpellVFXDemo : MonoBehaviour
{
    public float interval = 1.4f;

    static readonly string[] Elements = { "Fire", "Ice", "Lightning", "Poison" };
    static readonly string[] Types = { "Projectile", "Impact", "Beam", "Slash", "Explosion" };
    string label = "";

    IEnumerator Start()
    {
        while (true)
        {
            foreach (var e in Elements)
            foreach (var t in Types)
            {
                var prefab = Resources.Load<GameObject>($"SpellVFX/{t}_{e}");
                label = $"{t}_{e}" + (prefab ? "" : "  (not built yet: Tools > Spell VFX > Build All Prefabs)");
                if (prefab)
                {
                    var go = Instantiate(prefab, transform.position, transform.rotation);
                    if (t == "Beam") go.GetComponent<BeamVFX>().duration = interval * 0.8f;
                    else if (t == "Projectile") StartCoroutine(Fly(go));
                }
                yield return new WaitForSeconds(interval);
            }
        }
    }

    IEnumerator Fly(GameObject go)
    {
        float t = 0f;
        while (go && t < interval * 0.9f)
        {
            go.transform.position += transform.right * 6f * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (go) Destroy(go);
    }

    void OnGUI() { GUI.Label(new Rect(12, 10, 600, 30), label); }
}
