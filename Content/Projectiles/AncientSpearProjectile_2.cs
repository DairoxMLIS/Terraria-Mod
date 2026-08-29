using Dairox_Mod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Projectiles;

public class AncientSpearProjectile_2 : ModProjectile
{
    private const int FrameCount = 2;
    private const int FrameSpeed = 8;

    private const int MaxPenetrate = 3;
    private const int TimeLeft = 600;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 25;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;

        Projectile.penetrate = MaxPenetrate;
        Projectile.timeLeft = TimeLeft;
    }

    public override void AI()
    {
        UpdateAnimation();
        UpdateLighting();
        SpawnDust();
        UpdateRotation();
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

    private void UpdateLighting()
    {
        Lighting.AddLight(
            Projectile.Center,
            0.4f,
            0.4f,
            0.7f
        );
    }

    private void SpawnDust()
    {
        if (!Main.rand.NextBool(10))
            return;

        Dust.NewDust(
            Projectile.position + Projectile.velocity,
            Projectile.width,
            Projectile.height,
            ModContent.DustType<AncientSparkle>(),
            Projectile.velocity.X * 0.5f,
            Projectile.velocity.Y * 0.5f
        );
    }

    private void UpdateRotation()
    {
        Projectile.rotation =
            Projectile.velocity.ToRotation();

        if (Projectile.velocity.X < 0f)
        {
            Projectile.spriteDirection = -1;
            Projectile.rotation += MathHelper.Pi;
        }
        else
        {
            Projectile.spriteDirection = 1;
        }
    }

    public override void OnKill(int timeLeft)
    {
        SpawnDeathDust();

        SoundEngine.PlaySound(
            SoundID.Item25,
            Projectile.position
        );
    }

    private void SpawnDeathDust()
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