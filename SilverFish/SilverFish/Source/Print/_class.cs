using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rhino.Display;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace SilverFish.Class
{
    public class PrintBook
    {
        private readonly List<PrintMedia> _media;
        internal readonly double _page_space_in_previewing = 10;
        internal PrintBook(List<PrintMedia> media)
        {
            this._media = media;
            for (int i = 0; i < media.Count; i++)
                media[i].PageNumber = i;
        }
        internal List<PrintMedia> Media { get { return _media; } }
    }
    internal class PrintMedia       //RhinoPageView
    {
        private readonly DefaultSize _default = DefaultSize.Custom;
        private PointF _origin = default;
        private SizeF _size;
        private PrintContents _content;
        internal object PageNumber;
        
        internal PrintMedia(DefaultSize @default)
        {
            if (@default != this._default)
            {
                this._default = @default;
                this._size = this.GetSize(@default);
            }
        }
        internal PrintMedia(SizeF custom)
        {
            this._default = DefaultSize.Custom;
            this._size = custom;
        }

        internal RectangleF Rectangle
        {
            get { return new RectangleF(this._origin, this._size); }
            set
            {
                this._origin = value.Location;
                this._size = value.Size;
            }
        }
        internal PrintContents Content
        {
            get { return this._content; }
            set
            {
                if (new RectangleF(this._origin, this._size).Contains(value.ContentsBoundingBox))
                    this._content = value;
                else
                    this._content = null;
            }
        }
        private SizeF GetSize(DefaultSize @default)
        {
            switch (@default)
            {
                case DefaultSize.A3:
                    return new SizeF(297, 420);
                case DefaultSize.A4:
                    return new SizeF(210, 297);
                case DefaultSize.B4:
                    return new SizeF(257, 364);
                case DefaultSize.B5:
                    return new SizeF(182, 257);
                default:
                    return default;
            }
        }
        internal enum DefaultSize
        {
            Custom,
            A3,
            A4,
            B4,
            B5,
        }
    }
    internal class PrintContents
    {
        private class ContentSetting
        {
            private readonly RectangleF _detailview;        //NOTE:DetailViewObject
            private readonly GeometryBase _geometry;        //NOTE:特別なことをしない限り、Detail以外はすべてこれ。場合によってContentTypeで分類。

            private readonly string _name;
            private readonly ContentType _type;

            internal enum ContentType
            {
                Detail,
                ParametricText,                             //TODO:ノンブルとか。Geometryから区分する。
                Geometry,
            }

            internal ContentSetting(string name, object content)
            {
                if (content is RectangleF)
                {
                    this._type = ContentType.Detail;
                    this._detailview = (RectangleF)content;
                }
                else if (content is GeometryBase)
                {
                    this._type = ContentType.Geometry;
                    this._geometry = (GeometryBase)content;
                }
                else return;
                this._name = name;
            }
            internal string Name { get { return this._name; } }
            private interface IContent                      //TODO:プロパティの実装
            {
            }
            internal RectangleF Detail
            {
                get
                {
                    if (this._type == ContentType.Detail)
                        return this._detailview;
                    else
                        return RectangleF.Empty;
                }
            }
            internal GeometryBase Geometry
            {
                get
                {
                    if (this._type == ContentType.Geometry)
                        return this._geometry;
                    else
                        return null;
                }
            }
        }
        private List<ContentSetting> _contents;
        internal void Add(string name,object content)
        {
            ContentSetting setting = new ContentSetting(name, content);
            this._contents.Add(setting);
        }
        internal List<string> Names { get { return this._contents.Select(x => x.Name).ToList(); } }
        internal List<object> Content
        {
            get
            {
                return this._contents.Select(c =>
                {
                    if (c.Detail != RectangleF.Empty)
                        return c.Detail as object;
                    else if (c.Geometry != null)
                        return c.Geometry as object;
                    else return null;
                }).ToList();
            }
        }
        internal RectangleF ContentsBoundingBox
        {
            get
            {
                BoundingBox bb = BoundingBox.Empty;
                foreach (object i in this._contents)
                {
                    BoundingBox bb_i;
                    if (i is Rectangle)
                    {
                        Rectangle r = (Rectangle)i;
                        Plane p = new Plane
                        {
                            OriginX = r.Location.X,
                            OriginY = r.Location.Y,
                            XAxis = Vector3d.XAxis,
                            YAxis = Vector3d.YAxis,
                        };
                        bb_i = new Rectangle3d(p, r.Width, -r.Height).ToNurbsCurve().GetBoundingBox(true);
                    }
                    else if (i is GeometryBase)
                    {
                        GeometryBase g = i as GeometryBase;
                        bb_i = g.GetBoundingBox(true);
                    }
                    else
                        bb_i = BoundingBox.Empty;
                    bb.Union(bb_i);
                }
                return new RectangleF()
                {
                    Location = new PointF
                    {
                        X = (float)bb.PointAt(0, 1, 0).X,
                        Y = (float)bb.PointAt(0, 1, 0).Y,
                    },
                    Width = (float)bb.Diagonal.X,
                    Height = (float)bb.Diagonal.Y,
                };
            }
        }
    }

    namespace Parameter
    {
        public class PrintGoo : GH_GeometricGoo<PrintBook>, IGH_PreviewData
            //参考：https://www.grasshopper3d.com/forum/topics/custom-data-type-gh-geometricgoo-or-gh-goo?commentId=2985220%3AComment%3A586611
        {
            public PrintGoo(PrintBook print) { this.Value = print; }
            public PrintGoo(PrintGoo print) { this.Value = print.Value; }
            public override string TypeName { get; }
            public override string TypeDescription { get; }
            public override IGH_Goo Duplicate() => this;                                        //?
            public override string ToString()
            {
                if (this.IsValid)
                    return "Null Printer";
                else
                    return "Printer";
            }
            public override BoundingBox Boundingbox => BoundingBox.Empty;
            public override BoundingBox GetBoundingBox(Transform xform) => BoundingBox.Empty;
            public override IGH_GeometricGoo DuplicateGeometry() => this;                       //?
            #region xform methods
            public override IGH_GeometricGoo Transform(Transform xform)                         //TODO:
            {
                var a = this.Value.Media;
                return new GH_Brep();
            }
            public override IGH_GeometricGoo Morph(SpaceMorph xmorph)                           //TODO:
            {
                return new GH_Brep();
            }
            #endregion
            #region preview methods
            public BoundingBox ClippingBox => this.Boundingbox;
            public void DrawViewportMeshes(GH_PreviewMeshArgs args)                             //TODO:
            {

            }
            public void DrawViewportWires(GH_PreviewWireArgs args)                              //TODO:
            {

            }
            #endregion
        }
        public class PrintParameter : GH_PersistentParam<PrintGoo>
        {
            public PrintParameter() : base(null, null, null, null, null) { }
            public override Guid ComponentGuid => new Guid("25be6aba-a014-4a70-85c8-3fcdb16eac6e");
            protected override Bitmap Icon => null;
            protected override GH_GetterResult Prompt_Singular(ref PrintGoo value) => GH_GetterResult.cancel;
            protected override GH_GetterResult Prompt_Plural(ref List<PrintGoo> values) => GH_GetterResult.cancel;
            protected override ToolStripMenuItem Menu_CustomSingleValueItem()
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = "Not available",
                    Visible = false
                };
                return item;
            }
            protected override ToolStripMenuItem Menu_CustomMultiValueItem()
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = "Not available",
                    Visible = false
                };
                return item;
            }
        }
    }
}