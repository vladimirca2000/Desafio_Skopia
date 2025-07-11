using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Skopia.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstaDeletado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuandoDeletou = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Funcao = table.Column<int>(type: "int", nullable: false),
                    EstaDeletado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuandoDeletou = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProjetoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Titulo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Prioridade = table.Column<int>(type: "int", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstaDeletado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuandoDeletou = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarefas_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComentariosTarefas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TarefaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Conteudo = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataComentario = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstaDeletado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuandoDeletou = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosTarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosTarefas_Tarefas_TarefaId",
                        column: x => x.TarefaId,
                        principalTable: "Tarefas",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HistoricosAlteracaoTarefa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TarefaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CampoModificado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorAntigo = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorNovo = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataModificacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstaDeletado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuandoDeletou = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosAlteracaoTarefa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosAlteracaoTarefa_Tarefas_TarefaId",
                        column: x => x.TarefaId,
                        principalTable: "Tarefas",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Projetos",
                columns: new[] { "Id", "DataCriacao", "Descricao", "EstaDeletado", "Nome", "QuandoDeletou", "UsuarioId" },
                values: new object[] { new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Este é um projeto de exemplo para testes e demonstração de funcionalidades.", false, "Projeto Exemplo", null, new Guid("a0000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "EstaDeletado", "Funcao", "Nome", "QuandoDeletou" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "usuario@exemplo.com", false, 0, "Usuário Comum", null },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "gerente@exemplo.com", false, 1, "Usuário Gerente", null }
                });

            migrationBuilder.InsertData(
                table: "Tarefas",
                columns: new[] { "Id", "DataConclusao", "DataCriacao", "DataVencimento", "Descricao", "EstaDeletado", "Prioridade", "ProjetoId", "QuandoDeletou", "Status", "Titulo", "UsuarioId" },
                values: new object[] { new Guid("c0000000-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, "Descrição detalhada da tarefa de exemplo, demonstrando como o seed pode preencher campos e estados iniciais.", false, 1, new Guid("b0000000-0000-0000-0000-000000000001"), null, 0, "Tarefa de Exemplo", new Guid("a0000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "ComentariosTarefas",
                columns: new[] { "Id", "Conteudo", "DataComentario", "EstaDeletado", "QuandoDeletou", "TarefaId", "UsuarioId" },
                values: new object[] { new Guid("d0000000-0000-0000-0000-000000000001"), "Este é um comentário de exemplo para a tarefa.", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), false, null, new Guid("c0000000-0000-0000-0000-000000000001"), new Guid("a0000000-0000-0000-0000-000000000001") });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosTarefas_TarefaId",
                table: "ComentariosTarefas",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosAlteracaoTarefa_TarefaId",
                table: "HistoricosAlteracaoTarefa",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_ProjetoId",
                table: "Tarefas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosTarefas");

            migrationBuilder.DropTable(
                name: "HistoricosAlteracaoTarefa");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Tarefas");

            migrationBuilder.DropTable(
                name: "Projetos");
        }
    }
}
