-- Generato da dbe types generate: cruciverba-classico v1, schema riviste. Non modificare a mano.

CREATE TABLE riviste."gioco_cruciverba_classico" (
    "gioco_id" integer NOT NULL,
    "righe" integer NOT NULL,
    "colonne" integer NOT NULL,
    "griglia" jsonb NOT NULL,
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_cruciverba_classico" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_cruciverba_classico__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
);

CREATE TABLE riviste."gioco_cruciverba_classico__definizioni" (
    "gioco_id" integer NOT NULL,
    "posizione" integer NOT NULL,
    "numero" integer NOT NULL,
    "direzione" text NOT NULL,
    "testo" varchar(300) NOT NULL,
    "soluzione" varchar(40),
    CONSTRAINT "pk_gioco_cruciverba_classico__definizioni" PRIMARY KEY ("gioco_id", "posizione"),
    CONSTRAINT "fk_gioco_cruciverba_classico__definizioni__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."gioco_cruciverba_classico" ("gioco_id") ON DELETE CASCADE,
    CONSTRAINT "ck_gioco_cruciverba_classico__definizioni__posizione" CHECK ("posizione" >= 0),
    CONSTRAINT "ck_gioco_cruciverba_classico__definizioni__direzione" CHECK ("direzione" IN ('orizzontale', 'verticale'))
);

CREATE INDEX "ix_gioco_cruciverba_classico__autore" ON riviste."gioco_cruciverba_classico" ("autore");
