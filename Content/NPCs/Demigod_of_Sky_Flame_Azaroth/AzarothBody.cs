using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Dairox_Mod.Content.BossBars;
using Terraria.Audio;
using Dairox_Mod.Content.Projectiles.Demigod_of_Sky_Flame_Azaroth;

namespace Dairox_Mod.Content.NPCs.Demigod_of_Sky_Flame_Azaroth;

[AutoloadBossHead]
public class AzarothBody : ModNPC
{
    #region Enums & State
    private enum AttackType
    {
        None,
        FireballsFromSides,
        SkyFlameCore,
        StarBurst,
        SkyFlameSpear,
        Swords

    }

    private enum Phase
    {
        First = 0,
        Transition = 1,
        Second = 2,
        Third = 3
    }
    #endregion

    #region Constants
    private const int FIRST_PHASE_FRAME_COUNT = 4;
    private const int SECOND_PHASE_FRAME_COUNT = 4;
    private const int THIRD_PHASE_FRAME_COUNT = 4;
    private const int TRANSITION_FRAME_COUNT = 4;
    
    private const int TOTAL_FRAMES = FIRST_PHASE_FRAME_COUNT + SECOND_PHASE_FRAME_COUNT + THIRD_PHASE_FRAME_COUNT + TRANSITION_FRAME_COUNT; // 16

    // Индексы кадров
    private const int FIRST_PHASE_START = 0;
    private const int SECOND_PHASE_START = 4;
    private const int THIRD_PHASE_START = 12;
    private const int TRANSITION_START = 8;

    // Фреймы и тайминги
    private const int FRAME_SPEED = 10;
    private const int TRANSITION_DURATION = FRAME_SPEED * 4;

    // Полет
    private const float FLIGHT_RADIUS = 200f;
    private const float FLIGHT_HEIGHT_OFFSET = -280f;
    private const float RETURN_SPEED = 20f;
    private const float RETURN_LERP_SPEED = 0.02f;

    // Прочее
    private const int CROSS_MAX_HEALTH = 1000;

    private static readonly Vector2[] LIGHT_OFFSETS =
{
    new Vector2(0f, -80f),
    new Vector2(-20f, -80f),
    new Vector2(20f, -80f)
};

    #endregion

