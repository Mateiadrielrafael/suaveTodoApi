CREATE DATABASE "todo_api_db";

\connect "todo_api_db"

DROP TABLE IF EXISTS "todos" CASCADE;

CREATE TABLE "todos" (
    "id" SERIAL PRIMARY KEY NOT NULL,
    "name" varchar(120),
    "description" varchar(4000));

INSERT INTO "todos" ("name", "description") VALUES ('Example', 'I wonder if you are reading this.');

CREATE USER suave WITH ENCRYPTED Password '1234';
GRANT USAGE ON SCHEMA public to suave;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO suave;

GRANT CONNECT ON DATABASE "todo_api_db" to suave;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO suave;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO suave;
