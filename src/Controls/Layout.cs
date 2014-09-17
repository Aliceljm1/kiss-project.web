using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;

namespace Kiss.Web.Controls
{
    [ParseChildren(typeof(Content))]
    public class Layout : TemplatedControl
    {
        List<Content> ctrls = new List<Content>();

        protected override void AddParsedSubObject(object obj)
        {
            Content ctrl = obj as Content;

            if (ctrl != null)
            {
                ctrls.Add(ctrl);

                // add ctrl to layout's children to force normal life circle
                Controls.Add(ctrl);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string layout = string.Empty;

            // remove contents before render it
            foreach (Control ctrl in ctrls)
            {
                Controls.Remove(ctrl);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(new StreamWriter(ms)))
                {
                    base.Render(htmlWriter);
                    htmlWriter.Flush();
                }

                using (StreamReader rdr = new StreamReader(ms))
                {
                    rdr.BaseStream.Position = 0;
                    layout = rdr.ReadToEnd();
                }
            }

            foreach (Control ctrl in ctrls)
            {
                string tag = "{" + ctrl.ID + "}";

                if (layout.IndexOf(tag) == -1)
                    throw new ArgumentException(string.Format("{0} not found!", tag));

                string content = string.Empty;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (HtmlTextWriter htmlwriter = new HtmlTextWriter(new StreamWriter(ms)))
                    {
                        ctrl.RenderControl(htmlwriter);
                        htmlwriter.Flush();
                    }

                    using (StreamReader rdr = new StreamReader(ms))
                    {
                        rdr.BaseStream.Position = 0;

                        content = rdr.ReadToEnd();
                    }
                }

                layout = layout.Replace(tag, content);
            }

            writer.Write(layout);
        }
    }
}
