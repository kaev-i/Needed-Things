using Vintagestory.API.Common;

namespace NeededThings

{
    public class Core : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemComposittool", typeof(ItemCompositTool));
            api.RegisterBlockClass("BlockToolBench", typeof(BlockToolBench));
        }
    }
}
