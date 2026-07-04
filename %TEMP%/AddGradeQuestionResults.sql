START TRANSACTION;
ALTER TABLE "Grades" ADD "QuestionResults" character varying(4000);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260704171347_AddGradeQuestionResults', '10.0.2');

COMMIT;

