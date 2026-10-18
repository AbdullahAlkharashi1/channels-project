using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Api.Migrations
{
    public partial class AuditAppendOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                create or replace function audit_log_append_only() returns trigger
                    language plpgsql as
                $$
                begin
                    raise exception '% is append-only; % is not permitted on audit data',
                        tg_table_name, tg_op
                        using errcode = 'restrict_violation';
                end;
                $$;
                """);

            migrationBuilder.Sql("""
                comment on function audit_log_append_only() is
                    'Guard for audit tables: rejects any UPDATE, DELETE or TRUNCATE.';
                """);

            migrationBuilder.Sql("""
                create trigger ticket_status_log_no_mutate
                    before update or delete on ticket_status_log
                    for each row execute function audit_log_append_only();
                """);

            migrationBuilder.Sql("""
                create trigger login_history_no_mutate
                    before update or delete on login_history
                    for each row execute function audit_log_append_only();
                """);

            migrationBuilder.Sql("""
                create trigger ticket_status_log_no_truncate
                    before truncate on ticket_status_log
                    for each statement execute function audit_log_append_only();
                """);

            migrationBuilder.Sql("""
                create trigger login_history_no_truncate
                    before truncate on login_history
                    for each statement execute function audit_log_append_only();
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger if exists login_history_no_truncate on login_history;");
            migrationBuilder.Sql("drop trigger if exists ticket_status_log_no_truncate on ticket_status_log;");
            migrationBuilder.Sql("drop trigger if exists login_history_no_mutate on login_history;");
            migrationBuilder.Sql("drop trigger if exists ticket_status_log_no_mutate on ticket_status_log;");
            migrationBuilder.Sql("drop function if exists audit_log_append_only();");
        }
    }
}
