using UnityEditor;

/// <summary>
/// Legacy menu entry — syncs both SampleScene and OrbitScout_EditorTest.
/// </summary>
public static class OrbitScoutSampleSceneMenuBackground
{
    [MenuItem("Orbit Scout/Apply Menu Background To Sample Scene")]
    public static void ApplyMenuBackgroundToSampleScene()
    {
        OrbitScoutGameSceneSync.SyncHudAndMenuBackgroundInAllGameScenes();
    }
}
