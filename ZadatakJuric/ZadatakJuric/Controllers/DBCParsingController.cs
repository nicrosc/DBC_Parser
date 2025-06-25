using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZadatakJuric.Application.DBCParser.Commands;
using ZadatakJuric.Application.DBCParser.Dto;
using ZadatakJuric.Application.DBCParser.Queries;

namespace ZadatakJuric.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DBCParsingController : ControllerBase
{
    private readonly IMediator _mediator;

    public DBCParsingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get overview of all parsed DBC files
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of DBC files with summary statistics</returns>
    [HttpGet("overview")]
    public async Task<ActionResult<DBCOverviewResponse>> GetOverview(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDBCOverviewQuery
            {
                Page = Math.Max(1, page),
                PageSize = Math.Min(100, Math.Max(1, pageSize)) // Limit page size to 100
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving DBC overview: {ex.Message}");
        }
    }

    /// <summary>
    /// Search DBC files with filters
    /// </summary>
    /// <param name="searchTerm">Search term for file names, descriptions, etc.</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Filtered and paginated search results</returns>
    [HttpGet("search")]
    public async Task<ActionResult<SearchDBCResponse>> SearchDBC(
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var command = new SearchDBCFilesCommand
            {
                SearchTerm = searchTerm,
                FromDate = fromDate,
                ToDate = toDate,
                Page = Math.Max(1, page),
                PageSize = Math.Min(100, Math.Max(1, pageSize))
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem($"Error searching DBC files: {ex.Message}");
        }
    }

    /// <summary>
    /// Parse a DBC file and extract network, messages, and signals
    /// </summary>
    /// <param name="file">The DBC file to parse</param>
    /// <returns>Parsed DBC data with network structure</returns>
    [HttpPost("parse")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<object>> ParseDbcFile(IFormFile file)
    {
        // Validate file presence
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Validate file extension
        if (!file.FileName.EndsWith(".dbc", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .dbc files are supported");

        // Validate file size (50MB limit)
        if (file.Length > 50 * 1024 * 1024)
            return BadRequest("File size must be less than 50MB");

        try
        {
            // Read file content
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            
            // Create command
            var command = new ParseDBCFileCommand
            {
                FileContent = content,
                FileName = file.FileName
            };

            // Execute parsing via MediatR
            var result = await _mediator.Send(command);
            
            return Ok(new
            {
                data = result.Data,
                wasAlreadyParsed = result.WasAlreadyParsed,
                message = result.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid file format: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log the exception in a real application
            return Problem($"Error parsing DBC file: {ex.Message}");
        }
    }

    /// <summary>
    /// Get detailed information about a specific DBC file
    /// </summary>
    /// <param name="id">DBC file ID</param>
    /// <returns>Detailed DBC information including networks, messages, and signals</returns>
    [HttpGet("details/{id}")]
    public async Task<ActionResult<DBCDto>> GetDBCDetails(int id)
    {
        try
        {
            // Create a proper query to get full DBC details with networks
            var query = new GetDBCDetailsQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"DBC file with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving DBC details: {ex.Message}");
        }
    }
}
