using Dairox_Mod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Projectiles
{
    public class AncientSpearProjectile_2 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;

            Main.projFrames[Projectile.type] = 2;
            Projectile.frame = 0;
            Projectile.frameCounter = 0;   
        }

        public override void AI()

        {
            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.7f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8) // меняем кадр каждые 8 тиков
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0; // зацикливаем
                }
            }


            if (Main.rand.NextBool(10))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Если летит влево — отражаем и разворачиваем на 180°
            if (Projectile.velocity.X < 0)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation += MathHelper.Pi;
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation += MathHelper.ToRadians(0f);
            }
        }

        

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
        }

    }
}