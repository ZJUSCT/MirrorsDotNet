using Microsoft.AspNetCore.Mvc;

namespace Orchestrator.Utils;

public class CustomControllerBase(IConfiguration conf) : ControllerBase
{
    private const string StatusOk = "ok";
    private const string StatusError = "error";
    protected const string TableCss =
        """
        table {
          border-collapse: collapse;
          border: 2px solid rgb(140 140 140);
        }

        th,
        td {
          border: 1px solid rgb(160 160 160);
          padding: 8px 10px;
        }
        """;


    protected IConfiguration Conf => conf;

    protected static ResponseDto<T> Success<T>(T? data)
    {
        return new ResponseDto<T>(StatusOk, data);
    }

    protected static ResponseDto<object> Error(string? message = null)
    {
        return new ResponseDto<object>(StatusError, null, message);
    }
    
    protected static ResponseDto<T> Error<T>(string? message = null, T? data = default)
    {
        return new ResponseDto<T>(StatusError, data, message);
    }

    protected string? GetRequestIp()
    {
        var realIpHeader = conf["RealIPHeader"];
        return string.IsNullOrWhiteSpace(realIpHeader)
            ? Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            : Request.Headers[realIpHeader].FirstOrDefault();
    }

    public record ResponseDto<T>(string Status, T? Data, string? Message = null);
}
