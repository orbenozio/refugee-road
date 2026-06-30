using UnityEngine;
using Crossroads.Engine;
using Crossroads.UI;

namespace Crossroads.Game.RefugeeRoad
{
    // Declarative wiring only (spec 12.2): fill the shell Config (journey format) from the scene refs +
    // content and run it. The map<->card timing, plus the shared shell (intro / end / error - and the
    // optional menu/Settings when wired) live in Crossroads.UI.GameShell. Cloning = copy the folder + swap
    // Content/Map/Theme (M7).
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Content (per-game)")]
        [SerializeField] private TextAsset storyJson;
        [SerializeField] private TextAsset mapJson;     // the journey overlay
        [SerializeField] private ResourceSet resources;
        [SerializeField] private Theme theme;
        [SerializeField] private int seed = 12345;

        [Header("UI (from Crossroads.UI)")]
        [SerializeField] private CardView cardView;
        [SerializeField] private ResourceBarView resourceBar;
        [SerializeField] private SwipeInput swipeInput;
        [SerializeField] private EndScreen endScreen;
        [SerializeField] private MessageOverlay messageOverlay;
        [SerializeField] private MapView mapView;

        [Header("Opening screen")]
        [SerializeField] private string title = "Refugee Road";
        [SerializeField] [TextArea] private string intro = "Cross the border to safety. Choose your route; survive each stop.";

        private readonly GameShell _shell = new GameShell();

        private void Start()
        {
            _shell.Run(new GameShell.Config
            {
                format = GameShell.Format.Journey,
                storyJson = storyJson, mapJson = mapJson, resources = resources, theme = theme, seed = seed,
                title = title, intro = intro,
                cardView = cardView, resourceBar = resourceBar, swipeInput = swipeInput,
                endScreen = endScreen, messageOverlay = messageOverlay, mapView = mapView,
            });
        }
    }
}
