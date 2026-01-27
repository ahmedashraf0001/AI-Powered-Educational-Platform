using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class GeneratedQuizRepository : GenericRepository<GeneratedQuiz>, IGeneratedQuizRepository
    {
        public GeneratedQuizRepository(AppDbContext context) : base(context)
        {
        }
    }
}
