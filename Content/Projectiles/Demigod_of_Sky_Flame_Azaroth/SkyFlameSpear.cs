using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

public class SkyFlameSpear : ModProjectile
{
    #region Constants

    private const int WarningTime = 45;
    private const int FlashTime = 5;
    private const int DashTime = 14;

    private const float DashSpeed = 50f;

    private const float BeamLength = 2500f;
    private const float BeamWidth = 10f;

    private const float WarningAlphaMin = 0.08f;
    private const float WarningAlphaMax = 0.35f;

    private const float FlashAlpha = 1f;

    private const float RotationOffset = 0f;

    private const int Damage = 40;
    private const int LaunchDustCount = 35;
    private Vector2 RandomVector = Main.rand.NextVector2Unit() * 100;
    private const int HitboxSize = 20;

    private const float HitboxOffsetX = 40f;
    private const float HitboxOffsetY = 0f;

    #endregion


    #region Fields

    private int Timer
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    private float MovementAngle =>
        Projectile.ai[0];

    private int TargetPlayerIndex =>
        (int)Projectile.ai[1];

    private const int AfterimageCount = 6;
    private const float AfterimageSpacing = 6f;
    private const float AfterimageAlpha = 0.35f;

    private readonly Vector2[] afterimagePositions =
        new Vector2[AfterimageCount];

    private readonly float[] afterimageRotations =
        new float[AfterimageCount];



    #endregion


    #region Lifecycle

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 80;

        Projectile.scale = 2f;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.penetrate = 1;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft =
            400;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (Main.netMode != NetmodeID.Server)
        {
            SoundEngine.PlaySound(
                SoundID.Item20,
                Projectile.Center
            );
        }
    }

    #endregion


    #region AI

    public override void AI()
    {
        Timer++;

        if (Timer < WarningTime)
        {
            UpdateTargetDirection();
            HandleWarning();
            return;
        }

        if (Timer < WarningTime + FlashTime)
        {
            UpdateTargetDirection();
            HandleFlash();
            return;
        }

        if (Timer == WarningTime + FlashTime)
        {
            StartDash();
        }

        if (Timer < WarningTime + FlashTime + DashTime)
        {
            HandleDash();
            return;
        }
    }
    private void HandleFlash()
    {
        Projectile.velocity = Vector2.Zero;

        Projectile.rotation =
            MovementAngle;
    }
    private void UpdateTargetDirection()
    {
        Player player = GetTargetPlayer();

        if (player == null)
            return;

        Vector2 toPlayer =
            (player.Center +
    RandomVector) - Projectile.Center;

        if (toPlayer == Vector2.Zero)
            return;

        Projectile.ai[0] = toPlayer.ToRotation();
    }
    private Player GetTargetPlayer()
    {
        int targetIndex =
            (int)Projectile.ai[1];

        if (targetIndex < 0 ||
            targetIndex >= Main.maxPlayers)
        {
            return null;
        }

        Player player =
            Main.player[targetIndex];

        if (!player.active ||
            player.dead)
        {
            return null;
        }

        return player;
    }

    private void HandleWarning()
    {
        Projectile.velocity = Vector2.Zero;

        Projectile.rotation =
            MovementAngle +
            RotationOffset;
    }
    private void StartDash()
    {
        Projectile.velocity =
            MovementAngle.ToRotationVector2() *
            DashSpeed;

        Projectile.rotation =
            MovementAngle;

        SpawnLaunchDust();
        PlayLaunchSound();
    }
    private void HandleDash()
    {
        Projectile.velocity =
            MovementAngle.ToRotationVector2() *
            DashSpeed;

        Projectile.rotation =
            MovementAngle;
    }
    private void PlayLaunchSound()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        SoundEngine.PlaySound(
            SoundID.Item30,
            Projectile.Center
        );
    }
    private void SpawnLaunchDust()
    {
        Vector2 direction =
            MovementAngle.ToRotationVector2();

        for (int i = 0; i < LaunchDustCount; i++)
        {
            Vector2 velocity =
                direction * -Main.rand.NextFloat(1f, 5f) +
                Main.rand.NextVector2Circular(2f, 2f);

            Dust dust = Dust.NewDustPerfect(
                Projectile.Center,
                ModContent.DustType<SkyFlameDust>(),
                velocity,
                0,
                default,
                Main.rand.NextFloat(0.8f, 1.5f)
            );

            dust.noGravity = true;
        }
    }

    #endregion


    #region Collision

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 localOffset = new Vector2(
            HitboxOffsetX,
            HitboxOffsetY
        );

        Vector2 worldOffset =
            localOffset.RotatedBy(Projectile.rotation);

        Vector2 hitboxCenter =
            Projectile.Center +
            worldOffset;

        hitbox.Width = HitboxSize;
        hitbox.Height = HitboxSize;

        hitbox.X =
            (int)hitboxCenter.X -
            HitboxSize / 2;

        hitbox.Y =
            (int)hitboxCenter.Y -
            HitboxSize / 2;
    }

    #endregion


    #region Rendering

    public override bool PreDraw(ref Color lightColor)
    {
        
        if (Timer < WarningTime + FlashTime)
        {
            DrawWarningLine(lightColor);
        }

        DrawSpear(lightColor);
        DrawAfterimages(lightColor);

        return false;
    }
    private void DrawAfterimages(Color lightColor)
    {
        Texture2D texture =
            TextureAssets.Projectile[Type].Value;

        Rectangle frame =
            texture.Frame(1, 1);

        Vector2 origin =
            frame.Size() / 2f;

        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Vector2 oldCenter =
                Projectile.oldPos[i] +
                Projectile.Size / 2f;

            float alpha =
                AfterimageAlpha *
                (1f - i / (float)Projectile.oldPos.Length);

            Color color =
                Color.White * alpha;

            Main.EntitySpriteDraw(
                texture,
                oldCenter - Main.screenPosition,
                frame,
                color,
                Projectile.oldRot[i],
                origin,
                Projectile.scale,
                SpriteEffects.None
            );
        }
    }

    private void DrawWarningLine(
        Color lightColor)
    {
        Texture2D pixel =
            TextureAssets.MagicPixel.Value;

        float alpha;

        if (Timer < WarningTime)
        {
            float progress =
                Timer /
                (float)WarningTime;

            alpha =
                MathHelper.Lerp(
                    WarningAlphaMin,
                    WarningAlphaMax,
                    progress
                );
        }
        else
        {
            float progress =
                (Timer - WarningTime) /
                (float)FlashTime;

            alpha =
                MathHelper.Lerp(
                    WarningAlphaMax,
                    FlashAlpha,
                    progress
                );
        }

        Color beamColor =
            Color.White * alpha;

        Vector2 direction =
            MovementAngle.ToRotationVector2();

        float rotation =
            MovementAngle;

        Vector2 beamStart =
            Projectile.Center;

        Vector2 beamCenter =
            beamStart +
            direction *
            (BeamLength / 2f);

        Main.EntitySpriteDraw(
            pixel,
            beamCenter -
            Main.screenPosition,
            new Rectangle(
                0,
                0,
                1,
                1
            ),
            beamColor,
            rotation,
            Vector2.One / 2f,
            new Vector2(
                BeamLength,
                BeamWidth
            ),
            SpriteEffects.None
        );
    }

    private void DrawSpear(
        Color lightColor)
    {
        Texture2D texture =
            TextureAssets.Projectile[Type].Value;

        Rectangle frame =
            texture.Frame(1, 1);

        Vector2 origin =
            frame.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            Projectile.Center -
            Main.screenPosition,
            frame,
            Color.White,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None
        );
    }

    #endregion
}