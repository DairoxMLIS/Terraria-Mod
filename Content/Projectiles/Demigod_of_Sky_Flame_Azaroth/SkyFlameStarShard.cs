using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

public class SkyFlameStarShard : ModProjectile
{
    #region Constants

    private const int FrameCount = 3;
    private const int FrameSpeed = 5;
    private const float Speed = 9f;

    private const float OutwardTime = 20f;

    private const float TurnLerp = 0.04f;


    private const int HitboxWidth = 20;
    private const int HitboxHeight = 20;

    private const float HitboxOffsetX = 0f;
    private const float HitboxOffsetY = - 20f;


    #endregion


    #region Lifecycle
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;

        Projectile.scale = 2f;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.penetrate = 1;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 300;
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 localOffset = new Vector2(
            HitboxOffsetX,
            HitboxOffsetY
        );

        Vector2 worldOffset =
            localOffset.RotatedBy(Projectile.rotation);

        hitbox.X =
            (int)(Projectile.Center.X
            + worldOffset.X
            - HitboxWidth / 2f);

        hitbox.Y =
            (int)(Projectile.Center.Y
            + worldOffset.Y
            - HitboxHeight / 2f);

        hitbox.Width = HitboxWidth;
        hitbox.Height = HitboxHeight;
    }

    public override Color? GetAlpha(Color drawColor) => Color.White;
    #endregion
    private Player targetPlayer;
    private int TargetTimer = 0;

    #region AI

    public override void AI()
    {
        UpdateMovement();
        UpdateRotation();
        UpdateAnimation();
        SpawnDust();
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

    private void UpdateMovement()
    {
        Projectile.ai[1]++;

        // Сначала разлетаемся строго от звезды.
        if (Projectile.ai[1] <= OutwardTime)
        {
            Projectile.velocity =
                Projectile.velocity
                    .SafeNormalize(Vector2.Zero)
                * Speed;

            return;
        }

        if (TargetTimer <= 60)
        {
            targetPlayer = GetTargetPlayer();
            TargetTimer++;
        }
        else
        {
            targetPlayer =null;
        }
        if (targetPlayer == null)
        {
            Projectile.velocity =
                Projectile.velocity
                    .SafeNormalize(Vector2.Zero)
                * Speed;

            return;
        }

        Vector2 toPlayer =
            targetPlayer.Center -
            Projectile.Center;

        if (toPlayer == Vector2.Zero)
            return;

        Vector2 targetDirection =
            toPlayer.SafeNormalize(Vector2.Zero);

        Vector2 currentDirection =
            Projectile.velocity.SafeNormalize(Vector2.Zero);

        Vector2 newDirection =
            Vector2.Lerp(
                currentDirection,
                targetDirection,
                TurnLerp
            ).SafeNormalize(Vector2.Zero);

        Projectile.velocity =
            newDirection * Speed;
    }

    private void UpdateRotation()
    {
        if (Projectile.velocity != Vector2.Zero)
        {
            Projectile.rotation =
                Projectile.velocity.ToRotation();
        }
    }

    private Player GetTargetPlayer()
    {
        int targetIndex =
            (int)Projectile.ai[0];

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
    private void SpawnDust()
    {
        if (!Main.rand.NextBool(5))
            return;

        Vector2 offset = Main.rand.NextVector2Circular(20f, 35f);

        Vector2 velocity =
            -Projectile.velocity * 0.1f +
            Main.rand.NextVector2Circular(1f, 1f);

        Dust dust = Dust.NewDustPerfect(
            Projectile.Center + offset,
            ModContent.DustType<SkyFlameDust>(),
            velocity,
            0,
            default,
            Main.rand.NextFloat(0.5f, 1f)
        );

        dust.noGravity = true;
    }
    #endregion
}
