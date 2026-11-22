-- Generato da dbe types generate: anagramma v1, schema catalogo. Non modificare a mano.

CREATE TABLE catalogo."gioco_anagramma" (
    "gioco_id" uuid NOT NULL,
    "esposto" varchar(1000) NOT NULL,
    "diagramma" varchar(100) NOT NULL,
    "prima_parola" varchar(100),
    "soluzione" varchar(100),
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_anagramma" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_anagramma__gioco" FOREIGN KEY ("gioco_id") REFERENCES catalogo."giochi" ("id") ON DELETE CASCADE
);

CREATE INDEX "ix_gioco_anagramma__autore" ON catalogo."gioco_anagramma" ("autore");
