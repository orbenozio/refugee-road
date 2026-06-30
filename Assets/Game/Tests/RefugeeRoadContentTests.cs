using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Crossroads.Engine;
using Crossroads.UI;

namespace Crossroads.Game.RefugeeRoad.Tests
{
    // The real Refugee Road journey end-to-end through the same EventEngine + MapGraph: loads the real
    // story.json + map.json from disk, navigates the map to the goal, and asserts a win - no UI (map
    // screens are LATER, spec 9.6). Paths are local to this game project.
    public sealed class RefugeeRoadContentTests
    {
        private const string Dir = "Assets/Game/Content/";

        [Test]
        public void RefugeeRoad_FullJourney_ReachesHaven_AndWins()
        {
            var storyAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + "story.json");
            var mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + "map.json");
            var resources = AssetDatabase.LoadAssetAtPath<ResourceSet>(Dir + "resources.asset");
            Assert.IsNotNull(storyAsset, "RefugeeRoad story.json missing");
            Assert.IsNotNull(mapAsset, "RefugeeRoad map.json missing");
            Assert.IsNotNull(resources, "RefugeeRoad resources.asset missing");

            var story = StoryLoader.Parse(storyAsset.text);
            var map = MapLoader.Parse(mapAsset.text);
            Assert.IsEmpty(System.Linq.Enumerable.Where(StoryValidator.Validate(story, resources),
                i => i.Severity == IssueSeverity.Error), "RefugeeRoad content must validate clean");

            var engine = new EventEngine(story, resources, new MapGraph(story, map), 1);
            GameOverInfo? over = null;
            engine.OnGameOver += i => over = i;

            Assert.AreEqual("border", engine.Current.Id, "journey starts at the map start");

            // A live path along the map: border -> checkpoint -> forest -> town -> haven, with choices that preserve resources.
            engine.Resolve(ChoiceSide.Left); engine.EnterNode("checkpoint");
            engine.Resolve(ChoiceSide.Left); engine.EnterNode("forest");
            engine.Resolve(ChoiceSide.Left); engine.EnterNode("town");
            engine.Resolve(ChoiceSide.Left); engine.EnterNode("haven");

            Assert.AreEqual(GameStatus.GameOver, engine.Status);
            Assert.IsTrue(over.HasValue && over.Value.Reason == GameOverReason.ReachedGoal,
                "reaching haven is a victory");
            StringAssert.Contains("made it", over.Value.Text, "win shows the reachedGoal ending text");
        }
    }
}
