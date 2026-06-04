using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillTreeLinksUI : MonoBehaviour
{
    public SkillTreeManager skillTreeManager;
    public RectTransform skillButtonParent; // Même parent que pour les boutons
    public GameObject linePrefab; // Un prefab avec Image (UI), ancré centre-gauche

    private List<GameObject> lines = new List<GameObject>();

    public void DrawLinks()
    {
        // Nettoie les anciennes lignes
        foreach (var l in lines)
            Destroy(l);
        lines.Clear();

        var visibleSkills = skillTreeManager.GetVisibleSkills();
        // Dictionnaire pour retrouver le RectTransform de chaque skill
        Dictionary<SkillData, RectTransform> nodeRects = new Dictionary<SkillData, RectTransform>();
        foreach (Transform child in skillButtonParent)
        {
            var btn = child.GetComponent<SkillButtonUI>();
            if (btn != null && btn.skill != null)
                nodeRects[btn.skill] = child.GetComponent<RectTransform>();
        }

        foreach (var node in visibleSkills)
        {
            if (!nodeRects.ContainsKey(node)) continue;
            var rectA = nodeRects[node];
            foreach (var child in node.children)
            {
                if (!visibleSkills.Contains(child) || !nodeRects.ContainsKey(child)) continue;
                var rectB = nodeRects[child];
                // Calcul direction
                Vector2 posA = rectA.anchoredPosition;
                Vector2 posB = rectB.anchoredPosition;
                Vector2 dir = (posB - posA).normalized;
                // Point sur le bord du parent
                Vector2 halfA = rectA.sizeDelta * 0.5f;
                Vector2 edgeA = posA + new Vector2(dir.x * halfA.x, dir.y * halfA.y);
                // Point sur le bord de l'enfant
                Vector2 halfB = rectB.sizeDelta * 0.5f;
                Vector2 edgeB = posB - new Vector2(dir.x * halfB.x, dir.y * halfB.y);
                DrawLine(edgeA, edgeB);
            }
        }
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        var dir = end - start;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var lineObj = Instantiate(linePrefab, skillButtonParent);
        var rect = lineObj.GetComponent<RectTransform>();
        rect.anchoredPosition = start;
        rect.sizeDelta = new Vector2(length, 6f); // 6 pixels d'épaisseur
        rect.pivot = new Vector2(0, 0.5f);
        rect.localRotation = Quaternion.Euler(0, 0, angle);
        lines.Add(lineObj);
    }
}
