using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Armor;

[AutoloadEquip(EquipType.Body)]
public class AncientBreastplate : ModItem
{
    public static readonly int MaxManaIncrease = 20;
    public static readonly int MaxMinionIncrease = 1;

    public override LocalizedText Tooltip =>
        base.Tooltip.WithFormatArgs(
            MaxManaIncrease,
            MaxMinionIncrease
        );

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.defense = 6;
    }

    public override void UpdateEquip(Player player)
    {
        player.buffImmune[BuffID.OnFire] = true;
        player.statManaMax2 += MaxManaIncrease;
        player.maxMinions += MaxMinionIncrease;
    }
}