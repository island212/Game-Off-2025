using UnityEngine;

public static class SceneConstants
{
    // Main scenes
    public const string MENU = "StartScene";
    public const string PLAYGROUND = "Playground";
    
    public static readonly string[] ALL_SCENES = 
    {
        MENU,
        PLAYGROUND
    };

    public static class BuildIndex
    {
        public const int MENU = 0;
        public const int PLAYGROUND = 1;
    }
}