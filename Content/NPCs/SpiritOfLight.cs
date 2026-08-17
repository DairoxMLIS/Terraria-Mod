using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Dairox_Mod.Content.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;

namespace Dairox_Mod.Content.NPCs
{
    public class SpiritOfLight : ModNPC
    {

        private int attackTimer = 0;
        private const int AttackCooldown = 60;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4; // 4 кадра
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

            // Опускаем спрайт на 10 пикселей
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

        public override void AI()
        {
            NPC.TargetClosest(true);

            Player player = Main.player[NPC.target];

            // 3. Проверить, что игрок существует и жив (важно!)
            if (player == null || player.dead || !player.active)
            {
                // Если игрок мёртв или неактивен, можно выйти из AI или искать другого
                return;
            }

            Vector2 FromNPCToPlayer = player.Center - NPC.Center;
            Vector2 direction = FromNPCToPlayer.SafeNormalize(Vector2.Zero);
            float distanceToPlayer = FromNPCToPlayer.Length();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            NPC.ai[2] += 0.04f;
            float wave = (float)Math.Sin(NPC.ai[2]) * 0.6f;
            Vector2 waveVector = perpendicular * wave * 0.8f;

            bool canSeePlayer = Collision.CanHitLine(
            NPC.position,           // Позиция NPC
            NPC.width,              // Ширина NPC
            NPC.height,             // Высота NPC
            player.position,        // Позиция игрока
            player.width,           // Ширина игрока
            player.height           // Высота игрока
            );

            Lighting.AddLight(NPC.Center, 0.6f, 0.6f, 0.8f); // Добавляем свет вокруг NPC

            if (distanceToPlayer > 250 || !canSeePlayer)
            {
                NPC.velocity = direction * 2f + waveVector; // Двигаемся к игроку с постоянной скоростью
            }
            else if (distanceToPlayer < 100)
            {
                NPC.velocity = -direction * 2f + waveVector; // Отталкиваемся от игрока с постоянной скоростью
            }
            else
            {
                NPC.velocity = waveVector;
            }

            // --- Стрельба ТОЛЬКО если видит ---
            if (canSeePlayer && NPC.ai[0] > 60f)
            {
                NPC.ai[0] = 0;
                var source = NPC.GetSource_FromAI();
                Vector2 position = NPC.Center;
                Vector2 targetPosition = Main.player[NPC.target].Center;
                float speed = 10f;
                SoundEngine.PlaySound(SoundID.Item20, NPC.position);

                int damage = NPC.damage; //If the projectile is hostile, the damage passed into NewProjectile will be applied doubled, and quadrupled if expert mode, so keep that in mind when balancing projectiles if you scale it off NPC.damage (which also increases for expert/master)
                Projectile.NewProjectile(source, position, direction * speed, ModContent.ProjectileType<Sparkle>(), damage, 0f, Main.myPlayer);
            }
            else
            {
                NPC.ai[0]++;
            }





            // ====== АНИМАЦИЯ (ИСПРАВЛЕНА) ======
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                // frame — это Rectangle. Нужно двигать frame.Y, а не присваивать int.
                NPC.frame.Y += 40; // или += 40 (высота кадра)
                if (NPC.frame.Y >= Main.npcFrameCount[Type] * 40)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSkyHeight)
            {
                return 0.1f;
            }

            return 0f;
        }
    }
}