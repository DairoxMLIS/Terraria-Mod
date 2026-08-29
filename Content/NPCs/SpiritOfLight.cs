using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Dairox_Mod.Content.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.NPCs;

public class SpiritOfLight : ModNPC
{
    private int attackTimer = 0;
    private const int AttackCooldown = 60;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults()
    {
        NPC.width = 20;
        NPC.height = 20;
        NPC.defense = 10;
        NPC.lifeMax = 300;
        NPC.HitSound = SoundID.NPCHit36;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.8f;
        NPC.friendly = false;
        NPC.aiStyle = -1;
        NPC.dontTakeDamage = false;
        NPC.netAlways = true;
        NPC.scale = 1.5f;
        NPC.damage = 20;
    }

    public override bool? CanFallThroughPlatforms()
    {
        return true;
    }

    public override bool PreDraw(
        SpriteBatch spriteBatch,
        Vector2 screenPos,
        Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle sourceRectangle = NPC.frame;
        Vector2 drawPosition = NPC.Center - screenPos;

        drawPosition.Y += 5f;

        spriteBatch.Draw(
            texture,
            drawPosition,
            sourceRectangle,
            drawColor,
            NPC.rotation,
            sourceRectangle.Size() / 2f,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }

    public override void OnKill()
    {
        for (int k = 0; k < 5; k++)
        {
            Dust.NewDust(
                NPC.position + NPC.velocity,
                NPC.width,
                NPC.height,
                ModContent.DustType<AncientSparkle>(),
                NPC.oldVelocity.X * 0.5f,
                NPC.oldVelocity.Y * 0.5f
            );
        }

        SoundEngine.PlaySound(
            SoundID.Item73,
            NPC.position
        );
    }

    public override void AI()
    {
        NPC.TargetClosest(true);

        Player player = Main.player[NPC.target];

        if (player != null && player.active && !player.dead)
        {
            FollowPlayer(player);
            HandleAttack(player);
        }
        else
        {
            NPC.velocity = Vector2.Zero;
            NPC.rotation = 0f;
        }

        UpdateAnimation();
        UpdateLighting();
    }

    private void FollowPlayer(Player player)
    {
        Vector2 toPlayer = player.Center - NPC.Center;
        Vector2 direction = toPlayer.SafeNormalize(Vector2.Zero);

        float distanceToPlayer = toPlayer.Length();

        Vector2 perpendicular =
            new Vector2(-direction.Y, direction.X);

        NPC.ai[2] += 0.04f;

        float wave =
            (float)Math.Sin(NPC.ai[2]) * 0.6f;

        Vector2 waveVector =
            perpendicular * wave * 0.8f;

        bool canSeePlayer = Collision.CanHitLine(
            NPC.position,
            NPC.width,
            NPC.height,
            player.position,
            player.width,
            player.height
        );

        if (distanceToPlayer > 250f || !canSeePlayer)
        {
            NPC.velocity =
                direction * 2f + waveVector;
        }
        else if (distanceToPlayer < 100f)
        {
            NPC.velocity =
                -direction * 2f + waveVector;
        }
        else
        {
            NPC.velocity = waveVector;
        }
    }

    private void HandleAttack(Player player)
    {
        Vector2 toPlayer =
            player.Center - NPC.Center;

        Vector2 direction =
            toPlayer.SafeNormalize(Vector2.Zero);

        bool canSeePlayer = Collision.CanHitLine(
            NPC.position,
            NPC.width,
            NPC.height,
            player.position,
            player.width,
            player.height
        );

        if (canSeePlayer && NPC.ai[0] > AttackCooldown)
        {
            NPC.ai[0] = 0;

            SoundEngine.PlaySound(
                SoundID.Item20,
                NPC.position
            );

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                direction * 10f,
                ModContent.ProjectileType<Sparkle>(),
                NPC.damage,
                0f,
                Main.myPlayer
            );
        }
        else
        {
            NPC.ai[0]++;
        }
    }

    private void UpdateAnimation()
    {
        NPC.frameCounter++;

        if (NPC.frameCounter < 8)
            return;

        NPC.frameCounter = 0;
        NPC.frame.Y += 40;

        if (NPC.frame.Y >= Main.npcFrameCount[Type] * 40)
            NPC.frame.Y = 0;
    }

    private void UpdateLighting()
    {
        Lighting.AddLight(
            NPC.Center,
            0.6f,
            0.6f,
            0.8f
        );
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ZoneSkyHeight)
            return 0.1f;

        return 0f;
    }
}