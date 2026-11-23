-- Generato da dbe types generate: cambio-di-lettera v1, schema catalogo. Non modificare a mano.

CREATE TABLE catalogo."gioco_cambio_di_lettera_v1" (
    "gioco_id" uuid NOT NULL,
    "esposto" varchar(1000) NOT NULL,
    "diagramma" varchar(100) NOT NULL,
    "prima_parola" varchar(100),
    "soluzione" varchar(100),
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_cambio_di_lettera_v1" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_cambio_di_lettera_v1__gioco" FOREIGN KEY ("gioco_id") REFERENCES catalogo."giochi" ("id") ON DELETE CASCADE
);

CREATE INDEX "ix_gioco_cambio_di_lettera_v1__autore" ON catalogo."gioco_cambio_di_lettera_v1" ("autore");
