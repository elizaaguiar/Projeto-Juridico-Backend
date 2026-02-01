using AutoMapper;
using JuridicoAnalise.Application.DTOs;
using JuridicoAnalise.Application.Interfaces;
using JuridicoAnalise.Domain.Entities;
using JuridicoAnalise.Domain.Enums;
using JuridicoAnalise.Domain.Interfaces;

namespace JuridicoAnalise.Application.Services;

public class DocumentoService : IDocumentoService
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnifiedDocumentReaderService _documentReaderService;
    private readonly IClassificationService _classificationService;
    private readonly IExcelExportService _excelExportService;
    private readonly IMapper _mapper;

    public DocumentoService(
        IDocumentoRepository documentoRepository,
        IUnifiedDocumentReaderService documentReaderService,
        IClassificationService classificationService,
        IExcelExportService excelExportService,
        IMapper mapper)
    {
        _documentoRepository = documentoRepository;
        _documentReaderService = documentReaderService;
        _classificationService = classificationService;
        _excelExportService = excelExportService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DocumentoDto>> GetAllAsync()
    {
        var documentos = await _documentoRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<DocumentoDto>>(documentos);
    }

    public async Task<DocumentoDetalheDto?> GetByIdAsync(Guid id)
    {
        var documento = await _documentoRepository.GetByIdAsync(id);
        return documento != null ? _mapper.Map<DocumentoDetalheDto>(documento) : null;
    }

    public async Task<DocumentoDto> ProcessarDocumentoAsync(Stream fileStream, string fileName, UploadDocumentoDto uploadDto)
    {
        var documento = new Documento
        {
            NomeArquivo = fileName,
            Setor = uploadDto.Setor ?? "N/A",
            CaminhoArquivo = fileName
        };

        try
        {
            // Verificar se o tipo de arquivo é suportado
            if (!_documentReaderService.CanRead(fileName))
            {
                var supportedExtensions = string.Join(", ", _documentReaderService.GetAllSupportedExtensions());
                throw new NotSupportedException(
                    $"Tipo de arquivo não suportado. Extensões suportadas: {supportedExtensions}");
            }

            // Extrair texto do documento
            var content = await _documentReaderService.ExtractTextAsync(fileStream, fileName);
            documento.Conteudo = content;

            // Classificar documento
            var classificacao = await _classificationService.ClassifyAndExtractAsync(content);
            documento.NumeroProcesso = classificacao.NumeroProcesso ?? "N/A";
            documento.DataPublicacao = classificacao.DataPublicacao ?? DateTime.UtcNow;

            if (!string.IsNullOrEmpty(classificacao.Setor))
            {
                documento.Setor = classificacao.Setor;
            }

        }
        catch (Exception ex)
        {
            documento.MensagemErro = ex.Message;
            documento.NumeroProcesso = "ERRO";
            documento.DataPublicacao = DateTime.UtcNow;
        }

        var saved = await _documentoRepository.AddAsync(documento);
        return _mapper.Map<DocumentoDto>(saved);
    }

    public async Task<IEnumerable<DocumentoDto>> ProcessarMultiplosAsync(
        IEnumerable<(Stream stream, string fileName)> files,
        UploadDocumentoDto uploadDto)
    {
        var resultados = new List<DocumentoDto>();

        foreach (var (stream, fileName) in files)
        {
            var resultado = await ProcessarDocumentoAsync(stream, fileName, uploadDto);
            resultados.Add(resultado);
        }

        return resultados;
    }

    public async Task<DocumentoDto> AtualizarAsync(Guid id, AtualizarDocumentoDto dto)
    {
        var documento = await _documentoRepository.GetByIdAsync(id);
        if (documento == null)
        {
            throw new KeyNotFoundException($"Documento com ID {id} não encontrado.");
        }

        if (!string.IsNullOrEmpty(dto.Setor))
            documento.Setor = dto.Setor;

        if (!string.IsNullOrEmpty(dto.Responsavel))

        if (dto.InicioPrazo.HasValue)
            documento.InicioPrazo = dto.InicioPrazo;

        if (dto.Tipo.HasValue)

        await _documentoRepository.UpdateAsync(documento);
        return _mapper.Map<DocumentoDto>(documento);
    }

    public async Task DeletarAsync(Guid id)
    {
        await _documentoRepository.DeleteAsync(id);
    }

    public async Task<byte[]> ExportarParaExcelAsync(TipoDocumento? tipo = null)
    {
        IEnumerable<Documento> documentos;

            documentos = await _documentoRepository.GetAllAsync();

        return await _excelExportService.ExportDocumentosAsync(documentos);
    }

    public async Task<byte[]> ProcessarEExportarAsync(IEnumerable<(Stream stream, string fileName)> files)
    {
        var documentos = new List<Documento>();

        foreach (var (stream, fileName) in files)
        {
            try
            {
                if (!_documentReaderService.CanRead(fileName))
                {
                    var documento = new Documento
                    {
                        NomeArquivo = fileName,
                        Setor = "N/A",
                        CaminhoArquivo = fileName,
                        MensagemErro = "Tipo de arquivo não suportado",
                        NumeroProcesso = "ERRO",
                        DataPublicacao = DateTime.UtcNow,
                    };
                    documentos.Add(documento);
                }
                else
                {
                    var content = await _documentReaderService.ExtractTextAsync(stream, fileName);

                    // Extrair múltiplas publicações do documento
                    var publicacoes = await _classificationService.ExtractMultiplePublicationsAsync(content);

                    foreach (var pub in publicacoes)
                    {
                        var documento = new Documento
                        {
                            NomeArquivo = fileName,
                            Setor = pub.Setor ?? "N/A",
                            CaminhoArquivo = fileName,
                            NumeroProcesso = pub.NumeroProcesso ?? "N/A",
                            DataPublicacao = pub.DataPublicacao ?? DateTime.UtcNow
                        };
                        documentos.Add(documento);
                    }
                }
            }
            catch (Exception ex)
            {
                var documento = new Documento
                {
                    NomeArquivo = fileName,
                    Setor = "N/A",
                    CaminhoArquivo = fileName,
                    MensagemErro = ex.Message,
                    NumeroProcesso = "ERRO",
                    DataPublicacao = DateTime.UtcNow,
                };
                documentos.Add(documento);
            }
        }

        return await _excelExportService.ExportDocumentosAsync(documentos);
    }
}
