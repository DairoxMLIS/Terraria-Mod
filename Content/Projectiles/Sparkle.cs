using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles
{
    public class Sparkle : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 13;
            Projectile.friendly = false;
            Projectile.hostile = true;   
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;

            Main.projFrames[Projectile.type] = 4;
            Projectile.frame = 0;
            Projectile.frameCounter = 0;
        }

        

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.8f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnKill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            SoundEngine.PlaySound(SoundID.Item73, Projectile.position);
        }
    }
}