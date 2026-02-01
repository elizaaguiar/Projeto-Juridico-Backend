using JuridicoAnalise.Application.Interfaces;
using JuridicoAnalise.Domain.Enums;
using JuridicoAnalise.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace JuridicoAnalise.Infrastructure.Services;

public class ClassificationService : IClassificationService
{
    private readonly IPalavraChaveRepository _palavraChaveRepository;
    private readonly ILogger<ClassificationService> _logger;

    private static readonly Dictionary<TipoDocumento, string[]> DefaultKeywords = new()
    {
        [TipoDocumento.Execucao] = new[] { "INDICAR MEIOS", "SEGUIMENTO DA EXECUÇÃO", "ENDEREÇO", "SOBRESTAMENTO", "EXECUÇÃO FISCAL", "PENHORA" },
        [TipoDocumento.Alvara] = new[] { "EXPEDIDO ALVARÁ", "ALVARÁ DE LEVANTAMENTO", "ALVARÁ JUDICIAL" },
        [TipoDocumento.PericiaQuesitos] = new[] { "AGENDAMENTO", "PERÍCIA", "QUESITOS", "LAUDO PERICIAL", "PERITO" },
        [TipoDocumento.Audiencia] = new[] { "AUDIÊNCIA", "DESIGNADA AUDIÊNCIA", "PAUTA DE AUDIÊNCIA", "AUDIÊNCIA DE INSTRUÇÃO" },
        [TipoDocumento.Sentenca] = new[] { "SENTENÇA", "JULGO PROCEDENTE", "JULGO IMPROCEDENTE", "DISPOSITIVO" },
        [TipoDocumento.Despacho] = new[] { "DESPACHO", "DETERMINO", "INTIME-SE", "CITE-SE" },
        [TipoDocumento.Citacao] = new[] { "CITAÇÃO", "CITADO", "MANDADO DE CITAÇÃO" },
        [TipoDocumento.Intimacao] = new[] { "INTIMAÇÃO", "INTIMADO", "INTIMAR" },
        [TipoDocumento.Recurso] = new[] { "RECURSO", "APELAÇÃO", "AGRAVO", "EMBARGOS" }
    };

    // Palavras-chave para classificação de SETOR
    private static readonly Dictionary<string, string[]> SetorKeywords = new()
    {
        ["EXECUÇÃO"] = new[] { "INDICAR MEIOS", "SEGUIMENTO DA EXECUÇÃO", "SEGUIMENTO DA EXECUCAO", "ENDEREÇO", "ENDERECO", "SOBRESTAMENTO" },
        ["ALVARÁ"] = new[] { "EXPEDIDO ALVARÁ", "EXPEDIDO ALVARA" },
        ["PERÍCIA/QUESITOS"] = new[] { "AGENDAMENTO", "PERÍCIA", "PERICIA", "QUESITOS" },
        ["FALAR DE LAUDO"] = new[] { "MANIFESTAÇÃO AO LAUDO", "MANIFESTACAO AO LAUDO", "IMPUGNAÇÃO AO LAUDO", "IMPUGNACAO AO LAUDO" },
        ["DADOS BANCÁRIOS"] = new[] { "INFORMAR DADOS BANCÁRIOS", "INFORMAR DADOS BANCARIOS", "CONTAS", "DADOS PARA TRANSFERÊNCIA", "DADOS PARA TRANSFERENCIA", "HONORÁRIOS", "HONORARIOS" },
        ["RECURSAL"] = new[] { "TURMA", "ACÓRDÃO", "ACORDAO", "DECISÃO", "DECISAO", "CONTRARRAZÕES", "CONTRARRAZOES", "CONTRAMINUTA", "EMBARGOS", "SENTENÇA", "SENTENCA" },
        ["AUDIÊNCIA"] = new[] { "AUDIÊNCIA DESIGNADA", "AUDIENCIA DESIGNADA", "AUDIÊNCIA REDESIGNADA", "AUDIENCIA REDESIGNADA", "AUDIÊNCIA CANCELADA", "AUDIENCIA CANCELADA", "DATA DA AUDIÊNCIA", "DATA DA AUDIENCIA", "HORA DA AUDIÊNCIA", "HORA DA AUDIENCIA", "PAUTA DE AUDIÊNCIA", "PAUTA DE AUDIENCIA" },
        ["CÁLCULOS"] = new[] { "CONTÁBIL", "CONTABIL", "ARTIGOS DE LIQUIDAÇÃO", "ARTIGOS DE LIQUIDACAO", "PLANILHA DE CÁLCULOS", "PLANILHA DE CALCULOS", "FALAR DE CÁLCULOS", "FALAR DE CALCULOS", "CÁLCULOS", "CALCULOS" },
        ["INICIAL"] = new[] { "EMENDA A INICIAL", "CONEXÃO", "CONEXAO", "JUNTAR INICIAL", "PROCURAÇÃO", "PROCURACAO" },
        ["MANIFESTAÇÃO"] = new[] { "REGULARIZAR POLO", "LITISPENDÊNCIA", "LITISPENDENCIA", "CONEXÃO", "CONEXAO", "EXCEÇÃO DE INCOMPETÊNCIA", "EXCECAO DE INCOMPETENCIA" },
        ["DOCUMENTOS"] = new[] { "APRESENTE", "JUNTAR", "TRAZER" },
        ["CHC"] = new[] { "HABILITAÇÃO DE CRÉDITO", "HABILITACAO DE CREDITO", "CHC" },
        ["IMPUGNAÇÃO"] = new[] { "MANIFESTAÇÃO AOS DOCUMENTOS", "MANIFESTACAO AOS DOCUMENTOS", "IMPUGNAÇÃO", "IMPUGNACAO" }
    };

