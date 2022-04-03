using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SilverFish
{
    public class SilverFishInfo : GH_AssemblyInfo
    {
        public override string Name => "SilverFish";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("314D9B5B-2741-46F7-8BEA-FA1327610F66");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}