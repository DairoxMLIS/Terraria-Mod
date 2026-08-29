using Dairox_Mod.Content.Projectiles;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Weapons;

public class AncientStaff : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToStaff(
            ModContent.ProjectileType<AncientMagicBall>(),
            10,
            15,
            7
        );

        Item.UseSound = SoundID.Item20;

        Item.SetWeaponValues(20, 5);
        Item.SetShopValues(ItemRarityColor.Green2, 10000);
    }
}