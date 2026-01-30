using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class optimizationsToVectorDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable the pgvector extension if not already enabled
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS vector;");

            // Enable pg_trgm for faster text search (optional but recommended)
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // Verify tables exist before creating indexes
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Chunks') THEN
                        RAISE EXCEPTION 'Chunks table does not exist. Please run the table creation migration first.';
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Materials') THEN
                        RAISE EXCEPTION 'Materials table does not exist. Please run the table creation migration first.';
                    END IF;
                END $$;
            ");

            // 1. IVFFlat index on embeddings for fast approximate nearest neighbor search
            migrationBuilder.Sql(@"
                DO $$ 
                DECLARE
                    row_count INTEGER;
                BEGIN
                    SELECT COUNT(*) INTO row_count FROM ""Chunks"";
                    
                    IF row_count >= 100 THEN
                        CREATE INDEX IF NOT EXISTS idx_chunks_embedding_ivfflat 
                        ON ""Chunks"" 
                        USING ivfflat (""Embedding"" vector_cosine_ops) 
                        WITH (lists =100);
                    ELSE
                        RAISE NOTICE 'Skipping IVFFlat index creation - need at least 100 rows (current: %)', row_count;
                        RAISE NOTICE 'Index will be created automatically when you have enough data or you can create it manually later';
                    END IF;
                END $$;
            ");

            // Alternative: HNSW index (better recall, more memory intensive, slower build)
            /*
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_embedding_hnsw 
                ON ""Chunks"" 
                USING hnsw (""Embedding"" vector_cosine_ops) 
                WITH (m = 16, ef_construction = 64);
            ");
            */

            // 2. GIN index on content for fast text search with ILIKE
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_content_gin 
                ON ""Chunks"" 
                USING gin (""Content"" gin_trgm_ops);
            ");

            // 3. Regular index on MaterialId for efficient joins
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_material_id 
                ON ""Chunks"" (""MaterialId"");
            ");

            // 4. Composite index on MaterialId (for per-material searches)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_materialid_only 
                ON ""Chunks"" (""MaterialId"");
            ");

            // 5. Indexes on Materials table
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_materials_lecture_id 
                ON ""Materials"" (""LectureId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_materials_type 
                ON ""Materials"" (""Type"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_materials_title 
                ON ""Materials"" (""Title"");
            ");

            // 6. GIN index on summary for text search
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_materials_summary_gin 
                ON ""Materials"" 
                USING gin (""Summary"" gin_trgm_ops);
            ");

            // 7. Optional indexes on Chunks metadata fields for filtering
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_section 
                ON ""Chunks"" (""Section"") 
                WHERE ""Section"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_lecturename 
                ON ""Chunks"" (""LectureName"") 
                WHERE ""LectureName"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_chunks_coursename 
                ON ""Chunks"" (""CourseName"") 
                WHERE ""CourseName"" IS NOT NULL;
            ");

            // Set session-level IVFFlat probes
            migrationBuilder.Sql(@"
                SET ivfflat.probes = 10;
            ");

            // Add comment on IVFFlat index
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM pg_class c 
                        JOIN pg_namespace n ON n.oid = c.relnamespace 
                        WHERE c.relname = 'idx_chunks_embedding_ivfflat' 
                          AND n.nspname = 'public'
                    ) THEN
                        COMMENT ON INDEX idx_chunks_embedding_ivfflat 
                        IS 'IVFFlat index for fast approximate vector similarity search...';
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_embedding_ivfflat;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_embedding_hnsw;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_content_gin;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_material_id;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_materialid_only;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_materials_lecture_id;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_materials_type;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_materials_title;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_materials_summary_gin;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_section;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_lecturename;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_chunks_coursename;");
        }
    }
}
