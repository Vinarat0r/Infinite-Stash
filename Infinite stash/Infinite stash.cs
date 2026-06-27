using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services;
using Range = SemanticVersioning.Range;

namespace InfiniteStashServer
{
    public record ModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "com.vinarator.infinitestash";
        public override string Name { get; init; } = "Infinite Stash";
        public override string Author { get; init; } = "Vinarator";
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
        public override Range SptVersion { get; init; } = new("~4.0.13");
        public override string License { get; init; } = "MIT";
        public override bool? IsBundleMod { get; init; } = false;
        public override Dictionary<string, Range>? ModDependencies { get; init; } = null;
        public override string? Url { get; init; } = null;
        public override List<string>? Contributors { get; init; } = null;
        public override List<string>? Incompatibilities { get; init; } = null;
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
    public class Mod : IOnLoad
    {
        private readonly DatabaseService _databaseService;

        public Mod(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task OnLoad()
        {
            Console.WriteLine("[InfiniteStash] Starting server-side stash modification...");

            var itemsDb = _databaseService.GetTables().Templates.Items;

            if (itemsDb == null)
            {
                Console.WriteLine("[InfiniteStash] ERROR: Items database is null!");
                return;
            }

            const string stashParentId = "566abbb64bdc2d144c8b457d";
            bool foundAny = false;

            foreach (var kvp in itemsDb)
            {
                var item = kvp.Value;
                if (item.Parent == stashParentId && item.Properties?.Grids != null)
                {
                    var firstGrid = item.Properties.Grids.FirstOrDefault();
                    if (firstGrid != null && firstGrid.Properties != null)
                    {
                        int? oldCellsV = firstGrid.Properties.CellsV;
                        firstGrid.Properties.CellsV = 10000;
                        Console.WriteLine($"[InfiniteStash] Successfully changed stash {kvp.Key} ({item.Name}) CellsV from {oldCellsV} to {firstGrid.Properties.CellsV}");
                        foundAny = true;
                    }
                }
            }

            if (foundAny)
            {
                Console.WriteLine("[InfiniteStash] Stash size modification complete.");
            }
            else
            {
                Console.WriteLine("[InfiniteStash] ERROR: Could not find any stash items in database!");
            }

            await Task.CompletedTask;
        }
    }
}