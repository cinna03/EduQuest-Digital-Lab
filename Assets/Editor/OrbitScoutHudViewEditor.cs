using OrbitScout.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitScoutHudView))]
public class OrbitScoutHudViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var view = (OrbitScoutHudView)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("UI hierarchy shortcuts", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Main menu"))
                OrbitScoutUiEditSession.FocusPanel(view, OrbitScoutUiEditSession.UiPanel.MainMenu);
            if (GUILayout.Button("Level select"))
                OrbitScoutUiEditSession.FocusPanel(view, OrbitScoutUiEditSession.UiPanel.LevelSelect);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Play HUD"))
                OrbitScoutUiEditSession.FocusPanel(view, OrbitScoutUiEditSession.UiPanel.PlayHud);
            if (GUILayout.Button("End screen"))
                OrbitScoutUiEditSession.FocusPanel(view, OrbitScoutUiEditSession.UiPanel.EndScreen);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Show all panels"))
                OrbitScoutUiEditSession.ShowAllPanels(view);
            if (GUILayout.Button("Frame in Scene"))
                OrbitScoutHudEditModeLayout.FrameHudForEditing(view);
        }
    }
}

[CustomEditor(typeof(OrbitScoutUiEditAnchor))]
public class OrbitScoutUiEditAnchorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var anchor = (OrbitScoutUiEditAnchor)target;
        OrbitScoutHudView view = anchor.HudView != null
            ? anchor.HudView
            : Object.FindAnyObjectByType<OrbitScoutHudView>(FindObjectsInactive.Include);

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Edit UI under this object: " + OrbitScoutUiEditSceneOrganizer.UiRootName + " → OrbitScoutHud → panels.",
            MessageType.Info);

        if (GUILayout.Button("Prepare scene for UI editing"))
            OrbitScoutUiEditSceneOrganizer.PrepareSceneForUiHierarchyEditing(frameMenu: true);
    }
}
