using ClosedXML.Excel;
using JuridicoAnalise.Application.Interfaces;
using JuridicoAnalise.Domain.Entities;

namespace JuridicoAnalise.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public Task<byte[]> ExportDocumentosAsync(IEnumerable<Documento> documentos)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Documentos");

        // Cabeçalhos
        worksheet.Cell(1, 1).Value = "PROCESSO";
        worksheet.Cell(1, 2).Value = "SETOR";
        worksheet.Cell(1, 3).Value = "PALAVRA-CHAVE";
        worksheet.Cell(1, 4).Value = "DATA PUBLICAÇÃO";
        worksheet.Cell(1, 5).Value = "INÍCIO PRAZO";
        worksheet.Cell(1, 6).Value = "ARQUIVO";

        // Estilizar cabeçalho
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dados
        int row = 2;
        foreach (var doc in documentos)
        {
            worksheet.Cell(row, 1).Value = doc.NumeroProcesso;
            worksheet.Cell(row, 2).Value = doc.Setor;
            worksheet.Cell(row, 3).Value = doc.PalavraChaveUsada ?? "";
            worksheet.Cell(row, 4).Value = doc.DataPublicacao.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = doc.InicioPrazo?.ToString("dd/MM/yyyy") ?? "";
            worksheet.Cell(row, 6).Value = doc.NomeArquivo;
            row++;
        }

        // Ajustar largura das colunas
        worksheet.Columns().AdjustToContents();

        // Aplicar filtros
        worksheet.RangeUsed()?.SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<MemoryStream> ExportToStreamAsync(IEnumerable<Documento> documentos)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Documentos");

        // Cabeçalhos
        worksheet.Cell(1, 1).Value = "PROCESSO";
        worksheet.Cell(1, 2).Value = "SETOR";
        worksheet.Cell(1, 3).Value = "PALAVRA-CHAVE";
        worksheet.Cell(1, 4).Value = "DATA PUBLICAÇÃO";
        worksheet.Cell(1, 5).Value = "INÍCIO PRAZO";
        worksheet.Cell(1, 6).Value = "ARQUIVO";

        // Estilizar cabeçalho
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        headerRange.Style.Font.FontColor = XLColor.White;

        // Dados
        int row = 2;
        foreach (var doc in documentos)
        {
            worksheet.Cell(row, 1).Value = doc.NumeroProcesso;
            worksheet.Cell(row, 2).Value = doc.Setor;
            worksheet.Cell(row, 3).Value = doc.PalavraChaveUsada ?? "";
            worksheet.Cell(row, 4).Value = doc.DataPublicacao.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = doc.InicioPrazo?.ToString("dd/MM/yyyy") ?? "";
            worksheet.Cell(row, 6).Value = doc.NomeArquivo;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.RangeUsed()?.SetAutoFilter();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return Task.FromResult(stream);
    }
}