    // Padrão CNJ: 0000000-00.0000.0.00.0000
    private static readonly Regex ProcessoRegex = new(
        @"\b(\d{7}-\d{2}\.\d{4}\.\d\.\d{2}\.\d{4})\b",
        RegexOptions.Compiled);

    // Padrões alternativos de número de processo
    private static readonly Regex ProcessoAlternativoRegex = new(
        @"\b(\d{3,7}[\./-]\d{2,4}[\./-]?\d{0,4}[\./-]?\d{0,4})\b",
        RegexOptions.Compiled);

    private static readonly Regex DataRegex = new(
        @"\b(\d{2}/\d{2}/\d{4})\b",
        RegexOptions.Compiled);

    // Palavras que indicam início de uma nova publicação
    private static readonly string[] PublicationMarkers = new[]
    {
        "INTIMAÇÃO", "DESPACHO", "SENTENÇA", "CITAÇÃO", "EDITAL",
        "ALVARÁ", "AUDIÊNCIA", "RECURSO", "DECISÃO", "ACÓRDÃO",
        "MANDADO", "EXECUÇÃO", "PENHORA", "PERÍCIA"
    };

    public ClassificationService(IPalavraChaveRepository palavraChaveRepository, ILogger<ClassificationService> logger)
    {
        _palavraChaveRepository = palavraChaveRepository;
        _logger = logger;
    }

    public async Task<TipoDocumento> ClassifyDocumentAsync(string content)
    {
        var result = await ClassifyAndExtractAsync(content);
        return result.Tipo;
    }

