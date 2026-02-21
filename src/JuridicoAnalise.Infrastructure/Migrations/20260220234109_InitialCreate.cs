using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JuridicoAnalise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroProcesso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Setor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PalavraChaveUsada = table.Column<string>(type: "text", nullable: true),
                    DataPublicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InicioPrazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Conteudo = table.Column<string>(type: "text", nullable: true),
                    NomeArquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MensagemErro = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PalavrasChave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Termo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoDocumento = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalavrasChave", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_NumeroProcesso",
                table: "Documentos",
                column: "NumeroProcesso");

            migrationBuilder.CreateIndex(
                name: "IX_PalavrasChave_TipoDocumento",
                table: "PalavrasChave",
                column: "TipoDocumento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "PalavrasChave");
        }
    }
}
