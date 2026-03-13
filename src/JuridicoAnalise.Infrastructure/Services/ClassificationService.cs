using JuridicoAnalise.Application.Interfaces;
using JuridicoAnalise.Domain.Entities;
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

    // Expressões que indicam que a publicação NÃO deve aparecer na planilha (Rule 6)
    private static readonly string[] ExclusaoExpressions = new[]
    {
        "FALE O RECLAMADO", "FALAR O RECLAMADO",
        "OFICIE-SE O RECLAMADO", "OFICIE O RECLAMADO",
        "MANIFESTE-SE O RECLAMADO", "MANIFESTE O RECLAMADO"
    };

    // Palavras-chave para classificação de SETOR
    private static readonly Dictionary<string, string[]> SetorKeywords = new()
    {
        ["EXECUÇÃO"] = new[]
        {
            "INDICAR MEIOS", "SEGUIMENTO DA EXECUÇÃO", "SEGUIMENTO DA EXECUCAO",
            "SOBRESTAMENTO", "EXECUÇÃO FISCAL", "EXECUCAO FISCAL",
            "PENHORA", "HASTA PÚBLICA", "HASTA PUBLICA", "LEILÃO", "LEILAO",
            "EXPROPRIAÇÃO", "EXPROPRIACAO", "SISBAJUD",
            "BLOQUEIO DE CREDITO", "BLOQUEIO DE CRÉDITO",
            "ATOS EXECUTORIOS", "ATOS EXECUTÓRIOS",
            "INICIADA A EXECUCAO", "INICIADA A EXECUÇÃO",
            "ORIENTAR A EXECUCAO", "ORIENTAR A EXECUÇÃO",
            // Fase de execução identificada pela palavra "exequente" (Rule 2)
            "EXEQUENTE", "EXECUTADO", "CUMPRIMENTO DE SENTENÇA", "CUMPRIMENTO DE SENTENCA",
            "FASE EXECUTÓRIA", "FASE EXECUTORIA", "FASE DE EXECUÇÃO", "FASE DE EXECUCAO"
        },
        ["DESCUMPRIMENTO DE ACORDO"] = new[]
        {
            // Rule 4: movimentações que mencionem descumprimento de acordo
            "DESCUMPRIMENTO DE ACORDO", "DESCUMPRIMENTO DO ACORDO",
            "DESCUMPRIMENTO DO ACORDO HOMOLOGADO", "INADIMPLEMENTO DO ACORDO",
            "ACORDO NÃO CUMPRIDO", "ACORDO NAO CUMPRIDO",
            "QUEBRA DO ACORDO", "RESCISÃO DO ACORDO", "RESCISAO DO ACORDO"
        },
        ["ALVARÁ"] = new[] { "EXPEDIDO ALVARÁ", "EXPEDIDO ALVARA", "ALVARÁ DE LEVANTAMENTO", "ALVARA DE LEVANTAMENTO", "ALVARÁ JUDICIAL", "ALVARA JUDICIAL" },
        ["PERÍCIA/QUESITOS"] = new[]
        {
            // Rule 5: "realização da perícia" não deve ser classificada como audiência
            "REALIZAÇÃO DA PERÍCIA", "REALIZACAO DA PERICIA",
            "REALIZAÇÃO DE PERÍCIA", "REALIZACAO DE PERICIA",
            "AGENDAMENTO DA PERÍCIA", "AGENDAMENTO DE PERICIA",
            "PERÍCIA MÉDICA", "PERICIA MEDICA",
            "PERÍCIA", "PERICIA", "QUESITOS", "LAUDO PERICIAL", "PERITO",
            "PROVA PERICIAL", "EXAME PERICIAL"
        },
        ["FALAR DE LAUDO"] = new[] { "MANIFESTAÇÃO AO LAUDO", "MANIFESTACAO AO LAUDO", "IMPUGNAÇÃO AO LAUDO", "IMPUGNACAO AO LAUDO", "LAUDO COMPLEMENTAR", "FALAR SOBRE O LAUDO", "MANIFESTAR SOBRE LAUDO" },
        ["DADOS BANCÁRIOS"] = new[] { "INFORMAR DADOS BANCÁRIOS", "INFORMAR DADOS BANCARIOS", "CONTAS", "DADOS PARA TRANSFERÊNCIA", "DADOS PARA TRANSFERENCIA", "HONORÁRIOS", "HONORARIOS", "DEPÓSITO", "DEPOSITO", "PAGAMENTO" },
        ["RECURSAL"] = new[]
        {
            // Rule 8: quando aparecer sentença, classificar como fase recursal
            "SENTENÇA", "SENTENCA", "SENTENÇA PROFERIDA", "SENTENCA PROFERIDA",
            "FASE RECURSAL", "TURMA", "ACÓRDÃO", "ACORDAO",
            "CONTRARRAZÕES", "CONTRARRAZOES", "CONTRAMINUTA",
            "EMBARGOS DE DECLARAÇÃO", "EMBARGOS DE DECLARACAO",
            "EMBARGOS", "RECURSO", "APELAÇÃO", "APELACAO", "AGRAVO",
            "RECURSO ORDINÁRIO", "RECURSO ORDINARIO"
        },
        ["AUDIÊNCIA"] = new[]
        {
            // Rule 1: classificar como audiência somente quando houver indicação explícita
            "AUDIÊNCIA DESIGNADA", "AUDIENCIA DESIGNADA",
            "AUDIÊNCIA REDESIGNADA", "AUDIENCIA REDESIGNADA",
            "AUDIÊNCIA CANCELADA", "AUDIENCIA CANCELADA",
            "DATA DA AUDIÊNCIA", "DATA DA AUDIENCIA",
            "HORA DA AUDIÊNCIA", "HORA DA AUDIENCIA",
            "PAUTA DE AUDIÊNCIA", "PAUTA DE AUDIENCIA",
            "FOI REDESIGNADA", "FOI DESIGNADA",
            "AUDIENCIA UNA", "AUDIÊNCIA UNA",
            "AUSENCIA DO RECLAMANTE", "AUSÊNCIA DO RECLAMANTE",
            "REDESIGNO AUDIENCIA", "REDESIGNO AUDIÊNCIA",
            "REDESIGNO A AUDIENCIA", "REDESIGNO A AUDIÊNCIA",
            "REDESIGNA A AUDIENCIA", "REDESIGNA A AUDIÊNCIA",
            "REDESIGNA-SE A AUDIENCIA", "REDESIGNA-SE A AUDIÊNCIA",
            "REDESIGNACAO DA AUDIENCIA", "REDESIGNAÇÃO DA AUDIÊNCIA",
            "AUDIENCIA DE INSTRUÇÃO", "AUDIENCIA DE INSTRUCAO", "AUDIÊNCIA DE INSTRUÇÃO",
            "DESIGNO AUDIENCIA", "DESIGNO AUDIÊNCIA",
            "INSTRUÇÃO E JULGAMENTO", "INSTRUCAO E JULGAMENTO",
            "TENTATIVA DE CONCILIACAO", "TENTATIVA DE CONCILIAÇÃO",
            "AUDIENCIA INICIAL", "AUDIÊNCIA INICIAL"
        },
        ["CÁLCULOS"] = new[]
        {
            // Rule 7: "Cálculos" é diferente de "Impugnação aos cálculos"
            "CONTÁBIL", "CONTABIL",
            "ARTIGOS DE LIQUIDAÇÃO", "ARTIGOS DE LIQUIDACAO",
            "PLANILHA DE CÁLCULOS", "PLANILHA DE CALCULOS",
            "FALAR DE CÁLCULOS", "FALAR DE CALCULOS",
            "APRESENTE CÁLCULOS", "APRESENTE CALCULOS",
            "LIQUIDAÇÃO", "LIQUIDACAO",
            "ATUALIZAÇÃO DE CÁLCULOS", "ATUALIZACAO DE CALCULOS",
            "APRESENTAR CÁLCULOS", "APRESENTAR CALCULOS"
        },
        ["IMPUGNAÇÃO AOS CÁLCULOS"] = new[]
        {
            // Rule 7: impugnação aos cálculos é distinto de cálculos e de impugnação
            "IMPUGNAÇÃO AOS CÁLCULOS", "IMPUGNACAO AOS CALCULOS",
            "IMPUGNAR OS CÁLCULOS", "IMPUGNAR OS CALCULOS",
            "IMPUGNAÇÃO À LIQUIDAÇÃO", "IMPUGNACAO A LIQUIDACAO",
            "IMPUGNAÇÃO AO CÁLCULO", "IMPUGNACAO AO CALCULO"
        },
        ["INICIAL"] = new[] { "EMENDA A INICIAL", "CONEXÃO", "CONEXAO", "JUNTAR INICIAL", "PROCURAÇÃO", "PROCURACAO", "EMENDA À INICIAL", "REGULARIZAR INICIAL", "COMPLEMENTAR INICIAL" },
        ["MANIFESTAÇÃO"] = new[]
        {
            "REGULARIZAR POLO", "LITISPENDÊNCIA", "LITISPENDENCIA",
            "EXCEÇÃO DE INCOMPETÊNCIA", "EXCECAO DE INCOMPETENCIA",
            "PRAZO PARA MANIFESTAÇÃO", "PRAZO PARA MANIFESTACAO",
            "VISTA DOS AUTOS", "MANIFESTE-SE", "MANIFESTE",
            "FALAR SOBRE A PETICAO", "FALAR SOBRE A PETIÇÃO",
            // "Falem as partes" — despacho pedindo manifestação de ambas as partes (diferente de "fale o reclamado" que é excluído)
            "FALEM AS PARTES", "FALAR AS PARTES", "MANIFESTEM-SE AS PARTES",
            "PRONUNCIEM-SE AS PARTES"
        },
        ["DOCUMENTOS"] = new[]
        {
            "APRESENTE DOCUMENTOS", "JUNTAR DOCUMENTOS", "TRAZER DOCUMENTOS",
            "JUNTADA DE DOCUMENTOS", "DOCUMENTOS FALTANTES",
            // Solicitação de endereço para expedição de ofício
            "INFORME O ENDERECO", "INFORME O ENDEREÇO",
            "INFORMAR O ENDERECO", "INFORMAR O ENDEREÇO",
            "INFORMAR ENDERECO", "INFORMAR ENDEREÇO"
        },
        ["CHC"] = new[] { "HABILITAÇÃO DE CRÉDITO", "HABILITACAO DE CREDITO", "CHC", "CRÉDITO HABILITADO", "CREDITO HABILITADO" },
        ["IMPUGNAÇÃO"] = new[] { "MANIFESTAÇÃO AOS DOCUMENTOS", "MANIFESTACAO AOS DOCUMENTOS", "IMPUGNAÇÃO", "IMPUGNACAO", "IMPUGNAR", "CONTESTAÇÃO", "CONTESTACAO" }
    };

    // Marcador de publicação no documento: "Publicação: 1 de 219"
    private static readonly Regex PublicacaoMarcadorRegex = new(
        @"Publicação\s*:\s*\d+\s*de\s*\d+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Padrão CNJ genérico — usado para extrair o número de processo de dentro de um bloco
    private static readonly Regex ProcessoRegex = new(
        @"\b(\d{7}-\d{2}\.\d{4}\.\d\.\d{2}\.\d{4})\b",
        RegexOptions.Compiled);

    private static readonly Regex DataRegex = new(
        @"\b(\d{2}/\d{2}/\d{4})\b",
        RegexOptions.Compiled);

    // Regex para localizar data próxima a palavra-chave de audiência (Rule 1: extrair data da audiência)
    private static readonly Regex AudienciaComDataRegex = new(
        @"(?:AUDIÊN?CIA|AUDIENCIA|DESIGNAD[AO]|REDESIGNAD[AO])[^0-9]{1,120}(\d{2}/\d{2}/\d{4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Regex para localizar data de realização da perícia
    private static readonly Regex PericiaComDataRegex = new(
        @"(?:REALIZAC[AÃ]O\s+DA\s+PERIC[ÍI]A|DATA\s*(?:DA\s+PERIC[ÍI]A)?\s*:)[^0-9]{0,60}(\d{2}/\d{2}/\d{4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        var palavrasChave = await _palavraChaveRepository.GetAllAsync();
        return ClassifyAndExtractInternal(content, palavrasChave);
    }

    private DocumentClassificationResult ClassifyAndExtractInternal(string content, IEnumerable<PalavraChave> palavrasChave)
    {
        var result = new DocumentClassificationResult();
        var contentUpper = content.ToUpperInvariant();

        // Rule 6: verificar expressões de exclusão ("fale o reclamado", "oficie-se o reclamado")
        // Publicações com essas expressões não devem aparecer na planilha
        if (ExclusaoExpressions.Any(expr => contentUpper.Contains(expr)))
        {
            result.Setor = "N/A";
            result.Tipo = TipoDocumento.Outros;
            return result;
        }

        // Extrair número do processo
        var processoMatch = ProcessoRegex.Match(content);
        if (processoMatch.Success)
        {
            result.NumeroProcesso = processoMatch.Value;
        }

        // Extrair data de publicação (primeiro match = data da publicação)
        var dataMatch = DataRegex.Match(content);
        if (dataMatch.Success && DateTime.TryParse(dataMatch.Value, out var data))
        {
            result.DataPublicacao = data;
        }

        // Combinar keywords do banco com as padrão
        var keywordsFromDb = palavrasChave
            .GroupBy(p => p.TipoDocumento)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Termo.ToUpperInvariant()).ToArray()
            );

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
        var (setor, palavraChave) = ClassifySetor(contentUpper);
        result.Setor = setor;
        result.PalavraChaveUsada = palavraChave;

        // Rule 1: quando for audiência, extrair a DATA DA AUDIÊNCIA como InicioPrazo
        if (result.Setor == "AUDIÊNCIA")
        {
            result.InicioPrazo = ExtractAudienciaDate(content);
        }
        // Para perícia, extrair a data de realização do exame como InicioPrazo
        else if (result.Setor == "PERÍCIA/QUESITOS")
        {
            result.InicioPrazo = ExtractPericiaDate(content);
        }

        return result;
    }

    private static DateTime? ExtractPericiaDate(string content)
    {
        var match = PericiaComDataRegex.Match(content);
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var date))
            return date;
        return null;
    }

    private static DateTime? ExtractAudienciaDate(string content)
    {
        // Tenta encontrar uma data que aparece logo após palavras-chave de audiência
        var match = AudienciaComDataRegex.Match(content);
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var audienciaDate))
        {
            return audienciaDate;
        }

        // Fallback: coleta todas as datas do texto — se houver 2+, a última costuma ser a data da audiência
        var allDates = DataRegex.Matches(content);
        if (allDates.Count >= 2)
        {
            // Pula a primeira (normalmente data de publicação) e retorna a última
            if (DateTime.TryParse(allDates[allDates.Count - 1].Value, out var lastDate))
                return lastDate;
        }

        return null;
    }

    private (string setor, string? palavraChave) ClassifySetor(string contentUpper)
    {
        // Guarda score, última posição, palavras e linha
        var setorInfo = new Dictionary<string, (int score, int lastPosition, List<(string keyword, int position)> keywordsWithPosition)>();

        foreach (var (setor, keywords) in SetorKeywords)
        {
            var matchedKeywords = new List<(string keyword, int position)>();
            int lastPosition = -1;

            foreach (var keyword in keywords)
            {
                var keywordUpper = keyword.ToUpperInvariant();
                var position = contentUpper.LastIndexOf(keywordUpper);

                if (position >= 0)
                {
                    matchedKeywords.Add((keyword, position));
                    if (position > lastPosition)
                    {
                        lastPosition = position;
                    }
                }
            }

            if (matchedKeywords.Count > 0)
            {
                setorInfo[setor] = (matchedKeywords.Count, lastPosition, matchedKeywords);
            }
        }

        if (setorInfo.Count > 0)
        {
            var maxScore = setorInfo.Max(s => s.Value.score);
            var topSetores = setorInfo.Where(s => s.Value.score == maxScore).ToList();

            string setorEscolhido;
            string palavraUsada;
            if (topSetores.Count == 1)
            {
                var escolhido = topSetores.First();
                setorEscolhido = escolhido.Key;
                // Pega apenas a palavra com maior posição (mais próxima do fim)
                var melhorKeyword = escolhido.Value.keywordsWithPosition.OrderByDescending(k => k.position).First();
                palavraUsada = melhorKeyword.keyword;
            }
            else
            {
                // Em caso de empate de score:
                // 1º critério: setor com keyword mais longa (mais específica), ex: "IMPUGNAÇÃO AOS CÁLCULOS" > "IMPUGNAÇÃO"
                // 2º critério: setor cuja keyword aparece mais no fim do documento
                var maxKeywordLength = topSetores.Max(s => s.Value.keywordsWithPosition.Max(k => k.keyword.Length));
                var topPorEspecificidade = topSetores
                    .Where(s => s.Value.keywordsWithPosition.Any(k => k.keyword.Length == maxKeywordLength))
                    .ToList();

                var escolhido = topPorEspecificidade
                    .OrderByDescending(s => s.Value.lastPosition)
                    .First();
                setorEscolhido = escolhido.Key;
                var melhorKeyword = escolhido.Value.keywordsWithPosition.OrderByDescending(k => k.keyword.Length).ThenByDescending(k => k.position).First();
                palavraUsada = melhorKeyword.keyword;
            }

            return (setorEscolhido, palavraUsada);
        }

        return ("N/A", null);
    }

    public async Task<List<DocumentClassificationResult>> ExtractMultiplePublicationsAsync(string content)
    {
        var results = new List<DocumentClassificationResult>();

        // Buscar keywords do banco uma única vez para todas as publicações
        var palavrasChave = await _palavraChaveRepository.GetAllAsync();

        // Tentar dividir pelo marcador "Publicação: X de Y"
        var marcadorMatches = PublicacaoMarcadorRegex.Matches(content);

        _logger.LogDebug("Marcadores 'Publicação: X de Y' encontrados: {Count}", marcadorMatches.Count);

        if (marcadorMatches.Count > 1)
        {
            var positions = marcadorMatches.Select(m => m.Index).ToList();

            for (int i = 0; i < positions.Count; i++)
            {
                int start = positions[i];
                int end = (i + 1 < positions.Count) ? positions[i + 1] : content.Length;

                var block = content.Substring(start, end - start);
                var result = ClassifyAndExtractInternal(block, palavrasChave);
                results.Add(result);
            }

            return results;
        }

        // Fallback: processar documento inteiro como uma única publicação
        _logger.LogWarning("Marcador 'Publicação: X de Y' não encontrado. Processando como documento único.");
        results.Add(ClassifyAndExtractInternal(content, palavrasChave));
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
