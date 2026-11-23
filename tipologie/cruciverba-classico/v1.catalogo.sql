-- Generato da dbe types generate: cruciverba-classico v1, schema catalogo. Non modificare a mano.

CREATE TABLE catalogo."gioco_cruciverba_classico_v1" (
    "gioco_id" uuid NOT NULL,
    "righe" integer NOT NULL,
    "colonne" integer NOT NULL,
    "griglia" jsonb NOT NULL,
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_cruciverba_classico_v1" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_cruciverba_classico_v1__gioco" FOREIGN KEY ("gioco_id") REFERENCES catalogo."giochi" ("id") ON DELETE CASCADE
);

CREATE TABLE catalogo."gioco_cruciverba_classico_v1__definizioni" (
    "gioco_id" uuid NOT NULL,
    "posizione" integer NOT NULL,
    "numero" integer NOT NULL,
    "direzione" text NOT NULL,
    "testo" varchar(300) NOT NULL,
    "soluzione" varchar(40),
    CONSTRAINT "pk_gioco_cruciverba_classico_v1__definizioni" PRIMARY KEY ("gioco_id", "posizione"),
    CONSTRAINT "fk_gioco_cruciverba_classico_v1__definizioni__gioco" FOREIGN KEY ("gioco_id") REFERENCES catalogo."gioco_cruciverba_classico_v1" ("gioco_id") ON DELETE CASCADE,
    CONSTRAINT "ck_gioco_cruciverba_classico_v1__definizioni__posizione" CHECK ("posizione" >= 0),
    CONSTRAINT "ck_gioco_cruciverba_classico_v1__definizioni__direzione" CHECK ("direzione" IN ('orizzontale', 'verticale'))
);

CREATE INDEX "ix_gioco_cruciverba_classico_v1__autore" ON catalogo."gioco_cruciverba_classico_v1" ("autore");
