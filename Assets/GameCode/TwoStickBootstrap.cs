using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    public sealed class TwoStickBootstrap
    {
        public static TwoStickExampleSettings Settings;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            var settingsGO = GameObject.Find("Settings");
            Settings = settingsGO?.GetComponent<TwoStickExampleSettings>();

            World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();
        }

        public static void NewGame()
        {
            var player = Object.Instantiate(Settings.PlayerPrefab);
            player.GetComponent<Position2D>().Value = new float2(0, 0);
            player.GetComponent<Heading2D>().Value = new float2(0, 1);
        }
    }
}
