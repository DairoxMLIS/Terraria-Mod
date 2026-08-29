using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Armor;

[AutoloadEquip(EquipType.Head)]
public class AncientHelmet : ModItem
{
    public static readonly int AdditiveGenericDamageBonus = 20;

    public static LocalizedText SetBonusText { get; private set; }

    public override void SetStaticDefaults()
    {
        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;

        SetBonusText = this
            .GetLocalization("SetBonus")
            .WithFormatArgs(AdditiveGenericDamageBonus);
    }

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.defense = 5;
    }

    public override bool IsArmorSet(
        Item head,
        Item body,
        Item legs)
    {
        return body.type ==
                   ModContent.ItemType<AncientBreastplate>() &&
               legs.type ==
                   ModContent.ItemType<AncientLeggings>();
    }

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = SetBonusText.Value;

        player.GetDamage(DamageClass.Generic) +=
            AdditiveGenericDamageBonus / 100f;
    }
}