    #region Fields
    private AttackType currentAttack = AttackType.None;
    private int attackTimer;
    private bool crossDestroyedHandled;
    private int crossHealth;
    private int fireballPattern;
    private int SwordAttackTimer
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }
    private int SwordAttackIndex;
    #endregion

    #region Properties
    public int CrossHealth
    {
        get => crossHealth;
        set => crossHealth = value;
    }
    public int CrossMaxHealth => CROSS_MAX_HEALTH;
    #endregion

    #region Lifecycle
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = TOTAL_FRAMES;
    }

    public override void SetDefaults()
    {
        NPC.width = 130;
        NPC.height = 100;
        NPC.damage = 0;
        NPC.defense = 20;
        NPC.lifeMax = 5000;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.netAlways = true;
        NPC.scale = 2f;
        NPC.noTileCollide = true;
        NPC.BossBar = ModContent.GetInstance<AzarothBossBar>();
    }

    public override void OnSpawn(IEntitySource source)
    {
        Main.NewText($"AZAROTH SPAWN | boss={NPC.boss} | life={NPC.life}/{NPC.lifeMax} | BossBar={(NPC.BossBar == null ? "NULL" : NPC.BossBar.GetType().Name)}", Color.Yellow);
    }
    #endregion

    #region AI Main
    public override void AI()
    {
        Player player = GetTargetPlayer();
        if (player == null)
        {
            NPC.active = false;
            return;
        }

        UpdateCrossHealth();

        HandlePhaseTransition();
        HandleFlight(player);
        HandleRotation();
        HandleLighting();

        UpdateAttack(player);
    }
    #endregion

    #region AI Subroutines
    private Player GetTargetPlayer()
    {
        NPC.TargetClosest(true);
        Player player = Main.player[NPC.target];
        return (player != null && player.active && !player.dead) ? player : null;
    }

    private void HandlePhaseTransition()
    {
        // Переход на вторую фазу при 50% HP
        if (NPC.ai[0] == (float)Phase.First && NPC.life <= NPC.lifeMax / 2)
        {
            NPC.ai[0] = (float)Phase.Transition;
            NPC.ai[1] = 0;
            NPC.frame.Y = TRANSITION_START * NPC.height;
            NPC.netUpdate = true;
        }

        // Логика перехода
        if (NPC.ai[0] == (float)Phase.Transition)
        {
            NPC.ai[1]++;
            if (NPC.ai[1] >= TRANSITION_DURATION)
            {
                StartSecondPhase();
            }
        }

        // Логика уничтожения креста
        if (NPC.ai[0] == (float)Phase.Second && crossHealth <= 0 && !crossDestroyedHandled)
        {
            StartThirdPhase();
        }

        // Блокировка урона во второй фазе (пока есть крест)
        NPC.dontTakeDamage = (NPC.ai[0] == (float)Phase.Second && crossHealth > 0);
    }

    private void StartSecondPhase()
    {
        currentAttack = AttackType.None;
        NPC.ai[0] = (float)Phase.Second;
        ModContent.GetInstance<AzarothBossBar>().SetCrossIcon(true);
        SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
        NPC.ai[1] = 0;
        NPC.frame.Y = SECOND_PHASE_START * NPC.height;
        NPC.netUpdate = true;

        int cross = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y - 100, ModContent.NPCType<AzarothCross>());
        Main.npc[cross].ai[0] = NPC.whoAmI;
        Main.npc[cross].netUpdate = true;
        crossHealth = Main.npc[cross].life;

        SpawnSword(-1, -220, -170);
        SpawnSword(1, 220, -170);

    }

    private void SpawnSword(int direction, int xOffset, int yOffset)
    {
        int sword = NPC.NewNPC(
            NPC.GetSource_FromAI(),
            (int)NPC.Center.X + xOffset,
            (int)NPC.Center.Y + yOffset,
            ModContent.NPCType<AzarothSword>()
        );

        if (sword < 0 || sword >= Main.maxNPCs)
            return;

        Main.npc[sword].ai[0] = direction;
        Main.npc[sword].ai[1] = NPC.whoAmI;
        Main.npc[sword].ai[2] = 0f;
        Main.npc[sword].ai[3] = 0f;

        Main.npc[sword].netUpdate = true;
    }

    private void StartThirdPhase()
    {
        crossDestroyedHandled = true;
        NPC.ai[0] = (float)Phase.Third;
        ModContent.GetInstance<AzarothBossBar>().SetCrossIcon(false);
        SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
        NPC.frame.Y = THIRD_PHASE_START * NPC.height; // Возвращаем анимацию третьей фазы
        NPC.frameCounter = 0;
        NPC.dontTakeDamage = false; // Разрешаем урон
        NPC.netUpdate = true;
    }

    private void HandleFlight(Player player)
    {
        Vector2 flightCenter = player.Center + new Vector2(0f, FLIGHT_HEIGHT_OFFSET);
        Vector2 offset = NPC.Center - flightCenter;
        float distance = offset.Length();

        if (distance > FLIGHT_RADIUS)
        {
            Vector2 direction = (flightCenter - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, direction * RETURN_SPEED, RETURN_LERP_SPEED);
        }
        else
        {
            NPC.ai[2] += 0.02f;
            float waveX = (float)Math.Sin(NPC.ai[2]) * 2f;
            float waveY = (float)Math.Cos(NPC.ai[2] * 1.3f) * 1.5f;
            NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(waveX, waveY), 0.03f);
        }
    }

    private void HandleRotation()
    {
        float targetRotation = NPC.velocity.X * 0.04f;
        float maxRotation = MathHelper.ToRadians(10f);
        targetRotation = MathHelper.Clamp(targetRotation, -maxRotation, maxRotation);
        NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.1f);
    }

    private void HandleLighting()
    {

        foreach (var light_offset in LIGHT_OFFSETS)
            Lighting.AddLight(NPC.Center + light_offset, 1f, 1f, 1f);
    }

    private void UpdateAttack(Player player)
    {
        if (NPC.ai[0] == (float)Phase.First)
        {
            if (currentAttack == AttackType.None)
                StartAttack(AttackType.FireballsFromSides);

            switch (currentAttack)
            {
                case AttackType.FireballsFromSides:
                    FireballsFromSidesAttack(player);
                    break;
                case AttackType.SkyFlameCore:
                    SkyFlameCoreAttack();
                    break;
                case AttackType.StarBurst:
                    StarBurstAttack(player);
                    break;
                case AttackType.SkyFlameSpear:
                    SkyFlameSpearAttack(player);
                    break;
            }
        }
        if (NPC.ai[0] == (float)Phase.Second)
        {
            if (currentAttack == AttackType.None)
                StartAttack(AttackType.Swords);

            switch (currentAttack)
            {
                case AttackType.Swords:
                    SwordsAttack(player);
                    break;
            }
        }
    }
    #endregion

    #region Animation
    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;

        if (NPC.frameCounter < FRAME_SPEED) return;
        NPC.frameCounter = 0;

        if (NPC.ai[0] == (float)Phase.First)
        {
            AnimatePhase(FIRST_PHASE_START, FIRST_PHASE_START + FIRST_PHASE_FRAME_COUNT);
        }
        else if (NPC.ai[0] == (float)Phase.Transition)
        {
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y >= (TRANSITION_START + TRANSITION_FRAME_COUNT) * frameHeight)
                NPC.frame.Y = (TRANSITION_START + TRANSITION_FRAME_COUNT - 1) * frameHeight;
        }
        else if (NPC.ai[0] == (float)Phase.Second)
        {
            AnimatePhase(SECOND_PHASE_START, SECOND_PHASE_START + SECOND_PHASE_FRAME_COUNT);
        }
        else if (NPC.ai[0] == (float)Phase.Third)
        {
            AnimatePhase(THIRD_PHASE_START, THIRD_PHASE_START + THIRD_PHASE_FRAME_COUNT);
        }
    }

    private void AnimatePhase(int startFrame, int endFrame)
    {
        int frameHeight = NPC.height;
        NPC.frame.Y += frameHeight;
        if (NPC.frame.Y >= endFrame * frameHeight)
            NPC.frame.Y = startFrame * frameHeight;
    }
    #endregion

    #region Cross Health Sync
    private void UpdateCrossHealth()
    {
        crossHealth = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.type != ModContent.NPCType<AzarothCross>()) continue;
            if ((int)npc.ai[0] != NPC.whoAmI) continue;

            if (npc.ModNPC is AzarothCross cross && !cross.Destroyed)
            {
                crossHealth = npc.life;
                return;
            }
        }
    }
    #endregion

    #region Rendering
    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawAttachedNPCs(spriteBatch, screenPos);
    }

    private void DrawAttachedNPCs(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active) continue;

            if (npc.type != ModContent.NPCType<AzarothSword>() && npc.type != ModContent.NPCType<AzarothCross>())
                continue;

            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            Rectangle frame = npc.frame.Width > 0 && npc.frame.Height > 0
                ? npc.frame
                : texture.Frame(1, Main.npcFrameCount[npc.type]);

            spriteBatch.Draw(texture, npc.Center - screenPos, frame, npc.GetAlpha(Color.White), npc.rotation, frame.Size() / 2f, npc.scale, SpriteEffects.None, 0f);
        }
    }

    public override Color? GetAlpha(Color drawColor) => Color.White;
    #endregion

    #region Attacks

    private void StartAttack(AttackType attack)
    {
        currentAttack = attack;
        attackTimer = 0;
        NPC.netUpdate = true;
    }

    private void FireballsFromSidesAttack(Player player)
    {
        const float projectileSize = 32f;
        const float gapInsidePair = 20f;
        const float gapBetweenRows = 82f;

        const float spawnOffset = 500f;

        const float horizontalSpeed = 15f;
        const float diagonalSpeed = 7.5f;

        const int damage = 30;
        const int duration = 120;

        attackTimer++;

        if (attackTimer == 1)
        {
            SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
            SpawnFireballWall(
                player,
                projectileSize,
                gapInsidePair,
                gapBetweenRows,
                spawnOffset,
                horizontalSpeed,
                diagonalSpeed,
                damage
            );
        }

        if (attackTimer >= duration)
            StartAttack(AttackType.SkyFlameCore);
    }

    private void SpawnFireballWall(
        Player player,
        float projectileSize,
        float gapInsidePair,
        float gapBetweenRows,
        float spawnOffset,
        float horizontalSpeed,
        float diagonalSpeed,
        int damage)
    {
     

        float pairCenterDistance;
        float OffsetY_diagonal = 0;

        if (fireballPattern == 0)
        {
            pairCenterDistance =
                projectileSize + gapInsidePair;
        }
        else
        {
            float speedLength =
                MathF.Sqrt(
                    horizontalSpeed * horizontalSpeed +
                    diagonalSpeed * diagonalSpeed
                );

            float verticalPerpendicularComponent =
                horizontalSpeed / speedLength;

            pairCenterDistance =
                (projectileSize + gapInsidePair) /
                verticalPerpendicularComponent;
        }

        float rowSpacing =
            pairCenterDistance +
            projectileSize +
            gapBetweenRows;

        int rows = Math.Max(
        (int)Math.Ceiling(
        (Main.screenHeight + 1500f) / rowSpacing
        ),
        10
        );
       

        float wallHeight =
    (rows - 1) * rowSpacing;

        float firstRowY =
            player.Center.Y - wallHeight / 2f;

        float spawnDistance =
            Main.screenWidth / 2f + spawnOffset;

        float leftX =
            player.Center.X - spawnDistance;

        float rightX =
            player.Center.X + spawnDistance;

        Vector2 leftVelocity;
        Vector2 rightVelocity;
        bool diagonal = false;

        switch (fireballPattern)
        {
            case 0:
                leftVelocity = new Vector2(
                    horizontalSpeed,
                    0f
                );

                rightVelocity = new Vector2(
                    -horizontalSpeed,
                    0f
                );
                break;

            case 1:
                leftVelocity = new Vector2(
                    horizontalSpeed,
                    diagonalSpeed
                );

                rightVelocity = new Vector2(
                    -horizontalSpeed,
                    -diagonalSpeed
                );
                diagonal = true;
                break;


            default:
                leftVelocity = new Vector2(
                    horizontalSpeed,
                    -diagonalSpeed
                );

                rightVelocity = new Vector2(
                    -horizontalSpeed,
                    diagonalSpeed
                );
                diagonal = true;
                break;
        }
        if (diagonal)
            OffsetY_diagonal = gapBetweenRows*0.38f;

        for (int row = 0; row < rows; row++)
        {
            float pairCenterY =
                firstRowY + row * rowSpacing;

            if (fireballPattern == 0)
            {
                float leftY =
                    pairCenterY -
                    pairCenterDistance / 2f;

                float rightY =
                    pairCenterY +
                    pairCenterDistance / 2f;
                
                SpawnFireball(
                    new Vector2(leftX, leftY),
                    leftVelocity,
                    damage,
                    SkyFireBall.WallSide.Left
                );

                SpawnFireball(
                    new Vector2(rightX, rightY),
                    rightVelocity,
                    damage,
                    SkyFireBall.WallSide.Right
                );

                continue;
            }

            float travelTime =
                (player.Center.X - leftX) /
                leftVelocity.X;

            float leftSpawnY =
                pairCenterY -
                leftVelocity.Y * travelTime;

            float rightSpawnY =
                pairCenterY -
                rightVelocity.Y * travelTime;

            SpawnFireball(
                new Vector2(leftX, leftSpawnY),
                leftVelocity,
                damage,
                SkyFireBall.WallSide.Left
            );

            SpawnFireball(
                new Vector2(rightX, rightSpawnY+ OffsetY_diagonal),
                rightVelocity,
                damage,
                SkyFireBall.WallSide.Right
            );
        }
        fireballPattern++;

        if (fireballPattern > 2)
            fireballPattern = 0;
    }

    private void SpawnFireball(
        Vector2 position,
        Vector2 velocity,
        int damage,
        SkyFireBall.WallSide side)
    {
        int projectileIndex = Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            position,
            velocity,
            ModContent.ProjectileType<SkyFireBall>(),
            damage,
            0f
        );

        if (projectileIndex < 0 ||
            projectileIndex >= Main.maxProjectiles)
        {
            return;
        }

        Projectile projectile =
            Main.projectile[projectileIndex];

        projectile.ai[0] = fireballPattern;
        projectile.ai[1] = (int)side;
        projectile.netUpdate = true;
    }

    private void SkyFlameCoreAttack()
    {
        const int projectileCount = 12;
        const int damage = 40;
        const int duration = 120;

        attackTimer++;

        if (attackTimer == 1)
        {
            SpawnSkyFlameCoreProjectiles(
                projectileCount,
                damage
            );
        }

        if (attackTimer >= duration)
            StartAttack(AttackType.StarBurst);
    }

    private void SpawnSkyFlameCoreProjectiles(
        int projectileCount,
        int damage)
    {
        float angleStep =
            MathHelper.TwoPi / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle =
                angleStep * i;

            Vector2 direction =
                angle.ToRotationVector2();

            int projectileIndex = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                direction * 7f,
                ModContent.ProjectileType<SkyFlameCore>(),
                damage,
                0f
            );

            if (projectileIndex < 0 ||
                projectileIndex >= Main.maxProjectiles)
            {
                continue;
            }

            Projectile projectile =
                Main.projectile[projectileIndex];

            projectile.ai[0] = angle;
            projectile.ai[1] = 0f;

            projectile.netUpdate = true;
        }
    }
    private void StarBurstAttack(Player player)
    {
        const float horizontalOffset = 500f;
        const int duration = 120;

        attackTimer++;

        if (attackTimer == 1)
        {
            SpawnStarBurst(
                player,
                horizontalOffset
            );
        }

        if (attackTimer >= duration)
            StartAttack(AttackType.SkyFlameSpear);
    }

    private void SpawnStarBurst(
        Player player,
        float horizontalOffset)
    {
        Vector2 leftPosition =
            player.Center +
            new Vector2(-horizontalOffset, 0f);

        Vector2 rightPosition =
            player.Center +
            new Vector2(horizontalOffset, 0f);

        SpawnStar(
            leftPosition,
            player.whoAmI
        );

        SpawnStar(
            rightPosition,
            player.whoAmI
        );
    }

    private void SpawnStar(
        Vector2 position,
        int targetPlayer)
    {
        int projectileIndex = Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            position,
            Vector2.Zero,
            ModContent.ProjectileType<SkyFlameStar>(),
            0,
            0f
        );

        if (projectileIndex < 0 ||
            projectileIndex >= Main.maxProjectiles)
        {
            return;
        }

        Projectile projectile =
            Main.projectile[projectileIndex];

        // ai[0] = индекс игрока, которого выбрали в начале атаки
        projectile.ai[0] = targetPlayer;

        projectile.netUpdate = true;
    }
    private void SkyFlameSpearAttack(Player player)
    {
        const float spawnRadius = 500f;
        const int duration = 100;
        const int spearCount = 5;
        const int spawnInterval = 10;

        attackTimer++;

        if (attackTimer <= spearCount * spawnInterval &&
            attackTimer % spawnInterval == 1)
        {
            SpawnSkyFlameSpear(
                player,
                spawnRadius
            );
        }

        if (attackTimer >= duration)
            StartAttack(AttackType.None);
    }
    private void SpawnSkyFlameSpear(
    Player player,
    float spawnRadius)
    {
        float spawnAngle =
            Main.rand.NextFloat(MathHelper.TwoPi);

        Vector2 spawnDirection =
            spawnAngle.ToRotationVector2();

        Vector2 spawnPosition =
            player.Center +
            spawnDirection * spawnRadius;

        Vector2 movementDirection =
            (player.Center - spawnPosition)
            .SafeNormalize(Vector2.Zero);

        float movementAngle =
            movementDirection.ToRotation();

        int projectileIndex = Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            spawnPosition,
            Vector2.Zero,
            ModContent.ProjectileType<SkyFlameSpear>(),
            40,
            0f
        );

        if (projectileIndex < 0 ||
            projectileIndex >= Main.maxProjectiles)
        {
            return;
        }

        Projectile projectile =
            Main.projectile[projectileIndex];

        // ai[0] = угол движения
        projectile.ai[0] = movementAngle;

        // ai[1] = индекс игрока, выбранного при старте
        projectile.ai[1] = player.whoAmI;

        projectile.netUpdate = true;
    }
    private void SwordsAttack(Player player)
    {
        const int duration = 180;

        attackTimer++;

        if (attackTimer == 1)
        {
            StartSwordAttack(SwordAttackIndex);
        }

        if (attackTimer >= duration)
        {
            SwordAttackIndex++;

            if (SwordAttackIndex >= 2)
                SwordAttackIndex = 0;

            StartAttack(AttackType.None);
        }
    }
    private void StartSwordAttack(int attackIndex)
    {
        int swordType = ModContent.NPCType<AzarothSword>();

        float requiredSide = attackIndex == 0 ? -1f : 1f;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC sword = Main.npc[i];

            if (!sword.active)
                continue;

            if (sword.type != swordType)
                continue;

            if ((int)sword.ai[1] != NPC.whoAmI)
                continue;

            // Нужен конкретный меч:
            // -1 = левый
            // +1 = правый
            if (sword.ai[0] != requiredSide)
                continue;

            sword.ai[2] = 1f;
            sword.localAI[0] = 0f;
            sword.netUpdate = true;

            break;
        }

        if (Main.netMode != NetmodeID.Server)
        {
            SoundEngine.PlaySound(
                SoundID.Item29,
                NPC.Center
            );
        }
    }
    private void StopSwordAttack()
    {
        int swordType = ModContent.NPCType<AzarothSword>();

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC sword = Main.npc[i];

            if (!sword.active || sword.type != swordType)
                continue;

            if ((int)sword.ai[1] != NPC.whoAmI)
                continue;

            sword.ai[2] = 0f; // обратно в Follow
            sword.localAI[0] = 0f;
            sword.netUpdate = true;
        }
    }
    #endregion


}