    public async Task<DocumentClassificationResult> ClassifyAndExtractAsync(string content)
    {
        var result = new DocumentClassificationResult();
        var contentUpper = content.ToUpperInvariant();

        // Extrair número do processo
        var processoMatch = ProcessoRegex.Match(content);
        if (processoMatch.Success)
        {
            result.NumeroProcesso = processoMatch.Value;
        }

        // Extrair data de publicação
        var dataMatch = DataRegex.Match(content);
        if (dataMatch.Success && DateTime.TryParse(dataMatch.Value, out var data))
        {
            result.DataPublicacao = data;
        }

        // Buscar palavras-chave do banco de dados
        var palavrasChave = await _palavraChaveRepository.GetAllAsync();
        var keywordsFromDb = palavrasChave
            .GroupBy(p => p.TipoDocumento)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Termo.ToUpperInvariant()).ToArray()
            );

        // Combinar com keywords padrão
        var allKeywords = new Dictionary<TipoDocumento, string[]>();
        foreach (var tipo in Enum.GetValues<TipoDocumento>())
        {
            var dbKeywords = keywordsFromDb.GetValueOrDefault(tipo, Array.Empty<string>());
            var defaultKw = DefaultKeywords.GetValueOrDefault(tipo, Array.Empty<string>());
            allKeywords[tipo] = dbKeywords.Concat(defaultKw).Distinct().ToArray();
        }

        // Classificar por correspondência de palavras-chave
        var scores = new Dictionary<TipoDocumento, int>();
        foreach (var (tipo, keywords) in allKeywords)
        {
            var score = keywords.Count(keyword => contentUpper.Contains(keyword));
            if (score > 0)
            {
                scores[tipo] = score;
            }
        }

        if (scores.Count > 0)
        {
            var maxScore = scores.Max(s => s.Value);
            var bestMatch = scores.First(s => s.Value == maxScore);
            result.Tipo = bestMatch.Key;
            result.Confidence = (double)bestMatch.Value / allKeywords[bestMatch.Key].Length;
        }
        else
        {
            result.Tipo = TipoDocumento.Outros;
            result.Confidence = 0;
        }

        // Classificar setor baseado em palavras-chave
        result.Setor = ClassifySetor(contentUpper);

        return result;
    }

    private string ClassifySetor(string contentUpper)
    {
        _logger.LogInformation("=== CLASSIFICAÇÃO DE SETOR ===");
        _logger.LogInformation("Conteúdo (primeiros 500 chars): {Content}", contentUpper.Length > 500 ? contentUpper.Substring(0, 500) : contentUpper);

        var setorScores = new Dictionary<string, int>();

        foreach (var (setor, keywords) in SetorKeywords)
        {
            var matchedKeywords = keywords.Where(keyword => contentUpper.Contains(keyword.ToUpperInvariant())).ToList();
            var score = matchedKeywords.Count;

            if (score > 0)
            {
                setorScores[setor] = score;
                _logger.LogInformation("Setor: {Setor} - Palavras encontradas: {Keywords} - Score: {Score}", setor, string.Join(", ", matchedKeywords), score);
            }
        }

        if (setorScores.Count > 0)
        {
            var maxScore = setorScores.Max(s => s.Value);
            var setorEscolhido = setorScores.First(s => s.Value == maxScore).Key;
            _logger.LogInformation("SETOR ESCOLHIDO: {Setor} (score: {Score})", setorEscolhido, maxScore);
            return setorEscolhido;
        }

        _logger.LogInformation("Nenhum setor identificado - retornando N/A");
        return "N/A";
    }

    public async Task<List<DocumentClassificationResult>> ExtractMultiplePublicationsAsync(string content)
    {
        var results = new List<DocumentClassificationResult>();

        // Encontrar todas as ocorrências de números de processo
        var processoMatches = ProcessoRegex.Matches(content);

        if (processoMatches.Count <= 1)
        {
            // Se houver 0 ou 1 processo, tenta dividir por palavras-chave
            var publications = SplitByKeywords(content);

            if (publications.Count <= 1)
            {
                // Se não conseguir dividir, processa como documento único
                var singleResult = await ClassifyAndExtractAsync(content);
                results.Add(singleResult);
                return results;
            }

            foreach (var pub in publications)
            {
                if (!string.IsNullOrWhiteSpace(pub))
                {
                    var result = await ClassifyAndExtractAsync(pub);
                    if (result.NumeroProcesso != null || result.Tipo != TipoDocumento.Outros)
                    {
                        results.Add(result);
                    }
                }
            }
        }
        else
        {
            // Dividir o conteúdo baseado nas posições dos números de processo
            var positions = new List<(int start, string processo)>();

            foreach (Match match in processoMatches)
            {
                positions.Add((match.Index, match.Value));
            }

            // Ordenar por posição
            positions = positions.OrderBy(p => p.start).ToList();

            for (int i = 0; i < positions.Count; i++)
            {
                int start = positions[i].start;
                int end = (i + 1 < positions.Count) ? positions[i + 1].start : content.Length;

                // Pegar um pouco antes do número do processo para contexto
                int contextStart = Math.Max(0, start - 200);
                var block = content.Substring(contextStart, end - contextStart);

                var result = await ClassifyAndExtractAsync(block);
                result.NumeroProcesso = positions[i].processo;
                results.Add(result);
            }
        }

        // Se não encontrou nenhuma publicação, retorna ao menos uma com o documento inteiro
        if (results.Count == 0)
        {
            var singleResult = await ClassifyAndExtractAsync(content);
            results.Add(singleResult);
        }

        return results;
    }

    private List<string> SplitByKeywords(string content)
    {
        var publications = new List<string>();
        var contentUpper = content.ToUpperInvariant();

        // Encontrar todas as posições de marcadores
        var markerPositions = new List<int>();

        foreach (var marker in PublicationMarkers)
        {
            int index = 0;
            while ((index = contentUpper.IndexOf(marker, index)) != -1)
            {
                markerPositions.Add(index);
                index += marker.Length;
            }
        }

        if (markerPositions.Count <= 1)
        {
            publications.Add(content);
            return publications;
        }

        // Ordenar posições
        markerPositions = markerPositions.Distinct().OrderBy(p => p).ToList();

        // Dividir conteúdo
        for (int i = 0; i < markerPositions.Count; i++)
        {
            int start = markerPositions[i];
            int end = (i + 1 < markerPositions.Count) ? markerPositions[i + 1] : content.Length;

            var block = content.Substring(start, end - start).Trim();

            // Só adiciona se o bloco tiver conteúdo significativo (mais de 50 caracteres)
            if (block.Length > 50)
            {
                publications.Add(block);
            }
        }

        return publications;
    }
}
