-- Script de criação do banco de dados Jurídico Análise
-- Execute este script no DBeaver conectado ao PostgreSQL

-- Criar tabela de Documentos
CREATE TABLE IF NOT EXISTS "Documentos" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "NumeroProcesso" VARCHAR(50) NOT NULL,
    "Setor" VARCHAR(100) NOT NULL,
    "DataPublicacao" TIMESTAMP NOT NULL,
    "InicioPrazo" TIMESTAMP NULL,
    "Responsavel" VARCHAR(200) NULL,
    "Tipo" VARCHAR(50) NOT NULL,
    "Conteudo" TEXT NULL,
    "NomeArquivo" VARCHAR(500) NOT NULL,
    "CaminhoArquivo" VARCHAR(1000) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "MensagemErro" TEXT NULL,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT NOW(),
    "AtualizadoEm" TIMESTAMP NULL
);

-- Criar tabela de Palavras-Chave
CREATE TABLE IF NOT EXISTS "PalavrasChave" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Termo" VARCHAR(200) NOT NULL,
    "TipoDocumento" VARCHAR(50) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT TRUE,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT NOW(),
    "AtualizadoEm" TIMESTAMP NULL
);

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS "IX_Documentos_NumeroProcesso" ON "Documentos" ("NumeroProcesso");
CREATE INDEX IF NOT EXISTS "IX_Documentos_Status" ON "Documentos" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Documentos_Tipo" ON "Documentos" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_PalavrasChave_TipoDocumento" ON "PalavrasChave" ("TipoDocumento");

-- Inserir palavras-chave padrão
INSERT INTO "PalavrasChave" ("Termo", "TipoDocumento", "Ativo") VALUES
    ('INDICAR MEIOS', 'Execucao', TRUE),
    ('SEGUIMENTO DA EXECUÇÃO', 'Execucao', TRUE),
    ('PENHORA', 'Execucao', TRUE),
    ('EXECUÇÃO FISCAL', 'Execucao', TRUE),
    ('EXPEDIDO ALVARÁ', 'Alvara', TRUE),
    ('ALVARÁ DE LEVANTAMENTO', 'Alvara', TRUE),
    ('PERÍCIA', 'PericiaQuesitos', TRUE),
    ('QUESITOS', 'PericiaQuesitos', TRUE),
    ('LAUDO PERICIAL', 'PericiaQuesitos', TRUE),
    ('AUDIÊNCIA', 'Audiencia', TRUE),
    ('DESIGNADA AUDIÊNCIA', 'Audiencia', TRUE),
    ('SENTENÇA', 'Sentenca', TRUE),
    ('JULGO PROCEDENTE', 'Sentenca', TRUE),
    ('JULGO IMPROCEDENTE', 'Sentenca', TRUE),
    ('DESPACHO', 'Despacho', TRUE),
    ('DETERMINO', 'Despacho', TRUE),
    ('INTIME-SE', 'Despacho', TRUE),
    ('CITAÇÃO', 'Citacao', TRUE),
    ('MANDADO DE CITAÇÃO', 'Citacao', TRUE),
    ('INTIMAÇÃO', 'Intimacao', TRUE),
    ('INTIMAR', 'Intimacao', TRUE),
    ('RECURSO', 'Recurso', TRUE),
    ('APELAÇÃO', 'Recurso', TRUE),
    ('AGRAVO', 'Recurso', TRUE),
    ('EMBARGOS', 'Recurso', TRUE)
ON CONFLICT DO NOTHING;

-- Verificar tabelas criadas
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';
