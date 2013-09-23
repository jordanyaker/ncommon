using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace NCommon.Data.EntityFramework
{
    internal class EFSession : IEFSession
    {
        /// <summary>
        ///   Internal implementation of the <see cref = "IEFSession" /> interface.
        /// </summary>
        bool _disposed;
        readonly DbContext _context;

        /// <summary>
        ///   Default Constructor.
        ///   Creates a new instance of the <see cref = "EFSession" /> class.
        /// </summary>
        /// <param name = "context"></param>
        public EFSession(DbContext context)
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession()")) {
//#endif  
                Guard.Against<ArgumentNullException>(context == null, "Expected a non-null DbContext instance.");
                _context = context;
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        ///   Gets the underlying <see cref = "DbContext" />
        /// </summary>
        public DbContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///   Gets the Connection used by the <see cref = "DbContext" />
        /// </summary>
        public IDbConnection Connection
        {
            get { return _context.Database.Connection; }
        }

        readonly Dictionary<Type, object> _dbSets = new Dictionary<Type, object>();

        DbSet<T> GetDbSet<T>() where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.GetDbSet")) {
//#endif  
                object set = null;
                if (!_dbSets.TryGetValue(typeof (T), out set))
                {
                    set = _context.Set<T>();
                    _dbSets.Add(typeof(T), set);
                }
                return (DbSet<T>) set;
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        ///   Adds a transient instance to the context associated with the session.
        /// </summary>
        /// <param name = "entity"></param>
        public void Add<T>(T entity) where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Add")) {
//#endif  
                GetDbSet<T>().Add(entity);
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        /// Deletes an entity instance from the context.
        /// </summary>
        /// <param name="entity"></param>
        public void Delete<T>(T entity) where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Delete")) {
//#endif  
                GetDbSet<T>().Remove(entity);
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        /// Attaches an entity to the context. Changes to the entityt will be tracked by the underlying <see cref="DbContext"/>
        /// </summary>
        /// <param name="entity"></param>
        public void Attach<T>(T entity) where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Attach")) {
//#endif  
                //If the entity implementes the IEntityWithKey interface then we should use Context's Attach metho
                //instead of the set's Attach. Getting an exception 
                //"Mapping and metadata information could not be found for EntityType 'System.Data.Objects.DataClasses.IEntityWithKey"
                //when using set's Attach.
                GetDbSet<T>().Attach(entity);
                _context.Entry<T>(entity).State = EntityState.Modified;
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        /// Detaches an entity from the context. Changes to the entity will not be tracked by the underlying <see cref="DbContext"/>.
        /// </summary>
        /// <param name="entity"></param>
        public void Detach<T>(T entity) where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Detach")) {
//#endif  
                _context.Entry<T>(entity).State = EntityState.Detached;
//#if DEBUG 
            }
//#endif  

        }

        /// <summary>
        /// Refreshes an entity.
        /// </summary>
        /// <param name="entity"></param>
        public void Refresh<T>(T entity) where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Refresh")) {
//#endif  
                _context.Entry<T>(entity).Reload();
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        /// Creates a <see cref="DbQuery"/> of <typeparamref name="T"/> that can be used
        /// to query the entity.
        /// </summary>
        /// <typeparam name="T">The entityt type to query.</typeparam>
        /// <returns>A <see cref="DbQuery{T}"/> instance.</returns>
        public DbQuery<T> CreateQuery<T>() where T : class
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.CreateQuery")) {
//#endif  
                return (GetDbSet<T>() as DbQuery<T>);
//#if DEBUG 
            }
//#endif  
        }

        /// <summary>
        ///   Saves changes made to the object context to the database.
        /// </summary>
        public void SaveChanges()
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.SaveChanges")) {
//#endif
                _context.SaveChanges();
//#if DEBUG
            }
//#endif  
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Dispose")) {
//#endif
                Dispose(true);
                GC.SuppressFinalize(this);
//#if DEBUG
            }
//#endif  
        }

        /// <summary>
        ///   Disposes off the managed and un-managed resources used.
        /// </summary>
        /// <param name = "disposing"></param>
        void Dispose(bool disposing)
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFSession.Dispose")) {
//#endif
                if (!disposing)
                    return;
                if (_disposed)
                    return;

                _context.Dispose();
                _disposed = true;
//#if DEBUG
            }
//#endif  
        }
    }
}