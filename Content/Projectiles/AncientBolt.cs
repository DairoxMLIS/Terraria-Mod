using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles;

public class AncientBolt : ModProjectile
{
    private const int MaxPenetrate = 3;
    private const int TimeLeft = 600;

    private const int TrailDustId = 206;
    private const int TrailLength = 10;

    private const float TrailStepMultiplier = 0.1f;
    private const float TrailScale = 1.5f;
    private const int AlphaFade = 25;

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = MaxPenetrate;
        Projectile.timeLeft = TimeLeft;

        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.arrow = true;
        Projectile.alpha = 0;
    }

    public override void AI()
    {
        SpawnTrail();
        UpdateAlpha();
    }

    private void SpawnTrail()
    {
        if (Projectile.alpha >= 170)
            return;

        Vector2 step =
            Projectile.velocity * TrailStepMultiplier;

        for (int i = 0; i < TrailLength; i++)
        {
            Dust dust = Dust.NewDustPerfect(
                Projectile.position - step * i,
                TrailDustId,
                Vector2.Zero,
                Projectile.alpha,
                default,
                TrailScale
            );

            dust.noGravity = true;
        }

        if (Main.rand.NextBool(10))
        {
            Dust.NewDust(
                Projectile.position + Projectile.velocity,
                Projectile.width,
                Projectile.height,
                ModContent.DustType<AncientSparkle>(),
                Projectile.velocity.X * 0.5f,
                Projectile.velocity.Y * 0.5f
            );
        }
    }

    private void UpdateAlpha()
    {
        if (Projectile.alpha <= 0)
            return;

        Projectile.alpha -= AlphaFade;

        if (Projectile.alpha < 0)
            Projectile.alpha = 0;
    }
}