using JuridicoAnalise.Application.DTOs;
using JuridicoAnalise.Application.Services;
using JuridicoAnalise.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace JuridicoAnalise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;
    private readonly ILogger<DocumentosController> _logger;

    // Extensões de arquivo suportadas
    private static readonly string[] AllowedExtensions = new[]
    {
        ".pdf", ".docx", ".doc",           // Documentos
        ".xlsx", ".xls", ".xlsm",          // Planilhas
        ".txt", ".csv", ".rtf",            // Texto
        ".xml", ".json", ".html", ".htm"   // Outros
    };

    public DocumentosController(IDocumentoService documentoService, ILogger<DocumentosController> logger)
    {
        _documentoService = documentoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentoDto>>> GetAll()
    {
        var documentos = await _documentoService.GetAllAsync();
        return Ok(documentos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentoDetalheDto>> GetById(Guid id)
    {
        var documento = await _documentoService.GetByIdAsync(id);
        if (documento == null)
        {
            return NotFound();
        }
        return Ok(documento);
    }

    [HttpGet("extensoes-suportadas")]
    public ActionResult<string[]> GetExtensoesSuportadas()
    {
        return Ok(AllowedExtensions);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<ActionResult<DocumentoDto>> Upload(IFormFile file, [FromForm] string setor, [FromForm] string? responsavel)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Arquivo não fornecido.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest($"Tipo de arquivo não suportado. Extensões permitidas: {string.Join(", ", AllowedExtensions)}");
        }

        _logger.LogInformation("Processando arquivo: {FileName} ({Extension})", file.FileName, extension);

        using var stream = file.OpenReadStream();
        var uploadDto = new UploadDocumentoDto(file.FileName, setor, responsavel);
        var resultado = await _documentoService.ProcessarDocumentoAsync(stream, file.FileName, uploadDto);

        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }

    [HttpPost("upload-multiple")]
    [RequestSizeLimit(200_000_000)] // 200MB para múltiplos arquivos
    public async Task<ActionResult<IEnumerable<DocumentoDto>>> UploadMultiple(
        IFormFileCollection files,
        [FromForm] string setor,
        [FromForm] string? responsavel)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("Nenhum arquivo fornecido.");
        }

        // Validar todas as extensões
        var invalidFiles = files
            .Where(f => !AllowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
            .Select(f => f.FileName)
            .ToList();

        if (invalidFiles.Any())
        {
            return BadRequest($"Arquivos com tipo não suportado: {string.Join(", ", invalidFiles)}. Extensões permitidas: {string.Join(", ", AllowedExtensions)}");
        }

        _logger.LogInformation("Processando {Count} arquivo(s)", files.Count);

        var uploadDto = new UploadDocumentoDto(string.Empty, setor, responsavel);
        var fileStreams = files.Select(f => (f.OpenReadStream() as Stream, f.FileName));

        var resultados = await _documentoService.ProcessarMultiplosAsync(fileStreams, uploadDto);
        return Ok(resultados);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentoDto>> Update(Guid id, [FromBody] AtualizarDocumentoDto dto)
    {
        try
        {
            var documento = await _documentoService.AtualizarAsync(id, dto);
            return Ok(documento);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _documentoService.DeletarAsync(id);
        return NoContent();
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar([FromQuery] TipoDocumento? tipo = null)
    {
        var bytes = await _documentoService.ExportarParaExcelAsync(tipo);
        var fileName = $"documentos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("processar-exportar")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ProcessarEExportar(IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("Nenhum arquivo fornecido.");
        }

        var invalidFiles = files
            .Where(f => !AllowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
            .Select(f => f.FileName)
            .ToList();

        if (invalidFiles.Any())
        {
            return BadRequest($"Arquivos com tipo não suportado: {string.Join(", ", invalidFiles)}");
        }

        _logger.LogInformation("Processando e exportando {Count} arquivo(s)", files.Count);

        var fileStreams = files.Select(f => (f.OpenReadStream() as Stream, f.FileName));
        var bytes = await _documentoService.ProcessarEExportarAsync(fileStreams);

        var fileName = $"documentos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
