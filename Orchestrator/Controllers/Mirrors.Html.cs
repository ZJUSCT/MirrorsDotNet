using Microsoft.AspNetCore.Mvc;
using Orchestrator.Utils;

namespace Orchestrator.Controllers;

using static MiniHtmlGenerator;

public partial class Mirrors
{
    [HttpGet("html")]
    [Produces("text/html")]
    public string GetMirrorsHtml()
    {
        var mirrors = jobQueue.GetMirrorItems();
        return PlainPage(
            "Mirrors overview", [
                Table([
                    THead([
                        Th("ID"),
                        Th("URL"),
                        Th("Name"),
                        Th("Upstream"),
                        Th("Size"),
                        Th("Status"),
                        Th("LastSyncAt"),
                        Th("NextSyncAt"),
                    ]),
                    TBody(mirrors
                        .Select(kv => kv.Value)
                        .Select(x => Tr([
                            Th(x.Config.Id),
                            Td(x.Config.Info.Url),
                            Td(x.Config.Info.Name.En),
                            Td(x.Config.Info.Upstream),
                            Td(x.Size.ToString()),
                            Td(StatusToString(x.Status)),
                            Td(x.LastSyncAt.ToLongTimeString()),
                            Td(x.NextSyncAt().ToLongTimeString()),
                        ]))
                        .ToList())
                ])
            ],
            css: TableCss
        ).ToString();
    }
}