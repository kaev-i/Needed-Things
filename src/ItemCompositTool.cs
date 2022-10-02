using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace NeededThings

{
    public class ItemCompositTool : Item, ITexPositionSource
    {
        public Size2i AtlasSize { get; set; }

        //string curMat, curLining;
        string curBlade, curHead, curHandle, curCrystal;
        //ITexPositionSource glassTextureSource;
        ITexPositionSource tmpTextureSource;


        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (textureCode == "blade") return tmpTextureSource[curBlade];
                if (textureCode == "head") return tmpTextureSource[curHead];
                if (textureCode == "handle") return tmpTextureSource[curHandle];
                if (textureCode == "crystal") return tmpTextureSource[curCrystal];
                //if (textureCode == "material-deco") return tmpTextureSource["deco-" + curMat];
                //if (textureCode == "lining") return tmpTextureSource[curLining == "plain" ? curMat : curLining];
                //if (textureCode == "glass") return glassTextureSource["material"];
                return tmpTextureSource[textureCode];
            }
        }

        public override int GetMaxDurability(ItemStack itemstack)
        {
            JsonObject handles = Attributes["compositeHandleDurability"];
            JsonObject heads = Attributes["compositeHeadModifier"];

            string currHandle = itemstack.Attributes.GetString("handle");
            string currHead = itemstack.Attributes.GetString("head");

            int handleDurability = 0;
            float headModifier = 0;

            if (handles != null && handles[currHandle] != null)
            {
                handleDurability = handles[currHandle].AsInt(0);
            }

            if (heads != null && heads[currHead] != null)
            {
                headModifier = heads[currHead].AsFloat(0);
            }
            if (itemstack.Collectible.Durability <= 0)
            {
                itemstack.Collectible.Durability = (int)(handleDurability * headModifier);
            }
            api.Logger.Debug("durability: " + itemstack.Collectible.Durability);
            return (int)(handleDurability * headModifier);
        }

        public override float GetAttackPower(IItemStack withItemstack)
        {
            JsonObject blades = Attributes["compositeBladeAttack"];
            JsonObject heads = Attributes["compositeHeadModifier"];

            string currBlades = withItemstack.Attributes.GetString("blade");
            string currHead = withItemstack.Attributes.GetString("head");

            int bladeAttackPower = 0;
            float headModifier = 0;

            if (blades != null && blades[currBlades] != null)
            {
                bladeAttackPower = blades[currBlades].AsInt(0);
            }

            if (heads != null && heads[currHead] != null)
            {
                headModifier = heads[currHead].AsFloat(0);
            }

            return (int)(bladeAttackPower * headModifier);
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            Dictionary<string, MeshRef> meshrefs = ObjectCacheUtil.GetOrCreate(capi, "itemComposittoolGuiMeshRefs", () =>
            {
                return new Dictionary<string, MeshRef>();
            });

            string blade = itemstack.Attributes.GetString("blade");
            string head = itemstack.Attributes.GetString("head");
            string handle = itemstack.Attributes.GetString("handle");
            string crystal = itemstack.Attributes.GetString("crystal");
            //string lining = itemstack.Attributes.GetString("lining");
            // string glass = itemstack.Attributes.GetString("glass", "quartz");

            string key = blade + "-" + head + "-" + handle + "-" + crystal;
            MeshRef meshref;
            if (!meshrefs.TryGetValue(key, out meshref))
            {
                AssetLocation shapeloc = Shape.Base.Clone().WithPathPrefix("shapes/").WithPathAppendix(".json");
                Shape shape = capi.Assets.TryGet(shapeloc).ToObject<Shape>();

                MeshData mesh = GenMesh(capi, blade, head, handle, crystal, shape);
                meshrefs[key] = meshref = capi.Render.UploadMesh(mesh);
            }

            renderinfo.ModelRef = meshref;
            renderinfo.CullFaces = false;
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            ICoreClientAPI capi = api as ICoreClientAPI;
            if (capi == null) return;

            object obj;
            if (capi.ObjectCache.TryGetValue("copmposittoolGuiMeshRefs", out obj))
            {
                Dictionary<string, MeshRef> meshrefs = obj as Dictionary<string, MeshRef>;

                foreach (var val in meshrefs)
                {
                    val.Value.Dispose();
                }

                capi.ObjectCache.Remove("copmposittoolGuiMeshRefs");
            }
        }

        public MeshData GenMesh(ICoreClientAPI capi, string blade, string head, string handle, string crystal, Shape shape = null, ITesselatorAPI tesselator = null)
        {
            if (tesselator == null) tesselator = capi.Tesselator;

            tmpTextureSource = tesselator.GetTextureSource(this);

            if (shape == null)
            {
                var shapeobj = capi.Assets.TryGet("shapes/" + this.Shape.Base.Path + ".json");
                if (shapeobj != null)
                {

                    shape = /*capi.Assets.TryGet("shapes/" + this.Shape.Base.Path + ".json")?*/shapeobj.ToObject<Shape>();
                }
            }

            if (shape == null)
            {
                return null;
            }

            this.AtlasSize = capi.BlockTextureAtlas.Size;
            curBlade = blade;
            curHead = head;
            curHandle = handle;
            curCrystal = crystal;

            //Block glassBlock = capi.World.GetBlock(new AssetLocation("glass-" + glassMaterial));
            //glassTextureSource = tesselator.GetTexSource(glassBlock);
            MeshData mesh;
            tesselator.TesselateShape("itemComposittool", shape, out mesh, this, new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ));
            return mesh;
        }
    }
}
