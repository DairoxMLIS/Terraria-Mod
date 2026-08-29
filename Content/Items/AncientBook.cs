using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Dairox_Mod.Content.Items
{

    public class AncientBook : ModItem
    {
        private const int AnimationTicksPerFrame = 6;
        private const int AnimationFrameCount = 4;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;

            Main.RegisterItemAnimation(
                Item.type,
                new DrawAnimationVertical(
                    AnimationTicksPerFrame,
                    AnimationFrameCount
                )
            );
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 1);
        }
    }
}
