-- Generato da dbe types generate: anagramma v1, schema riviste. Non modificare a mano.

CREATE TABLE riviste."gioco_anagramma_v1" (
    "gioco_id" integer NOT NULL,
    "esposto" varchar(1000) NOT NULL,
    "diagramma" varchar(100) NOT NULL,
    "prima_parola" varchar(100),
    "soluzione" varchar(100),
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_anagramma_v1" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_anagramma_v1__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
);

CREATE INDEX "ix_gioco_anagramma_v1__autore" ON riviste."gioco_anagramma_v1" ("autore");
