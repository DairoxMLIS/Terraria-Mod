using Dairox_Mod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Projectiles;

public class AncientSpearProjectile : ModProjectile
{
    protected virtual float HoldoutRangeMin => -10f;
    protected virtual float HoldoutRangeMax => 24f;

    protected bool fired;

    private const int FrameCount = 2;
    private const int FrameSpeed = 8;
    private const int TimeLeft = 60;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.timeLeft = TimeLeft;

        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
        Projectile.hide = true;
        Projectile.scale = 2f;
        Projectile.aiStyle = -1;
    }

    public override bool PreAI()
    {
        Player player = Main.player[Projectile.owner];
        int duration = player.itemAnimationMax;

        player.heldProj = Projectile.whoAmI;

        if (Projectile.timeLeft > duration)
            Projectile.timeLeft = duration;

        UpdateAnimation();

        Projectile.velocity =
            Projectile.velocity.SafeNormalize(Vector2.UnitX);

        float halfDuration = duration * 0.5f;

        float progress =
            Projectile.timeLeft < halfDuration
                ? Projectile.timeLeft / halfDuration
                : (duration - Projectile.timeLeft) / halfDuration;

        Vector2 offset = new Vector2(0f, 8f);

        Projectile.Center =
            player.MountedCenter +
            offset +
            Vector2.SmoothStep(
                Projectile.velocity * HoldoutRangeMin,
                Projectile.velocity * HoldoutRangeMax,
                progress
            );

        UpdateRotation();
        SpawnDust();
        FireProjectile(progress);

        return false;
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

        if (Projectile.velocity.X < 0f)
        {
            Projectile.spriteDirection = -1;
            Projectile.rotation += MathHelper.Pi;
        }
        else
        {
            Projectile.spriteDirection = 1;
            Projectile.rotation += MathHelper.PiOver2;
        }

        Projectile.rotation -= MathHelper.ToRadians(45f);
    }

    private void SpawnDust()
    {
        if (Main.dedServ)
            return;

        if (Main.rand.NextBool(11))
        {
            Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                ModContent.DustType<AncientSparkle>(),
                Projectile.velocity.X * 2f,
                Projectile.velocity.Y * 2f,
                Alpha: 128,
                Scale: 1.2f
            );
        }

        if (Main.rand.NextBool(10))
        {
            Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                ModContent.DustType<AncientSparkle>(),
                Alpha: 128,
                Scale: 0.3f
            );
        }
    }

    private void FireProjectile(float progress)
    {
        if (progress < 0.9f || fired)
            return;

        fired = true;

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Projectile.velocity * 5f,
            ModContent.ProjectileType<AncientSpearProjectile_2>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner
        );
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 tipCenter =
            Projectile.Center +
            Projectile.velocity * 50f;

        hitbox = new Rectangle(
            (int)(tipCenter.X - 25f),
            (int)(tipCenter.Y - 18f),
            50,
            25
        );
    }
}