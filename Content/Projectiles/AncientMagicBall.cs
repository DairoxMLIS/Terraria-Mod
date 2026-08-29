using Dairox_Mod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Projectiles;

public class AncientMagicBall : ModProjectile
{
    private const int MaxPenetrate = 3;
    private const int TimeLeft = 600;

    private const float GravityIncrease = 0.1f;
    private const float VelocityDamping = 0.75f;

    public override void SetDefaults()
    {
        Projectile.width = 6;
        Projectile.height = 6;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = MaxPenetrate;
        Projectile.timeLeft = TimeLeft;
        Projectile.scale = 2f;
    }

    public override void AI()
    {
        UpdateGravity();
        SpawnDust();
        UpdateLighting();
    }

    private void UpdateGravity()
    {
        Projectile.velocity.Y += Projectile.ai[0];
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

    private void UpdateLighting()
    {
        Lighting.AddLight(
            Projectile.position,
            0.6f,
            0.6f,
            0.7f
        );
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.penetrate--;

        if (Projectile.penetrate <= 0)
        {
            Projectile.Kill();
            return false;
        }

        Projectile.ai[0] += GravityIncrease;

        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X;

        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y;

        Projectile.velocity *= VelocityDamping;

        SoundEngine.PlaySound(
            SoundID.Item10,
            Projectile.position
        );

        return false;
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

    public override void OnHitNPC(
        NPC target,
        NPC.HitInfo hit,
        int damageDone)
    {
        Projectile.ai[0] += GravityIncrease;
        Projectile.velocity *= VelocityDamping;
    }
}