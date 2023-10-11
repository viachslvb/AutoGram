using System;
using System.Linq;
using Database;
using Database.Direct;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class DirectRepository
    {
        readonly DirectContext _context;

        public DirectRepository(DirectContext context)
        {
            _context = context;
        }

        public void Create(DirectThread directThread)
        {
            directThread.DateCreated = DateTime.Now;
            directThread.DateModified = DateTime.Now;
            _context.DirectThreads.Add(directThread);
            _context.SaveChanges();
        }

        public DirectThread Init(string threadId)
        {
            return _context.DirectThreads
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.ThreadId == threadId);
        }

        public bool AnyThreadByThreadId(string threadId)
        {
            return _context.DirectThreads
                .Any(x => x.ThreadId == threadId);
        }

        public bool Any()
        {
            return _context.DirectThreads.Any();
        }

        public void Update(DirectThread thread)
        {
            thread.DateModified = DateTime.Now;
            _context.Entry(thread).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
