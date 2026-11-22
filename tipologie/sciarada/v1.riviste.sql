-- Generato da dbe types generate: sciarada v1, schema riviste. Non modificare a mano.

CREATE TABLE riviste."gioco_sciarada" (
    "gioco_id" integer NOT NULL,
    "esposto" varchar(1000) NOT NULL,
    "diagramma" varchar(100) NOT NULL,
    "parti" varchar(100),
    "soluzione" varchar(100),
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_sciarada" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_sciarada__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
);

CREATE INDEX "ix_gioco_sciarada__autore" ON riviste."gioco_sciarada" ("autore");
