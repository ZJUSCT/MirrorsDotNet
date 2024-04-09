using System.Text;
using System.Web;

namespace Orchestrator.Utils;

using HtmlAttributes = Dictionary<string, string?>;
using HtmlNodes = List<HtmlNode>;

public class HtmlNode(
    string? tag,
    HtmlAttributes? attributes = null,
    HtmlNodes? children = null)
{
    public string? Tag { get; set; } = tag ?? null;
    public HtmlAttributes Attributes { get; set; } = attributes ?? new HtmlAttributes();
    public HtmlNodes Children { get; set; } = children ?? [];
    public string Content { get; set; } = string.Empty;
    public bool EscapeContent { get; set; } = true;

    private static readonly List<string> VoidElements =
    [
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    ];

    protected void RecursiveToString(StringBuilder sb)
    {
        if (!string.IsNullOrWhiteSpace(Tag))
        {
            var escapedTag = HttpUtility.HtmlEncode(Tag);
            sb.Append('<').Append(escapedTag);
            foreach (var (k, v) in Attributes)
            {
                sb.Append(' ').Append(HttpUtility.HtmlEncode(k));
                if (v == null) continue;
                sb.Append("=\"").Append(HttpUtility.HtmlEncode(v)).Append('"');
            }

            sb.Append('>');

            if (VoidElements.Contains(Tag))
            {
                return;
            }
        }

        sb.Append(EscapeContent ? HttpUtility.HtmlEncode(Content) : Content);

        foreach (var child in Children)
        {
            child.RecursiveToString(sb);
        }

        if (!string.IsNullOrWhiteSpace(Tag))
        {
            sb.Append("</").Append(HttpUtility.HtmlEncode(Tag)).Append('>');
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        RecursiveToString(sb);
        return sb.ToString();
    }
}

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
public static class MiniHtmlGenerator
{
    public static HtmlNode HtmlRoot(HtmlAttributes? attributes, HtmlNodes? children) =>
        CustomEl(string.Empty, [
            DangerousUnescaped("<!DOCTYPE html>"),
            Html(attributes, children)
        ]);

    public static HtmlNode PlainPage(string title, HtmlNodes children, string script = "", string css = "") => HtmlRoot(
        new()
        {
            ["lang"] = "en"
        }, [
            Head([
                Meta(new()
                {
                    ["charset"] = "utf-8"
                }),
                Title(title),
                CustomEl("script", content: script, attributes: new()
                {
                    ["type"] = "application/json"
                }),
                CustomEl("style", content: css)
            ]),
            Body(children)
        ]);

    private static HtmlNode Html(HtmlAttributes? attributes = null, HtmlNodes? children = null) =>
        new("html", attributes, children);

    public static HtmlNode DangerousUnescaped(string content) => new(null) { Content = content, EscapeContent = false };
    public static HtmlNode PlainText(string content) => new(null) { Content = content };

    public static HtmlNode CustomEl(string tag, HtmlNodes? children = null,
        string content = "", HtmlAttributes? attributes = null) =>
        new(tag, attributes, children) { Content = content };

    public static HtmlNode Head(HtmlNodes? children = null) => new("head", null, children);
    public static HtmlNode Body(HtmlNodes? children = null) => new("body", null, children);
    public static HtmlNode Meta(HtmlAttributes? attributes = null) => new("meta", attributes);
    public static HtmlNode Title(string title) => new("title") { Content = title };

    public static HtmlNode P(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("p", attributes, children) { Content = content };

    public static HtmlNode Table(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("table", attributes, children) { Content = content };

    public static HtmlNode Tr(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("tr", attributes, children) { Content = content };

    public static HtmlNode Th(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("th", attributes, children) { Content = content };

    public static HtmlNode Td(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("td", attributes, children) { Content = content };

    public static HtmlNode THead(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("thead", attributes, children) { Content = content };

    public static HtmlNode TBody(HtmlNodes? children = null, HtmlAttributes? attributes = null, string content = "") =>
        new("tbody", attributes, children) { Content = content };

    public static HtmlNode P(string content = "") => new("p") { Content = content };

    public static HtmlNode Table(string content = "") => new("table") { Content = content };

    public static HtmlNode Tr(string content = "") => new("tr") { Content = content };

    public static HtmlNode Th(string content = "") => new("th") { Content = content };

    public static HtmlNode Td(string content = "") => new("td") { Content = content };

    public static HtmlNode THead(string content = "") => new("thead") { Content = content };
}
// ReSharper restore ArrangeObjectCreationWhenTypeNotEvident