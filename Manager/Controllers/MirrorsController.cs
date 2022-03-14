using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

[ApiController]
[Route("mirrors")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    private readonly MirrorContext _context;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;

    public MirrorsController(ILogger<MirrorsController> logger, MirrorContext context, IDistributedCache cache,
        IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
        _mapper = mapper;
    }

    /// <summary>
    /// All mirror status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Mirror.MirrorItemDto>> GetAllMirrors()
    {
        var serializedString = await _cache.GetStringAsync(Utils.Constants.MirrorAllCacheKey);
        if (serializedString != null)
        {
            return JsonSerializer.Deserialize<List<Mirror.MirrorItemDto>>(serializedString);
        }

        _logger.LogInformation("Status cache miss, regenerating");

        var mirrorList = await _context.Mirrors.ToListAsync();
        var mirrorDtoList = mirrorList.Select(mirror => _mapper.Map<Mirror.MirrorItemDto>(mirror)).ToList();

        await _cache.SetStringAsync(Utils.Constants.MirrorAllCacheKey, JsonSerializer.Serialize(mirrorDtoList));
        _logger.LogInformation("Wrote status to cache");

        return mirrorDtoList;
    }

    /// <summary>
    /// Mirror status query with id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Mirror.MirrorItemDto>> GetMirror(string id)
    {
        var serializedString = await _cache.GetStringAsync(Utils.Constants.MirrorItemCacheKeyPrefix + id);
        if (serializedString != null)
        {
            return JsonSerializer.Deserialize<Mirror.MirrorItemDto>(serializedString);
        }

        var mirrorItem = await _context.Mirrors.FindAsync(id);
        if (mirrorItem == null)
        {
            return NotFound();
        }

        var mirrorDto = _mapper.Map<Mirror.MirrorItemDto>(mirrorItem);
        await _cache.SetStringAsync(Utils.Constants.MirrorItemCacheKeyPrefix + id, JsonSerializer.Serialize(mirrorDto));
        _logger.LogInformation("Wrote ${Id} status to cache", id);

        return mirrorDto;
    }
}