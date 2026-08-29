using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Armor;

[AutoloadEquip(EquipType.Legs)]
public class AncientLeggings : ModItem
{
    public static readonly int MoveSpeedBonus = 5;

    public override LocalizedText Tooltip =>
        base.Tooltip.WithFormatArgs(MoveSpeedBonus);

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.defense = 5;
    }

    public override void UpdateEquip(Player player)
    {
        player.moveSpeed += MoveSpeedBonus / 100f;
    }
}