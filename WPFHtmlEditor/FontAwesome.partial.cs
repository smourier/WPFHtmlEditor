using System;
using System.Reflection;
using System.Text;
using System.Windows.Markup;

namespace FontAwesome
{
    [MarkupExtensionReturnType(typeof(string))]
    public class GlyphsExtension : MarkupExtension
    {
        public GlyphsExtension(string glyph)
        {
            glyphs = glyph;
        }

        [ConstructorArgument("glyphs")]
        public string glyphs { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (glyphs == null)
                return null;

            StringBuilder sb = new StringBuilder();
            foreach (string g in glyphs.Split(new[] { '|', ':', '_' }, StringSplitOptions.RemoveEmptyEntries))
            {
                FieldInfo fi = typeof(FontAwesomeResource).GetField(g);
                if (fi == null)
                    continue;

                sb.AppendFormat("{0}", fi.GetValue(null));
            }
            return sb.ToString();
        }
    }
}
