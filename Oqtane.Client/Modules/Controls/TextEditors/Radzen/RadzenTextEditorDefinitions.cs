using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

namespace Oqtane.Modules.Controls
{
    public sealed class RadzenEditorDefinitions
    {
        public static IStringLocalizer<Oqtane.Modules.Controls.RadzenTextEditor> Localizer { get; internal set; }

        public const string TransparentBackgroundColor = "rgba(0, 0, 0, 0)";

        public const string LightBackgroundColor = "rgba(255, 255, 255, 1)";

        public const string DarkBackgroundColor = "rgba(0, 0, 0, 1)";

        public const string DefaultTheme = "default";

        public const string DefaultBackground = "Default";

        public static readonly IDictionary<string, Func<RenderTreeBuilder, int, int>> ToolbarItems = new Dictionary<string, Func<RenderTreeBuilder, int, int>>()
        {
            { "AlignCenter", (builder, sequence) => CreateFragment(builder, sequence, "AlignCenter", "RadzenHtmlEditorAlignCenter") },
            { "AlignLeft", (builder, sequence) => CreateFragment(builder, sequence, "AlignLeft", "RadzenHtmlEditorAlignLeft") },
            { "AlignRight", (builder, sequence) => CreateFragment(builder, sequence, "AlignRight", "RadzenHtmlEditorAlignRight") },
            { "Background", (builder, sequence) => CreateFragment(builder, sequence, "Background", "RadzenHtmlEditorBackground") },
            { "Color", (builder, sequence) => CreateFragment(builder, sequence, "Color", "RadzenHtmlEditorColor") },
            { "FontName", (builder, sequence) => CreateFragment(builder, sequence, "FontName", "RadzenHtmlEditorFontName") },
            { "FontSize", (builder, sequence) => CreateFragment(builder, sequence, "FontSize", "RadzenHtmlEditorFontSize") },
            { "FormatBlock", (builder, sequence) => CreateFragment(builder, sequence, "FormatBlock", "RadzenHtmlEditorFormatBlock") },
            { "Indent", (builder, sequence) => CreateFragment(builder, sequence, "Indent", "RadzenHtmlEditorIndent") },
            { "InsertImage", (builder, sequence) => CreateFragment(builder, sequence, "InsertImage", "RadzenHtmlEditorCustomTool", "InsertImage", "image") },
            { "Bold", (builder, sequence) => CreateFragment(builder, sequence, "Italic", "RadzenHtmlEditorBold") },
            { "Italic", (builder, sequence) => CreateFragment(builder, sequence, "Italic", "RadzenHtmlEditorItalic") },
            { "Justify", (builder, sequence) => CreateFragment(builder, sequence, "Justify", "RadzenHtmlEditorJustify") },
            { "Link", (builder, sequence) => CreateFragment(builder, sequence, "InsertLink", "RadzenHtmlEditorCustomTool", "InsertLink", "insert_link") },
            { "OrderedList", (builder, sequence) => CreateFragment(builder, sequence, "OrderedList", "RadzenHtmlEditorOrderedList") },
            { "Outdent", (builder, sequence) => CreateFragment(builder, sequence, "Outdent", "RadzenHtmlEditorOutdent") },
            { "Redo", (builder, sequence) => CreateFragment(builder, sequence, "Redo", "RadzenHtmlEditorRedo") },
            { "RemoveFormat", (builder, sequence) => CreateFragment(builder, sequence, "RemoveFormat", "RadzenHtmlEditorRemoveFormat") },
            { "Separator", (builder, sequence) => CreateFragment(builder, sequence, "Separator", "RadzenHtmlEditorSeparator") },
            { "Source", (builder, sequence) => CreateFragment(builder, sequence, "Source", "RadzenHtmlEditorSource") },
            { "StrikeThrough", (builder, sequence) => CreateFragment(builder, sequence, "StrikeThrough", "RadzenHtmlEditorStrikeThrough") },
            { "Subscript", (builder, sequence) => CreateFragment(builder, sequence, "Subscript", "RadzenHtmlEditorSubscript") },
            { "Superscript", (builder, sequence) => CreateFragment(builder, sequence, "Superscript", "RadzenHtmlEditorSuperscript") },
            { "Underline", (builder, sequence) => CreateFragment(builder, sequence, "Underline", "RadzenHtmlEditorUnderline") },
            { "Undo", (builder, sequence) => CreateFragment(builder, sequence, "Undo", "RadzenHtmlEditorUndo") },
            { "Unlink", (builder, sequence) => CreateFragment(builder, sequence, "Unlink", "RadzenHtmlEditorUnlink") },
            { "UnorderedList", (builder, sequence) => CreateFragment(builder, sequence, "UnorderedList", "RadzenHtmlEditorUnorderedList") },
        };

        public static readonly string DefaultToolbarItems = "Undo,Redo,Separator,FontName,FontSize,FormatBlock,Bold,Italic,Underline,StrikeThrough,Separator,AlignLeft,AlignCenter,AlignRight,Justify,Separator,Indent,Outdent,UnorderedList,OrderedList,Separator,Color,Background,RemoveFormat,Separator,Subscript,Superscript,Separator,Link,Unlink,InsertImage,Separator,Source";

        private static int CreateFragment(RenderTreeBuilder builder, int sequence, string name, string typeName, string commaneName = "", string icon = "")
        {
            var fullTypeName = $"Radzen.Blazor.{typeName}, Radzen.Blazor";
            var type = Type.GetType(fullTypeName);
            if (type != null)
            {
                var title = Localizer[$"{name}.Title"];
                var placeholder = Localizer[$"{name}.Placeholder"];
                builder.OpenComponent(sequence++, type);
                if (!string.IsNullOrEmpty(title) && title != $"{name}.Title" && type.GetProperty("Title") != null)
                {
                    builder.AddAttribute(sequence++, "Title", title);
                }
                if (!string.IsNullOrEmpty(placeholder) &&  placeholder != $"{name}.Placeholder" && type.GetProperty("Placeholder") != null)
                {
                    builder.AddAttribute(sequence++, "Placeholder", placeholder);
                }
                if (!string.IsNullOrEmpty(commaneName) && type.GetProperty("CommandName") != null)
                {
                    builder.AddAttribute(sequence++, "CommandName", commaneName);
                }
                if (!string.IsNullOrEmpty(icon) && type.GetProperty("Icon") != null)
                {
                    builder.AddAttribute(sequence++, "Icon", icon);
                }
                builder.CloseComponent();
            }

            return sequence;
        }
    }
}
