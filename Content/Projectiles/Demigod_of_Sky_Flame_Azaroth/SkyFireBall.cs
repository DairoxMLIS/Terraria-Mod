using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

public class SkyFireBall : ModProjectile
{
    #region Enums

    public enum WallPattern
    {
        Horizontal = 0,
        Diagonal_1 = 1,
        Diagonal_2 = 2
    }

    public enum WallSide
    {
        Left = -1,
        Right = 1
    }

    #endregion


    #region Constants

    private const int FrameCount = 4;
    private const int FrameSpeed = 5;

    private const int HitboxWidth = 40;
    private const int HitboxHeight = 40;

    private const float LightRed = 0.2f;
    private const float LightGreen = 0.7f;
    private const float LightBlue = 1f;

    private const float DustVelocityMultiplier = 0.15f;
    private const float DustMinScale = 0.7f;
    private const float DustMaxScale = 1.2f;

    #endregion


    #region Properties

    public WallPattern Pattern
    {
        get => (WallPattern)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    public WallSide Side
    {
        get => (WallSide)(int)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    #endregion


    #region Lifecycle

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.scale = 2f;

        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 500;
        Projectile.light = 0.8f;
    }

    #endregion


    #region AI

    public override void AI()
    {
        UpdateRotation();
        UpdateAnimation();
        UpdateLighting();
        SpawnDust();
    }

    private void UpdateRotation()
    {
        Projectile.rotation =
            Projectile.velocity.ToRotation();
    }

    private void UpdateAnimation()
    {
        Projectile.frameCounter++;

        if (Projectile.frameCounter < FrameSpeed)
            return;

        Projectile.frameCounter = 0;
        Projectile.frame++;

        if (Projectile.frame >= FrameCount)
            Projectile.frame = 0;
    }

    private void UpdateLighting()
    {
        Lighting.AddLight(
            Projectile.Center,
            LightRed,
            LightGreen,
            LightBlue
        );
    }

    private void SpawnDust()
    {
        if (!Main.rand.NextBool(3))
            return;

        Vector2 localOffset = new Vector2(
            Main.rand.NextFloat(-25f, 25f),
            Main.rand.NextFloat(-25f, 25f)
        );

        Vector2 spawnOffset =
            localOffset.RotatedBy(Projectile.rotation);

        Vector2 dustVelocity =
            -Projectile.velocity * DustVelocityMultiplier +
            Main.rand.NextVector2Circular(2.5f, 2.5f);

        Dust dust = Dust.NewDustPerfect(
            Projectile.Center + spawnOffset,
            ModContent.DustType<SkyFlameDust>(),
            dustVelocity,
            0,
            default,
            Main.rand.NextFloat(
                DustMinScale,
                DustMaxScale
            )
        );

        dust.noGravity = true;
    }

    #endregion


    #region Collision

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        int offsetX = 0;
        int offsetY = 0;

        switch (Pattern)
        {
            case WallPattern.Horizontal:
                if (Side == WallSide.Left)
                {
                    offsetX = -3;
                    offsetY = -22;
                }
                else
                {
                    offsetX = -12;
                    offsetY = 22;
                }
                break;

            case WallPattern.Diagonal_1:
                if (Side == WallSide.Left)
                {
                    offsetX = 10;
                    offsetY = -10;
                }
                else
                {
                    offsetX = -35;
                    offsetY = 20;
                }
                break;

            case WallPattern.Diagonal_2:
                if (Side == WallSide.Left)
                {
                    offsetX = -10;
                    offsetY = -26;
                }
                else
                {
                    offsetX = -5;
                    offsetY = 30;
                }
                break;
        }

        hitbox.X =
            (int)Projectile.Center.X -
            HitboxWidth / 2 +
            offsetX;

        hitbox.Y =
            (int)Projectile.Center.Y -
            HitboxHeight / 2 +
            offsetY;

        hitbox.Width = HitboxWidth;
        hitbox.Height = HitboxHeight;
    }

    #endregion


    #region Rendering

    public override Color? GetAlpha(Color drawColor)
    {
        return Color.White;
    }

    #endregion
}