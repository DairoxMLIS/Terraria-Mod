using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;

using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.NPCs.Demigod_of_Sky_Flame_Azaroth;

public class AzarothCross : ModNPC
{
    #region State

    public bool Destroyed
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value ? 1f : 0f;
    }

    #endregion


    #region Constants

    private const int FrameCount = 8;
    private const int ActiveFrames = 4;
    private const int DestroyedStartFrame = 4;
    private const int FrameSpeed = 8;

    private const float FollowOffsetY = -100f;

    private const float OrbitOffsetY = -300f;
    private const float OrbitRadiusX = 30f;
    private const float OrbitRadiusY = 20f;

    private const float DestroyedMoveSpeed = 8f;
    private const float DestroyedMoveLerp = 0.08f;
    private const float DestroyedSlowdown = 0.95f;

    #endregion


    #region Lifecycle

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        NPC.width = 50;
        NPC.height = 50;
        NPC.lifeMax = 1000;
        NPC.defense = 20;
        NPC.HitSound = SoundID.NPCHit4;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.scale = 2f;
        NPC.friendly = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        SpawnDust();
    }

    public override void OnKill()
    {
        SpawnDust();
    }

    #endregion


    #region AI Main

    public override void AI()
    {
        NPC.dontTakeDamage = Destroyed;

        NPC azaroth = FindAzaroth();

        if (azaroth == null)
        {
            NPC.active = false;
            return;
        }

        if (!Destroyed)
        {
            FollowAzaroth(azaroth);
            return;
        }

        HandleDestroyedMovement(azaroth);
    }

    #endregion


    #region AI Subroutines

    private NPC FindAzaroth()
    {
        int azarothType =
            ModContent.NPCType<AzarothBody>();

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (npc.active && npc.type == azarothType)
                return npc;
        }

        return null;
    }

    private void FollowAzaroth(NPC azaroth)
    {
        NPC.Center =
            azaroth.Center +
            new Vector2(0f, FollowOffsetY);

        NPC.velocity = azaroth.velocity;
        NPC.rotation = 0f;

        AddLight();
    }

    private void HandleDestroyedMovement(NPC azaroth)
    {
        Vector2 orbitCenter =
            azaroth.Center +
            new Vector2(0f, OrbitOffsetY);

        NPC.ai[1] += 0.02f;

        Vector2 offset = new Vector2(
            (float)Math.Sin(NPC.ai[1]) * OrbitRadiusX,
            (float)Math.Cos(NPC.ai[1] * 1.2f) * OrbitRadiusY
        );

        Vector2 targetPosition =
            orbitCenter + offset;

        Vector2 toTarget =
            targetPosition - NPC.Center;

        if (toTarget.Length() > 5f)
        {
            Vector2 direction =
                toTarget.SafeNormalize(Vector2.Zero);

            NPC.velocity = Vector2.Lerp(
                NPC.velocity,
                direction * DestroyedMoveSpeed,
                DestroyedMoveLerp
            );
        }
        else
        {
            NPC.velocity *= DestroyedSlowdown;
        }

        NPC.rotation = 0f;

        AddLight();
    }

    private void AddLight()
    {
        Lighting.AddLight(
            NPC.Center,
            1f,
            1f,
            1f
        );
    }

    private void SpawnDust()
    {
        for (int i = 0; i < 55; i++)
        {
            Vector2 velocity =
                Main.rand.NextVector2Circular(3f, 3f);

            Dust dust = Dust.NewDustPerfect(
                NPC.Center + new Vector2(0f, 50f),
                ModContent.DustType<HolyDust>(),
                velocity,
                0,
                default,
                Main.rand.NextFloat(0.9f, 1.6f)
            );

            dust.noGravity = true;
        }

        Lighting.AddLight(
            NPC.Center,
            0.7f,
            0.8f,
            1f
        );
    }

    #endregion


    #region Animation

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;

        if (NPC.frameCounter < FrameSpeed)
            return;

        NPC.frameCounter = 0;

        if (!Destroyed)
        {
            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y >= ActiveFrames * frameHeight)
                NPC.frame.Y = 0;

            return;
        }

        NPC.frame.Y += frameHeight;

        if (NPC.frame.Y >= FrameCount * frameHeight)
            NPC.frame.Y = DestroyedStartFrame * frameHeight;
    }

    #endregion


    #region Death

    public override bool CheckDead()
    {
        if (Destroyed)
            return false;

        Destroyed = true;

        NPC.life = 1;
        NPC.dontTakeDamage = true;
        NPC.frame.Y =
            DestroyedStartFrame * NPC.frame.Height;

        NPC.netUpdate = true;

        Main.NewText(
            $"Azaroth's Cross has been destroyed! , DESTROYED={Destroyed}",
            255,
            0,
            0
        );

        return false;
    }

    #endregion


    #region Rendering

    public override bool PreDraw(
        SpriteBatch spriteBatch,
        Vector2 screenPos,
        Color drawColor)
    {
        return false;
    }

    #endregion
}