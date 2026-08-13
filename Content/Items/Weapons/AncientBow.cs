using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;


namespace Dairox_Mod.Content.Items.Weapons
{
    public class AncientBow : ModItem
    {
        public override void SetDefaults()
        {
            // Базовая настройка лука: время между выстрелами 20, скорость снаряда 10
            Item.DefaultToBow(20, 10f, true); // true = авто-огонь

            Item.damage = 20;                 // Урон
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.rare = ItemRarityID.LightRed; // Редкость
            Item.value = Item.sellPrice(gold: 5);
            Item.useAmmo = AmmoID.Arrow;
            

            Item.scale = 1.5f;// Использует стрелы как боеприпасы

            // ВАЖНО: указываем, что лук будет стрелять нашим кастомным снарядом
            Item.shoot = ModContent.ProjectileType<Projectiles.AncientBolt>();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            float xOffset = -25f * player.direction;
            float yOffset = 0f;

            Vector2 offset = new Vector2(xOffset, yOffset).RotatedBy(player.itemRotation);
            player.itemLocation += offset;
        }

        // Этот метод позволяет заменить стандартную стрелу на нашу
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Принудительно меняем тип снаряда на наш
            type = ModContent.ProjectileType<Projectiles.AncientBolt>();
        }
    }
}