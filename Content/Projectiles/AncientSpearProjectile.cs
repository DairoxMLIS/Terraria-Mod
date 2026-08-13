using Dairox_Mod.Content.Dusts;
using Dairox_Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.Projectiles
{
    public class AncientSpearProjectile : ModProjectile
    {
        protected virtual float HoldoutRangeMin => -10f;
        protected virtual float HoldoutRangeMax => 24f;
        protected bool fired = false;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true; // скрывает, пока не в руке
            Projectile.scale = 2f;
            Projectile.aiStyle = -1;

            Main.projFrames[Projectile.type] = 2;
            Projectile.frame = 0;
            Projectile.frameCounter = 0;


        }


        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner]; // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            int duration = player.itemAnimationMax; // Define the duration the projectile will exist in frames

            player.heldProj = Projectile.whoAmI; // Update the player's held projectile id

            // Reset projectile time left if necessary
            if (Projectile.timeLeft > duration)
            {
                Projectile.timeLeft = duration;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8) // меняем кадр каждые 8 тиков
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0; // зацикливаем
                }
            }


            Projectile.velocity = Vector2.Normalize(Projectile.velocity); // Velocity isn't used in this spear implementation, but we use the field to store the spear's attack direction.

            float halfDuration = duration * 0.5f;
            float progress;

            // Here 'progress' is set to a value that goes from 0.0 to 1.0 and back during the item use animation.
            if (Projectile.timeLeft < halfDuration)
            {
                progress = Projectile.timeLeft / halfDuration;
            }
            else
            {
                progress = (duration - Projectile.timeLeft) / halfDuration;
            }
            Vector2 offset = new Vector2(0, 8f);
            // Move the projectile from the HoldoutRangeMin to the HoldoutRangeMax and back, using SmoothStep for easing the movement
            Projectile.Center = player.MountedCenter + offset + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Если летит влево — отражаем и разворачиваем на 180°
            if (Projectile.velocity.X < 0)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation += MathHelper.Pi;
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation += MathHelper.ToRadians(90f);
            }

            // КОРРЕКЦИЯ для диагонального спрайта (вычитаем 45°)
            Projectile.rotation -= MathHelper.ToRadians(45f);

            if (!Main.dedServ)
            {
                // These dusts are added later, for the 'ExampleMod' effect
                if (Main.rand.NextBool(3))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Projectile.velocity.X * 2f, Projectile.velocity.Y * 2f, Alpha: 128, Scale: 1.2f);
                }

                if (Main.rand.NextBool(4))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<AncientSparkle>(), Alpha: 128, Scale: 0.3f);
                }
            }
            if (progress >=0.9f && !fired)
            {
                fired = true;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, 
                    Projectile.velocity * 5f,
                    ModContent.ProjectileType<AncientSpearProjectile_2>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);

            }



            return false; // Don't execute vanilla AI.
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Создаём новый хитбокс. Его центр будет на 30 пикселей впереди от центра снаряда.
            // 30f — это примерное расстояние до острия, подбери своё значение.
            Vector2 tipOffset = Projectile.velocity * 50f;
            Vector2 tipCenter = Projectile.Center + tipOffset;

            // Размер хитбокса на конце. Например, 16x16, как у копья.
            hitbox = new Rectangle(
                (int)(tipCenter.X - 50 / 2),
                (int)(tipCenter.Y - 36 / 2),
                50,
                25
              
            );
        }
    }
}
