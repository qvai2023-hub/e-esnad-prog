using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagementModel
{

    public class GenericRepository<TEntity> where TEntity : class
    {
        internal EtaskMinstryEntities ETaskContext;
        internal DbSet<TEntity> DbSet;


        private bool IsSavedContext = false;

        public GenericRepository()
            : this(new EtaskMinstryEntities(GenericDelete.ConnectionString))
        {
        }


        public GenericRepository(EtaskMinstryEntities context)
        {
            ETaskContext = context;
            DbSet = context.Set<TEntity>();
          

            IsSavedContext = true;
        }

        /// <summary>
        /// get all object data 
        /// with filter and sort options 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
                query = query.Where(filter);

            foreach (
                var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
                return orderBy(query);
            else
                return query;
        }

        /// <summary>
        /// get object by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity GetByID(object id)
        {
            return DbSet.Find(id);
        }

        /// <summary>
        /// Insert new object.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);

            //Log.
        //    logging.LogInsert(entity);
        }


        /// <summary>
        /// delete objetct by id.
        /// </summary>
        /// <param name="id"></param>
        public virtual Boolean Delete(object id)
        {
            TEntity entityToDelete = DbSet.Find(id);
           return Delete(entityToDelete);

            // Log . 
            // logging.LogDelete(entityToDelete);
        }

        /// <summary>
        /// delete object by object
        /// </summary>
        /// <param name="entityToDelete"></param>
        public virtual Boolean Delete(TEntity entityToDelete)
        {
            Boolean bDeleted = false;
            if (entityToDelete != null)
            {
                if (ETaskContext.Entry(entityToDelete).State == EntityState.Detached)
                    DbSet.Attach(entityToDelete);

                DbSet.Remove(entityToDelete);

                if (IsSavedContext)
                {
                    ETaskContext.SaveChanges();
                    bDeleted = true;
                }
            }
            return bDeleted;
        }

        /// <summary>
        /// Update object.
        /// </summary>
        /// <param name="entityToUpdate"></param>
        public virtual void Update(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            ETaskContext.Entry(entityToUpdate).State = EntityState.Modified;

            // Log .
           //logging.LogUpdate(entityToUpdate);
        }

        //SP
        public virtual List<TEntity> CallStoredProcedure(string SPname, object[] listParams)
        {
            try
            {
                string SPSignature = "";
                if (listParams != null)
                {
                    SPSignature = PrepareSPNameWithParameter(SPname, listParams);
                }
                ObjectContext currContext = ((IObjectContextAdapter)ETaskContext).ObjectContext;
                ObjectResult<TEntity> result = currContext.ExecuteStoreQuery<TEntity>(SPSignature, listParams);
                return result.ToList<TEntity>();
            }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine("SP_ERROR: " + e.Message + " | " + e.InnerException?.Message); throw; }
        }

        public virtual ObjectResult<TEntity> CallStored(string SPname, object[] listParams)
        {
            try
            {
                string SPSignature = "";
                if (listParams != null)
                {
                    SPSignature = PrepareSPNameWithParameter(SPname, listParams);
                }
                ObjectContext currContext = ((IObjectContextAdapter)ETaskContext).ObjectContext;
                ObjectResult<TEntity> result = currContext.ExecuteStoreQuery<TEntity>(SPSignature, listParams);
                return result;
            }
            catch (Exception e) { return null; }
        }

        public virtual List<TEntity> CallStoredProcedure(string SPname)
        {
            try
            {
                ObjectContext currContext = ((IObjectContextAdapter)ETaskContext).ObjectContext;
                ObjectResult<TEntity> result = currContext.ExecuteStoreQuery<TEntity>(SPname);
                return result.ToList();
            }
            catch { return new List<TEntity>(); }
        }

        public string PrepareSPNameWithParameter(string SPname, object[] listParams)
        {
            try
            {
                string ParameterSignature = SPname;
                if (listParams != null)
                {
                    for (int i = 0; i < listParams.Length; i++)
                    {
                        if (i > 0)
                        {
                            ParameterSignature += ",";
                        }
                        ParameterSignature += " " + listParams[i].ToString();
                    }
                }
                return ParameterSignature;
            }
            catch { return ""; }
        }

    }

}
