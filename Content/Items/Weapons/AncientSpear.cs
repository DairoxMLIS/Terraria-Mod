using Dairox_Mod.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Items.Weapons
{
    public class AncientSpear : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Type] = true;
            ItemID.Sets.Spears[Type] = true;
        }

        public override void SetDefaults()
        {

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(silver: 10);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation =20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.damage = 25;
            Item.knockBack = 6.5f;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.shootSpeed = 2.3f;
            Item.shoot = ModContent.ProjectileType<AncientSpearProjectile>();
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool? UseItem(Player player)
        {
            if (!Main.dedServ && Item.UseSound.HasValue)
            {
                SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
            }

            return null;
        }
    }
}