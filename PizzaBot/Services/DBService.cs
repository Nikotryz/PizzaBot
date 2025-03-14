using Microsoft.EntityFrameworkCore;
using PizzaBot.Models;

namespace PizzaBot.Services
{
    public class DBService
    {
        private readonly PostgresContext db;

        public DBService(PostgresContext db)
        {
            this.db = db;
        }

        public async Task<List<T>> GetAll<T>() where T : class
        {
            return await db.Set<T>().ToListAsync();
        }

        public async Task<T?> GetById<T>(long id) where T : class
        {
            if (typeof(T) != typeof(User))
            {
                var newId = Convert.ToInt32(id);
                return await db.Set<T>().FindAsync(newId);
            }
            return await db.Set<T>().FindAsync(id);
        }

        public async Task Create<T>(T entity) where T : class
        {
            try
            {
                await db.Set<T>().AddAsync(entity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                db.Set<T>().Remove(entity);
                throw new Exception(ex.Message);
            }
        }

        public async Task Update<T>(T entity) where T : class
        {
            db.Set<T>().Update(entity);
            await db.SaveChangesAsync();
        }

        public async Task Delete<T>(T entity) where T : class
        {
            db.Set<T>().Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}