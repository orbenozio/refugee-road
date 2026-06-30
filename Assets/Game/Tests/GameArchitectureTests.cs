using System.Linq;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace Crossroads.Game.RefugeeRoad.Tests
{
    // Per-project architecture invariant (M7): this game depends on the engine + UI, and neither the engine
    // nor UI depend back on the game.
    public sealed class GameArchitectureTests
    {
        [Test]
        public void Game_DependsOn_EngineAndUI_OneDirectional()
        {
            var asms = CompilationPipeline.GetAssemblies(AssembliesType.Player);
            var game = asms.FirstOrDefault(a => a.name == "Crossroads.Game.RefugeeRoad");
            Assert.IsNotNull(game, "Crossroads.Game.RefugeeRoad assembly not found");

            var refs = game.assemblyReferences.Select(r => r.name).ToList();
            Assert.Contains("Crossroads.Engine", refs, "game must depend on Engine");
            Assert.Contains("Crossroads.UI", refs, "game must depend on UI");

            foreach (var name in new[] { "Crossroads.Engine", "Crossroads.UI" })
            {
                var layer = asms.FirstOrDefault(a => a.name == name);
                Assert.IsNotNull(layer, name + " assembly not found (engine package not resolved?)");
                Assert.IsFalse(layer.assemblyReferences.Any(r => r.name.StartsWith("Crossroads.Game")),
                    name + " must not reference any Game assembly (one-directional dependency, M7)");
            }
        }
    }
}
