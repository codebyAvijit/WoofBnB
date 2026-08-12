import { MongoClient } from "mongodb";

/**
 * Inserts documents shaped exactly like server/src/modules/auth/auth.model.js and
 * server/src/modules/petsitter/petsitter.model.js would produce via Mongoose — but
 * through the raw driver, so isActive/false and arbitrary createdAt values (which
 * Mongoose's own public API on the Node side has no route to set) are reachable for
 * fixture setup. This never touches server/src — it talks directly to the disposable
 * mongodb-memory-server instance the harness owns.
 */
export function createMongoSeeder(uri, dbName = "woofbnb") {
  let client;
  let db;

  return {
    async connect() {
      client = new MongoClient(uri);
      await client.connect();
      db = client.db(dbName);
    },

    async disconnect() {
      await client?.close();
    },

    async seedUser({ name, email, passwordHash, role = "admin", isActive = true, createdAt = new Date() }) {
      const doc = {
        name,
        email: email.toLowerCase().trim(),
        password: passwordHash,
        role,
        isActive,
        lastLogin: null,
        createdAt,
        updatedAt: createdAt,
      };

      const result = await db.collection("users").insertOne(doc);
      return { id: result.insertedId.toString(), ...doc };
    },
  };
}
