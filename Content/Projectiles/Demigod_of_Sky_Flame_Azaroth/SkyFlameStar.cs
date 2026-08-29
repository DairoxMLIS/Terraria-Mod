using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

public class SkyFlameStar : ModProjectile
{
    #region Constants

    private const int FrameCount = 3;
    private const int FrameSpeed = 6;
    private const int AnimationCycles = 5;

    private const int ShardCount = 8;
    private const float ShardSpeed = 9f;
    private const int ShardDamage = 25;

    private const int ExplosionDustCount = 45;

    private const float ShardSpawnOffset = 4f;

    #endregion


    #region Fields

    private int animationTimer;
    private bool exploded;

    #endregion


    #region Lifecycle

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;

        Projectile.scale = 2f;

        Projectile.hostile = false;
        Projectile.friendly = false;

        Projectile.penetrate = -1;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 600;
        
        Projectile.scale = 3f;
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

        Lighting.AddLight(
            Projectile.Center,
            1f,
            1f,
            1f
        );
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Height = 50;
        hitbox.Width = 50;
    }
    #endregion


    #region AI

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;

        UpdateAnimation();
        UpdateLighting();
    }

    private void UpdateAnimation()
    {
        animationTimer++;

        if (animationTimer % FrameSpeed != 0)
            return;

        Projectile.frame++;

        if (Projectile.frame >= FrameCount)
        {
            Projectile.frame = 0;

            int completedCycles =
                animationTimer /
                (FrameCount * FrameSpeed);

            if (completedCycles >= AnimationCycles)
            {
                Explode();
            }
        }
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
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture =
            TextureAssets.Projectile[Type].Value;

        Rectangle frame =
            texture.Frame(
                1,
                FrameCount,
                0,
                Projectile.frame
            );

        Vector2 origin =
            new Vector2(
                13.5f,
                15.5f
            );
        Vector2 drawOffset =
       new Vector2(
           -40f,
           -40f
       );

        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition + drawOffset,
            frame,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None
        );

        return false;
    }
    #endregion


    #region Explosion

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        SpawnExplosionDust();
        PlayExplosionSound();
        SpawnShards();

        Projectile.Kill();
    }

    private void SpawnExplosionDust()
    {
        for (int i = 0; i < ExplosionDustCount; i++)
        {
            Vector2 velocity =
                Main.rand.NextVector2Circular(
                    5f,
                    5f
                );

            Dust dust = Dust.NewDustPerfect(
                Projectile.Center,
                ModContent.DustType<SkyFlameDust>(),
                velocity,
                0,
                default,
                Main.rand.NextFloat(
                    0.8f,
                    1.5f
                )
            );

            dust.noGravity = true;
        }
    }

    public override Color? GetAlpha(Color drawColor) => Color.White;

    private void PlayExplosionSound()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        SoundEngine.PlaySound(
            SoundID.Item14,
            Projectile.Center
        );
    }

    private void SpawnShards()
    {
        Player targetPlayer = GetTargetPlayer();

        if (targetPlayer == null)
            return;

        float angleStep =
            MathHelper.TwoPi / ShardCount;

        for (int i = 0; i < ShardCount; i++)
        {
            float angle =
                angleStep * i;

            Vector2 direction =
                angle.ToRotationVector2();

            Vector2 spawnPosition =
                Projectile.Center +
                direction * ShardSpawnOffset;

            int projectileIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                spawnPosition,
                direction * ShardSpeed,
                ModContent.ProjectileType<SkyFlameStarShard>(),
                ShardDamage,
                0f
            );

            if (projectileIndex < 0 ||
                projectileIndex >= Main.maxProjectiles)
            {
                continue;
            }

            Projectile shard =
                Main.projectile[projectileIndex];

            // ai[0] = игрок, выбранный при начале атаки
            shard.ai[0] = targetPlayer.whoAmI;

            // ai[1] = таймер разворота
            shard.ai[1] = 0f;

            shard.netUpdate = true;
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

    #endregion
}

