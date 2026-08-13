using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;
namespace Dairox_Mod.Content.Projectiles
{
    public class AncientBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;      // Проходит через 3 врагов
            Projectile.timeLeft = 600;     // Живет 10 секунд
            Projectile.aiStyle = ProjAIStyleID.Arrow; // Использует ИИ стрелы
            Projectile.arrow = true;       // Считается стрелой
            Projectile.alpha = 0;        // Делает спрайт почти прозрачным!
        }

        public override void AI()
        {
            // Светящийся шлейф (секрет эффекта пульсирующего лука)
            if (Projectile.alpha < 170) // Начинаем рисовать шлейф после небольшой задержки[reference:5]
            {
                Vector2 step = Projectile.velocity / 10f;
                for (int i = 0; i < 10; i++)
                {
                    // Создаем частицы пыли (Dust)
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.position - step * i, // Позиция позади снаряда
                        206, // ID пыли. 206 = светящаяся, похожая на лазер[reference:6]
                        Vector2.Zero,                   // Скорость
                        Projectile.alpha,               // Прозрачность
                        default,
                        1.5f // Масштаб
                    );
                    dust.noGravity = true; // Пыль не падает
                }
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                }
            }

            // Постепенно уменьшаем прозрачность (эффект появления)[reference:7]
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0;
                }
            }
        }
    }
}