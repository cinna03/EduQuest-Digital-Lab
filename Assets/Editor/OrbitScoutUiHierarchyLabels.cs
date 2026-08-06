using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class OrbitScoutUiHierarchyLabels
{
    static readonly Color PanelColor = new Color(0.45f, 0.85f, 1f, 0.85f);
    static readonly Color ButtonColor = new Color(0.55f, 0.95f, 0.65f, 0.85f);
    static readonly Color TextColor = new Color(1f, 0.88f, 0.45f, 0.85f);

    static OrbitScoutUiHierarchyLabels()
    {
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += DrawHierarchyLabel;
    }

    static void DrawHierarchyLabel(EntityId entityId, Rect selectionRect)
    {
        GameObject go = EditorUtility.EntityIdToObject(entityId) as GameObject;
        if (go == null)
            return;

        Transform t = go.transform;
        bool underHud = IsUnderOrbitScoutHud(t);
        if (!underHud && go.name != OrbitScoutUiEditSceneOrganizer.UiRootName && go.name != "OrbitScoutHud")
            return;

        string badge = null;
        Color color = PanelColor;

        if (go.name == OrbitScoutUiEditSceneOrganizer.UiRootName)
        {
            badge = "UI";
            color = new Color(1f, 0.55f, 0.85f, 0.9f);
        }
        else if (go.name == "OrbitScoutHud")
        {
            badge = "HUD";
            color = PanelColor;
        }
        else if (go.name.EndsWith("Panel"))
        {
            badge = "Panel";
            color = PanelColor;
        }
        else if (go.name.Contains("Button") || go.GetComponent<UnityEngine.UI.Button>() != null)
        {
            badge = "Btn";
            color = ButtonColor;
        }
        else if (go.GetComponent<TMPro.TextMeshProUGUI>() != null)
        {
            badge = "Text";
            color = TextColor;
        }

        if (badge == null)
            return;

        Rect badgeRect = selectionRect;
        badgeRect.x = badgeRect.xMax - 36f;
        badgeRect.width = 34f;
        badgeRect.height -= 2f;

        EditorGUI.DrawRect(badgeRect, new Color(0f, 0f, 0f, 0.35f));
        var style = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            fontStyle = FontStyle.Bold
        };
        GUI.Label(badgeRect, badge, style);
    }

    static bool IsUnderOrbitScoutHud(Transform t)
    {
        while (t != null)
        {
            if (t.name == "OrbitScoutHud")
                return true;
            t = t.parent;
        }

        return false;
    }
}
