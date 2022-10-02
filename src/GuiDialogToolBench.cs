using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace NeededThings
{
    internal class GuiDialogToolBench : GuiDialogBlockEntity
    {
        public GuiDialogToolBench(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, blockEntityPos, capi)
        {
        }
    }
}
