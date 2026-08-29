using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Dairox_Mod.Content.Dusts;

namespace Dairox_Mod.Content.NPCs.Demigod_of_Sky_Flame_Azaroth;

public class AzarothSword : ModNPC
{
    // ==========================================
    // ОБЫЧНОЕ ПОЛОЖЕНИЕ
    // ==========================================

    private const float FollowDistance = 220f;
    private const float FollowHeight = -200f;

    private const float FollowSpeed = 10f;
    private const float FollowAcceleration = 0.08f;

    // ==========================================
    // АТАКА
    // ==========================================

    private const int AttackPrepareTime = 30;
    private const int FlashTime = 5;

    private const float DashSpeed = 50f;

    // Через сколько пикселей от игрока меч удаляется
    private const float DespawnDistance = 1400f;

    // ==========================================
    // ЭФФЕКТЫ
    // ==========================================

    private const int LaunchDustCount = 35;

    private const int AfterimageCount = 12;
    private const float AfterimageAlpha = 0.35f;

    private const float RotationOffset =
        MathHelper.PiOver4;


    // ==========================================
    // AI
    // ==========================================

    // ai[0] = сторона
    // -1 = левый
    //  1 = правый
    private float Side
    {
        get => NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    // ai[1] = whoAmI AzarothBody
    private int AzarothIndex
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    // ai[2] = состояние
    //
    // 0 = обычное следование
    // 1 = подготовка
    // 2 = вспышка
    // 3 = полёт
    private int State
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    // ai[3] = направление атаки
    private float AttackAngle
    {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    private int AttackTimer
{
    get => (int)NPC.localAI[0];
    set => NPC.localAI[0] = value;
}


    // ==========================================
    // DEFAULTS
    // ==========================================

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.TrailCacheLength[Type] = AfterimageCount;
        NPCID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        NPC.width = 40;
        NPC.height = 100;

        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 200;

        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath6;

        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.aiStyle = -1;

        NPC.scale = 3f;

        NPC.boss = false;

        NPC.dontTakeDamage = true;
        NPC.npcSlots = 0f;

        NPC.knockBackResist = 0f;
    }


    // ==========================================
    // AI
    // ==========================================

    public override void AI()
    {
        NPC azaroth = GetAzaroth();

        if (azaroth == null)
        {
            NPC.active = false;
            return;
        }

        Player player = GetPlayer(azaroth);

        if (player == null)
        {
            NPC.velocity *= 0.95f;
            return;
        }

        switch (State)
        {
            // -----------------------------
            // ОБЫЧНОЕ СОСТОЯНИЕ
            // -----------------------------

            case 0:
                FollowAzaroth(azaroth);
                UpdateNormalRotation(player);
               /// Main.NewText("State: 0", Color.White);
                break;


            // -----------------------------
            // ПОДГОТОВКА
            // -----------------------------

            case 1:
                PrepareAttack(player);
               /// Main.NewText("State: 1", Color.White); 
               break;


            // -----------------------------
            // ВСПЫШКА
            // -----------------------------

            case 2:
                HandleFlash();
               /// Main.NewText("State: 2", Color.White);
                break;


            // -----------------------------
            // ПОЛЁТ
            // -----------------------------

            case 3:
                HandleDash(player);
              ///  Main.NewText("State: 3", Color.White);
                break;
        }
    }


    // ==========================================
    // FOLLOW
    // ==========================================

    private void FollowAzaroth(NPC azaroth)
    {
        Vector2 targetPosition =
            azaroth.Center +
            new Vector2(
                Side * FollowDistance,
                FollowHeight
            );

        Vector2 direction =
            targetPosition - NPC.Center;

        if (direction.Length() > 5f)
        {
            direction.Normalize();

            Vector2 targetVelocity =
                direction * FollowSpeed;

            NPC.velocity =
                Vector2.Lerp(
                    NPC.velocity,
                    targetVelocity,
                    FollowAcceleration
                );
        }
        else
        {
            NPC.velocity *= 0.95f;
        }
    }


    private void UpdateNormalRotation(Player player)
    {
        Vector2 direction =
            player.Center - NPC.Center;

        if (direction == Vector2.Zero)
            return;

        NPC.rotation =
            direction.ToRotation() -
            RotationOffset;
    }


    // ==========================================
    // ПОДГОТОВКА АТАКИ
    // ==========================================

    private void PrepareAttack(Player player)
    {
        NPC.velocity = Vector2.Zero;

        AttackTimer++;

        Vector2 direction =
            player.Center - NPC.Center;

        if (direction != Vector2.Zero)
        {
            AttackAngle =
                direction.ToRotation();
        }

        NPC.rotation =
            AttackAngle -
            RotationOffset;

        if (AttackTimer >= AttackPrepareTime)
        {
            AttackTimer = 0;

            State = 2;

            NPC.netUpdate = true;
        }
    }


    // ==========================================
    // ВСПЫШКА
    // ==========================================

    private void HandleFlash()
    {
        NPC.velocity = Vector2.Zero;

        AttackTimer++;

        NPC.rotation =
            AttackAngle -
            RotationOffset;

        // Белая вспышка
        Lighting.AddLight(
            NPC.Center,
            1.5f,
            1.5f,
            1.5f
        );

        if (AttackTimer >= FlashTime)
        {
            AttackTimer = 0;

            StartDash();
        }
    }


    // ==========================================
    // ЗАПУСК
    // ==========================================

    private void StartDash()
    {
        Vector2 direction =
            AttackAngle.ToRotationVector2();

        NPC.velocity =
            direction * DashSpeed;

        NPC.rotation =
            AttackAngle -
            RotationOffset;

        State = 3;

        SpawnLaunchDust();

        if (Main.netMode != NetmodeID.Server)
        {
            SoundEngine.PlaySound(
                SoundID.Item71,
                NPC.Center
            );
        }

        NPC.netUpdate = true;
    }


    // ==========================================
    // ПОЛЁТ
    // ==========================================

    private void HandleDash(Player player)
    {
        NPC.velocity =
            AttackAngle.ToRotationVector2() *
            DashSpeed;

        NPC.rotation =
            AttackAngle -
            RotationOffset;

        Lighting.AddLight(
            NPC.Center,
            1f,
            1f,
            1f
        );

        float distance =
            Vector2.Distance(
                NPC.Center,
                player.Center
            );

        if (distance >= DespawnDistance)
        {
            RespawnSword();
        }
    }


    // ==========================================
    // РЕСПАВН
    // ==========================================

    private void RespawnSword()
    {
        // Создавать NPC должен сервер
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        NPC azaroth = GetAzaroth();

        if (azaroth == null)
        {
            NPC.active = false;
            return;
        }

        Vector2 spawnPosition =
            azaroth.Center +
            new Vector2(
                Side * FollowDistance,
                FollowHeight
            );

        int newSword =
            NPC.NewNPC(
                NPC.GetSource_Misc(
                    "AzarothSwordRespawn"
                ),
                (int)spawnPosition.X,
                (int)spawnPosition.Y,
                ModContent.NPCType<AzarothSword>(),
                0,
                Side,
                AzarothIndex,
                0,
                0
            );

        if (newSword >= 0 &&
            newSword < Main.maxNPCs)
        {
            Main.npc[newSword].netUpdate = true;
        }

        // Звук появления нового меча
        if (Main.netMode != NetmodeID.Server)
        {
            SoundEngine.PlaySound(
                SoundID.Item20,
                spawnPosition
            );
        }

        NPC.active = false;
        NPC.netUpdate = true;
    }


    // ==========================================
    // AZAROTH
    // ==========================================

    private NPC GetAzaroth()
    {
        if (AzarothIndex < 0 ||
            AzarothIndex >= Main.maxNPCs)
        {
            return null;
        }

        NPC azaroth =
            Main.npc[AzarothIndex];

        if (!azaroth.active)
            return null;

        if (azaroth.type !=
            ModContent.NPCType<AzarothBody>())
        {
            return null;
        }

        return azaroth;
    }


    // ==========================================
    // PLAYER
    // ==========================================

    private Player GetPlayer(NPC azaroth)
    {
        int target =
            azaroth.target;

        if (target < 0 ||
            target >= Main.maxPlayers)
        {
            return null;
        }

        Player player =
            Main.player[target];

        if (!player.active ||
            player.dead)
        {
            return null;
        }

        return player;
    }


    // ==========================================
    // DUST
    // ==========================================

    private void SpawnLaunchDust()
    {
        Vector2 direction =
            AttackAngle.ToRotationVector2();

        for (int i = 0;
            i < LaunchDustCount;
            i++)
        {
            Vector2 velocity =
                -direction *
                Main.rand.NextFloat(
                    1f,
                    5f
                );

            velocity +=
                Main.rand.NextVector2Circular(
                    2f,
                    2f
                );

            Dust dust =
                Dust.NewDustPerfect(
                    NPC.Center,
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


    // ==========================================
    // DRAW
    // ==========================================

    public override bool PreDraw(
    SpriteBatch spriteBatch,
    Vector2 screenPos,
    Color drawColor)
    {
        DrawSwordAfterimages(spriteBatch, screenPos);

        DrawSword(spriteBatch, screenPos);

        return false;
    }


    private void DrawSword(
        SpriteBatch spriteBatch,
        Vector2 screenPos)
    {
        Texture2D texture =
            TextureAssets.Npc[Type].Value;

        Rectangle frame =
            texture.Frame(
                1,
                4,
                0,
                NPC.frame.Y / NPC.frame.Height
            );

        Vector2 origin =
            frame.Size() / 2f;

        Main.EntitySpriteDraw(
            texture,
            NPC.Center - screenPos,
            frame,
            Color.White,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None
        );
    }


    private void DrawSwordAfterimages(
    SpriteBatch spriteBatch,
    Vector2 screenPos)
    {
        if (State != 3)
            return;

        Texture2D texture =
            TextureAssets.Npc[Type].Value;

        Rectangle frame =
            texture.Frame(
                1,
                4,
                0,
                NPC.frame.Y / NPC.frame.Height
            );

        Vector2 origin =
            frame.Size() / 2f;

        for (int i = 0; i < NPC.oldPos.Length; i++)
        {
            Vector2 oldCenter =
                NPC.oldPos[i] +
                NPC.Size / 2f;

            float progress =
                i / (float)NPC.oldPos.Length;

            float alpha =
                MathHelper.Lerp(
                    AfterimageAlpha,
                    0f,
                    progress
                );

            Main.EntitySpriteDraw(
                texture,
                oldCenter - screenPos,
                frame,
                Color.White * alpha,
                NPC.oldRot[i],
                origin,
                NPC.scale,
                SpriteEffects.None
            );
        }
    }


    // ==========================================
    // ANIMATION
    // ==========================================

    public override void FindFrame(
        int frameHeight)
    {
        NPC.frameCounter++;

        if (NPC.frameCounter >= 8)
        {
            NPC.frameCounter = 0;

            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y >=
                frameHeight * 4)
            {
                NPC.frame.Y = 0;
            }
        }
    }
}