import { execFile } from "node:child_process";
import { promisify } from "node:util";
import crypto from "node:crypto";

const execFileAsync = promisify(execFile);

function sqlEscape(value) {
  return String(value).replace(/'/g, "''");
}

/**
 * Inserts rows shaped exactly like WoofBnB.Domain.Entities.User would produce — via
 * sqlcmd against the harness's dedicated WoofBnB_Parity database, mirroring
 * mongoSeed.js's role on the Node side: reaching fixture states (a disabled account)
 * that neither backend exposes a public route to create.
 */
export function createSqlSeeder(connectionString, databaseName) {
  async function run(query) {
    await execFileAsync("sqlcmd", ["-S", "(localdb)\\MSSQLLocalDB", "-d", databaseName, "-C", "-Q", query]);
  }

  return {
    async seedUser({ name, email, passwordHash, role = "admin", isActive = true, createdAt = new Date() }) {
      const id = crypto.randomUUID();
      const normalizedEmail = email.toLowerCase().trim();
      const createdAtSql = createdAt.toISOString().replace("Z", "");

      await run(
        `INSERT INTO [Users] ([Id],[Name],[Email],[PasswordHash],[Role],[IsActive],[LastLogin],[CreatedAt],[UpdatedAt]) ` +
        `VALUES ('${id}', N'${sqlEscape(name)}', N'${sqlEscape(normalizedEmail)}', '${sqlEscape(passwordHash)}', ` +
        `N'${sqlEscape(role)}', ${isActive ? 1 : 0}, NULL, '${createdAtSql}', '${createdAtSql}');`,
      );

      return { id, name, email: normalizedEmail, passwordHash, role, isActive, createdAt };
    },
  };
}
