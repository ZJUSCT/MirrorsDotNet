using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Manager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Manager.Controllers;

[ApiController]
[Route("mirrors")]
public class MirrorsController : ControllerBase
{
    private readonly ILogger<MirrorsController> _logger;
    private readonly MirrorContext _context;
    private readonly IMapper _mapper;

    public MirrorsController(ILogger<MirrorsController> logger, MirrorContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// All mirror status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<MirrorItemDto>> GetAllMirrors()
    {
        _logger.LogInformation("Get Request: GetAllMirrors");
        var mirrorList = await _context.Mirrors.Include(mirror => mirror.Files).ToListAsync();
        var mirrorDtoList = mirrorList.Select(mirror => _mapper.Map<MirrorItemDto>(mirror)).ToList();
        return mirrorDtoList;
    }

    /// <summary>
    /// Mirror status query with id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MirrorItemDto>> GetMirror(string id)
    {
        _logger.LogInformation("Get Request: GetMirror {MirrorId}", id);
        var mirrorItem = await _context.Mirrors.Include(mirror => mirror.Files).FirstOrDefaultAsync(i => i.Id == id);
        if (mirrorItem == null)
        {
            return NotFound();
        }

        var mirrorDto = _mapper.Map<MirrorItemDto>(mirrorItem);
        return mirrorDto;
    }
}