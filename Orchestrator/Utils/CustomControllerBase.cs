using Microsoft.AspNetCore.Mvc;

namespace Orchestrator.Utils;

public class CustomControllerBase(IConfiguration conf) : ControllerBase
{
    private const string StatusOk = "ok";
    private const string StatusError = "error";

    protected object Success(object? data)
    {
        return new ResponseDto(StatusOk, data);
    }

    protected object Error(string? message = null, object? data = null)
    {
        return new ResponseDto(StatusError, data, message);
    }

    protected string? GetRequestIp()
    {
        var realIpHeader = conf["RealIPHeader"];
        return string.IsNullOrWhiteSpace(realIpHeader)
            ? Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            : Request.Headers[realIpHeader].FirstOrDefault();
    }

    private record ResponseDto(string Status, object? Data, string? Message = null);
}