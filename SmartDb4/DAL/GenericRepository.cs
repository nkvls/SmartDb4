using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using SmartDb4.Models;

namespace SmartDb4.DAL
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        protected readonly SmartDbContext Context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(SmartDbContext context)
        {
            Context = context;
            dbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual TEntity GetById(object id)
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (Context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual IEnumerable<TEntity> GetReportData(ReportDataModel model)
        {
            //var exeQry = string.Format("EXEC GetParameterizedReport '{0}', '{1}', {2}, {3}, '{4}', {5}, {6}, {7}, '{8}'", model.DateFrom, model.DateTo, model.NominationId, model.ProjectId, model.AgeBracketText, model.GenderId, model.EthnicityId, model.FundingResponsibilityId, "project");
            return
                Context.Database.SqlQuery<TEntity>(
                    "EXEC GetParameterizedReport {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", model.DateFrom,
                    model.DateTo, model.NominationId.ToString(CultureInfo.InvariantCulture),
                    model.ProjectId.ToString(CultureInfo.InvariantCulture), model.AgeBracketText,
                    model.GenderId.ToString(CultureInfo.InvariantCulture),
                    model.EthnicityId.ToString(CultureInfo.InvariantCulture),
                    model.FundingResponsibilityId.ToString(CultureInfo.InvariantCulture), "project");
        }
    }
}