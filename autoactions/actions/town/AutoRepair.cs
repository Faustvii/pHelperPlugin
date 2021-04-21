namespace Turbo.plugins.patrick.autoactions.actions.town
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using parameters;
    using Plugins;
    using util.diablo;
    using util.thud;

    public class AutoRepair : AbstractAutoAction
    {
        public override string tooltip => "Will automatically repair at the blacksmith.";

        public override string GetAttributes() => "";

        public override long minimumExecutionDelta => 2000;

        public override List<AbstractParameter> GetParameters()
        {
            return new List<AbstractParameter>();
        }

        private bool hasRepaired;

        public override bool Applicable(IController hud)
        {
            if (!hud.Game.Me.IsInTown)
                return false;

            var blacksmithOpen = hud.Render.IsUiElementVisible(UiPathConstants.Blacksmith.UNIQUE_PAGE);

            if (!blacksmithOpen)
            {
                hasRepaired = false;
                return false;
            }

            if (!AnyDamagedItems(hud))
            {
                return false;
            }

            return true;
        }

        public override void Invoke(IController hud)
        {
            if (hasRepaired)
                return;
            var goldCostElement = hud.Render.GetOrRegisterAndGetUiElement(UiPathConstants.Blacksmith.REPAIR_ALL);
            hud.Render.WaitForVisiblityAndClickOrAbortHotkeyEvent(UiPathConstants.Blacksmith.REPAIR_PAGE, 500);
            var goldCost = GetGoldCost(goldCostElement);
            if (goldCost > hud.Game.Me.Materials.Gold)
            {
                // We can't afford to repair, let's not try and end up in a repair loop.
                return;
            }

            hasRepaired = true;

            hud.Render.WaitForVisiblityAndClickOrAbortHotkeyEvent(UiPathConstants.Blacksmith.REPAIR_ALL, 250);
        }

        private static long GetGoldCost(IUiElement repairAll)
        {
            var goldCostText = repairAll?.ReadText(System.Text.Encoding.ASCII, true);
            var extractedGoldCost = GoldRegex.Match(goldCostText);
            long.TryParse(extractedGoldCost.Value?.Trim(), out var goldCost);
            return goldCost;
        }

        private static Regex GoldRegex = new Regex(@".(\d+).");

        private bool AnyDamagedItems(IController hud)
        {
            var equippedItems = hud.Game.Items.Where(i => i.Location >= ItemLocation.Head && i.Location <= ItemLocation.Neck);

            var statItems = equippedItems.Select(x => new
            {
                ItemName = x.FullNameEnglish,
                    StatList = x.StatList.Where(s => s.Id.Contains("Durability"))
            });

            foreach (var statItem in statItems)
            {
                var currentDurability = statItem.StatList.FirstOrDefault(i => i.Id.Contains("Durability_Cur"))?.DoubleValue;
                var maxDurability = statItem.StatList.FirstOrDefault(i => i.Id.Contains("Durability_Max"))?.DoubleValue;
                if (currentDurability != maxDurability)
                    hud.TextLog.Log("repair", $"{statItem.ItemName} {currentDurability}/{maxDurability}");

            }

            foreach (var item in equippedItems)
            {
                var currentDurability = item.StatList.FirstOrDefault(i => i.Id.Contains("Durability_Cur"))?.DoubleValue;
                var maxDurability = item.StatList.FirstOrDefault(i => i.Id.Contains("Durability_Max"))?.DoubleValue;
                if (currentDurability != maxDurability)
                    return true; // Return as soon as we find just one item damaged
            }
            return false;
        }
    }
}