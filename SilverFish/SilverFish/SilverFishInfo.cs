using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SilverFish
{
    public class SilverFishInfo : GH_AssemblyInfo
    {
        public override string Name => Setting.Category;

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("533D7C9D-BDE6-4FAC-A03F-41BAD1274187");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
    public class Setting
    {
        public static readonly string Category = "SilverFish";
        internal class SubCategory
        {
            public static readonly string Utility = "Utility";
            public static readonly string Print = "Print";
        }
    }
    public class TestToggle
    {
        public static readonly bool CreateLayout = true;

    }
}