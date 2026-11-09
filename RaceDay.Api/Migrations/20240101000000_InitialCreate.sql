-- RaceDay.Api initial schema (SQLite)
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL DEFAULT 'Participant',
    "ImageUrl" TEXT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");

CREATE TABLE IF NOT EXISTS "Events" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Events" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Date" TEXT NOT NULL,
    "Location" TEXT NULL,
    "OrganiserId" INTEGER NOT NULL,
    CONSTRAINT "FK_Events_Users_OrganiserId" FOREIGN KEY ("OrganiserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_Events_OrganiserId" ON "Events" ("OrganiserId");

CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "EventId" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    CONSTRAINT "FK_Categories_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_Categories_EventId" ON "Categories" ("EventId");

CREATE TABLE IF NOT EXISTS "Enrolments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Enrolments" PRIMARY KEY AUTOINCREMENT,
    "EventId" INTEGER NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    "EnrolledAtUtc" TEXT NOT NULL,
    CONSTRAINT "FK_Enrolments_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrolments_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Enrolments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Enrolments_EventId_UserId" ON "Enrolments" ("EventId", "UserId");
CREATE INDEX IF NOT EXISTS "IX_Enrolments_CategoryId" ON "Enrolments" ("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_Enrolments_UserId" ON "Enrolments" ("UserId");

CREATE TABLE IF NOT EXISTS "Results" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Results" PRIMARY KEY AUTOINCREMENT,
    "EnrolmentId" INTEGER NOT NULL,
    "FinishTimeSeconds" REAL NOT NULL,
    "Position" INTEGER NOT NULL,
    "RecordedAtUtc" TEXT NOT NULL,
    CONSTRAINT "FK_Results_Enrolments_EnrolmentId" FOREIGN KEY ("EnrolmentId") REFERENCES "Enrolments" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Results_EnrolmentId" ON "Results" ("EnrolmentId");

INSERT INTO "Users" ("Id", "FullName", "Email", "PasswordHash", "Role", "ImageUrl")
SELECT 1, 'Demo Organiser', 'organiser@raceday.local',
       '6ead7465a3c50606ef4c721408f7ecd4d68f9b4b84fab7ecfe448067307db8f4', 'Organiser', NULL
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Id" = 1);
