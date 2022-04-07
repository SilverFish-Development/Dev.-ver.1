using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System;
using System.Drawing;
using Eto.Forms;
using GH_IO.Serialization;

namespace SilverFish.Component
{
    public class BrowseForFolder : GH_Param<GH_String>
    {
        public BrowseForFolder() :
            base(new GH_InstanceDescription(
                "Browse For Folder", "Browse",
                "",
                Setting.Category, Setting.SubCategory.Utility
                ))
        { }
        #region collect data
        private string _folder = null;
        protected override void CollectVolatileData_Custom()
        {
            this.VolatileData.Clear();
            this.AddVolatileData(new GH_Path(0), 0, new GH_String(this._folder));
            if (this._folder == null)
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Double-click a left button to get a folder path.");
        }
        #endregion
        #region (de)serialization
        private static readonly string IoPath = "Folder Path";
        public override bool Write(GH_IWriter writer)
        {
            writer.SetString(IoPath, this._folder);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            this._folder = reader.GetString(IoPath);
            return base.Read(reader);
        }
        #endregion
        #region setting component
        public override Guid ComponentGuid => new Guid("6234f30d-d223-47c2-b3ab-bdefe0bc88c5");
        protected override Bitmap Icon => SilverFish.Properties.Resources.Browse_For_Folder;
        public override void CreateAttributes() => this.m_attributes = new BFF_Attributes(this);
        #endregion

        internal class BFF_Attributes : GH_Attributes<BrowseForFolder>
        {
            internal BFF_Attributes(BrowseForFolder owner) : base(owner)
            {
                RectangleF rect = new RectangleF(this.Pivot, new SizeF(this.Owner.Icon.Size));
                rect.Width *= 2;
                this.Bounds = rect;
            }
            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    SelectFolderDialog folder = new SelectFolderDialog();
                    DialogResult result = folder.ShowDialog(null);
                    if (result == DialogResult.Ok)
                        this.Owner._folder = folder.Directory;
                    this.Owner.ExpireSolution(true);
                }
                return base.RespondToMouseDown(sender, e);
            }
            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                if (channel != GH_CanvasChannel.Objects)
                    return;
                base.Render(canvas, graphics, channel);

                GH_CapsuleRenderEngine.RenderOutputGrip(graphics, canvas.Viewport.Zoom, this.OutputGrip, true);

                GH_Capsule capsule = GH_Capsule.CreateCapsule(this.Bounds, this.Owner._folder != null ? GH_Palette.Normal : GH_Palette.Warning);
                capsule.Render(graphics, this.Owner.Icon, this.Selected, this.Owner.Locked, true);

                capsule.Dispose();
            }
            public override bool HasInputGrip => false;
            public override bool HasOutputGrip => true;
        }
    }
}