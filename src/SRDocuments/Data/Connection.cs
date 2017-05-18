using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public class Connection
    {
        private readonly ApplicationDbContext _context;

        public Connection(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationUser getUserByCPF(string cpf)
        {
            return _context.Users.FirstOrDefault(u => u.CPF == cpf);
        }
    }
}
