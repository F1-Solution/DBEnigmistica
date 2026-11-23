-- Generato da dbe types generate: sciarada v1, schema riviste. Non modificare a mano.

CREATE TABLE riviste."gioco_sciarada_v1" (
    "gioco_id" integer NOT NULL,
    "esposto" varchar(1000) NOT NULL,
    "diagramma" varchar(100) NOT NULL,
    "parti" varchar(100),
    "soluzione" varchar(100),
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_sciarada_v1" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_sciarada_v1__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
);

CREATE INDEX "ix_gioco_sciarada_v1__autore" ON riviste."gioco_sciarada_v1" ("autore");
