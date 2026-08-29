using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Weapons;

public class AncientBow : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToBow(20, 10f, true);

        Item.damage = 20;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 3f;

        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 5);

        Item.useAmmo = AmmoID.Arrow;
        Item.shoot =
            ModContent.ProjectileType<Projectiles.AncientBolt>();

        Item.scale = 1.5f;
    }

    public override void UseStyle(
        Player player,
        Rectangle heldItemFrame)
    {
        Vector2 offset = new Vector2(
            -25f * player.direction,
            0f
        ).RotatedBy(player.itemRotation);

        player.itemLocation += offset;
    }

    public override void ModifyShootStats(
        Player player,
        ref Vector2 position,
        ref Vector2 velocity,
        ref int type,
        ref int damage,
        ref float knockback)
    {
        type =
            ModContent.ProjectileType<Projectiles.AncientBolt>();
    }
}