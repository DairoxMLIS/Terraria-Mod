using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;


using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

public class SkyFlameCore : ModProjectile
{
    #region Constants

    private const int FrameCount = 4;
    private const int FrameSpeed = 5;

    private const float LightRed = 0.2f;
    private const float LightGreen = 0.7f;
    private const float LightBlue = 1f;

    private const float DustVelocityMultiplier = 0.1f;

    private const float InitialSpeed = 7f;
    private const float InitialSlowdownDuration = 50f;

    private const float DashSpeed = 12f;
    private const float DashDuration = 5f;

    private const float DashSlowdownDuration = 35f;

    private const float MinimumSpeed = 0.05f;

    private const int DashCount = 3;

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
    }

    public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
    {
        SoundEngine.PlaySound(
            SoundID.Item20,
            Projectile.Center
        );
    }

    #endregion

    #region Collision
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Height = 40;
        hitbox.Width = 40;
        hitbox.X = (int)(Projectile.Center.X - 40);
        hitbox.Y = (int)(Projectile.Center.Y - 60);
    }
    #endregion
    #region AI

    public override void AI()
    {
        UpdateMovement();
        UpdateAnimation();
        UpdateLighting();
        SpawnDust();

        
    }

    private void UpdateMovement()
    {
        float angle = Projectile.ai[0];

        float timer = Projectile.ai[1];

        int dashIndex = (int)Projectile.ai[2];

        Vector2 direction =
            angle.ToRotationVector2();

        timer++;
        Projectile.ai[1] = timer;

        float speed;

        // Первичное замедление после появления
        if (dashIndex == 0 &&
            timer <= InitialSlowdownDuration)
        {
            float progress =
                timer / InitialSlowdownDuration;

            speed = MathHelper.Lerp(
                InitialSpeed,
                MinimumSpeed,
                progress
            );
        }
        else
        {
            float cycleTime =
                DashDuration +
                DashSlowdownDuration;

            float cycleTimer =
                timer - InitialSlowdownDuration;

            int currentDash =
                (int)(cycleTimer / cycleTime);

            if (currentDash >= DashCount)
            {
                speed = 0f;
            }
            else
            {
                float timeInCycle =
                    cycleTimer % cycleTime;

                if (timeInCycle < DashDuration)
                {
                    speed = DashSpeed;
                }
                else
                {
                    float slowdownProgress =
                        (timeInCycle - DashDuration) /
                        DashSlowdownDuration;

                    speed = MathHelper.Lerp(
                        DashSpeed,
                        MinimumSpeed,
                        slowdownProgress
                    );
                }

                dashIndex = currentDash + 1;
                Projectile.ai[2] = dashIndex;
            }
        }

        if (speed <= MinimumSpeed)
        {
            Projectile.velocity = Vector2.Zero;
        }
        else
        {
            Projectile.velocity =
                direction * speed;
        }
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
        if (!Main.rand.NextBool(4))
            return;

        Vector2 offset =
            Main.rand.NextVector2Circular(
                18f,
                18f
            );

        Vector2 dustVelocity =
            -Projectile.velocity *
            DustVelocityMultiplier;
        Vector2 OffsetXY = new Vector2(
            -25f,-50f
        );
        Dust dust = Dust.NewDustPerfect(
            Projectile.Center + offset+OffsetXY,
            ModContent.DustType<SkyFlameDust>(),
            dustVelocity,
            0,
            default,
            Main.rand.NextFloat(
                0.6f,
                1.1f
            )
        );

        dust.noGravity = true;
    }
    public override Color? GetAlpha(Color drawColor) => Color.White;

    #endregion


}
