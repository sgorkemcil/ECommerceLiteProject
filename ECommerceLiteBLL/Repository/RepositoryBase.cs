using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceLiteDAL;


namespace ECommerceLiteBLL.Repository
{
    public abstract class RepositoryBase<T,Id>where T:class,new ()
    {
        protected static MyContext dbContext;

        public virtual List<T> GetAll()
        {
            try
            {
                dbContext =dbContext ?? new MyContext();
                return dbContext.Set<T>().ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async virtual Task<List<T>> GetTsAsync()
        {
            try
            {
                dbContext = new MyContext();
                return await dbContext.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual T GetById (Id id)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();//dbContext nesnesi zaten var mı ?null değilse onu kullan null ise yenı olustur

                return dbContext.Set<T>().Find(id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async virtual Task<T> GetByIdAsync(Id id)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();//dbContext nesnesi zaten var mı ?null değilse onu kullan null ise yenı olustur

                return await dbContext.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual int Insert( T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Add(entity);
                return dbContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async virtual Task<int> InsertAsync(T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Add(entity);
                return await dbContext.SaveChangesAsync();

            }
            catch (Exception ex )
            {

                throw ex;
            }
        }

        public virtual int Delete (T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Remove(entity);
                return dbContext.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async virtual Task<int> DeleteAsync(T entity)
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                dbContext.Set<T>().Remove(entity);
                return await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual int Update()
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                return dbContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async virtual Task<int> UpdateAsync()
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                return await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public virtual IQueryable<T> AsQueryable()
        {
            try
            {
                dbContext = dbContext ?? new MyContext();
                return dbContext.Set<T>().AsQueryable();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void Dispose()
        {
            dbContext.Dispose();
            GC.SuppressFinalize(this);
            dbContext = null;

        }
    }
}
