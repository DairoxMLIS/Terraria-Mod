using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles;

public class Sparkle : ModProjectile
{
    private const int FrameCount = 4;
    private const int FrameSpeed = 8;

    private const int MaxPenetrate = 3;
    private const int TimeLeft = 600;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 13;

        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.DamageType = DamageClass.Default;

        Projectile.penetrate = MaxPenetrate;
        Projectile.timeLeft = TimeLeft;
    }

    public override void AI()
    {
        UpdateAnimation();
        UpdateRotation();
        UpdateLighting();
    }

    private void UpdateAnimation()
    {
        Projectile.frameCounter++;

        if (Projectile.frameCounter < FrameSpeed)
            return;

        Projectile.frameCounter = 0;
        Projectile.frame++;

        if (Projectile.frame >= Main.projFrames[Type])
            Projectile.frame = 0;
    }

    private void UpdateRotation()
    {
        Projectile.rotation =
            Projectile.velocity.ToRotation();
    }

    private void UpdateLighting()
    {
        Lighting.AddLight(
            Projectile.Center,
            0.8f,
            0.8f,
            0.8f
        );
    }

    public override void OnKill(int timeLeft)
    {
        SpawnDust();

        SoundEngine.PlaySound(
            SoundID.Item73,
            Projectile.position
        );
    }

    private void SpawnDust()
    {
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(
                Projectile.position + Projectile.velocity,
                Projectile.width,
                Projectile.height,
                ModContent.DustType<AncientSparkle>(),
                Projectile.oldVelocity.X * 0.5f,
                Projectile.oldVelocity.Y * 0.5f
            );
        }
    }
}