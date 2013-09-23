#region license
//Copyright 2010 Ritesh Rao 

//Licensed under the Apache License, Version 2.0 (the "License"); 
//you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software 
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
#endregion

using NCommon.Extensions;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;

namespace NCommon.Data.EntityFramework {
    /// <summary>
    /// Implementation of <see cref="IEFSessionResolver"/> that resolves <see cref="IEFSession"/> instances.
    /// </summary>
    public class EFSessionResolver : IEFSessionResolver {
        readonly IDictionary<string, Guid> _DbContextTypeCache = new Dictionary<string, Guid>();
        readonly IDictionary<Guid, Func<DbContext>> _contexts = new Dictionary<Guid, Func<DbContext>>();

        /// <summary>
        /// Gets the number of <see cref="DbContext"/> instances registered with the session resolver.
        /// </summary>
        public int ContextsRegistered {
            get { return _contexts.Count; }
        }

        /// <summary>
        /// Gets the unique DbContext key for a type. 
        /// </summary>
        /// <typeparam name="T">The type for which the DbContext key should be retrieved.</typeparam>
        /// <returns>A <see cref="Guid"/> representing the unique object context key.</returns>
        public Guid GetSessionKeyFor<T>() {
//#if DEBUG
            using (MiniProfiler.Current.Step("EFSessionResolver.GetSessionKeyFor")) {
//#endif
                var typeName = typeof(T).Name;
                Guid key;
                if (!_DbContextTypeCache.TryGetValue(typeName, out key))
                    throw new ArgumentException("No DbContext has been registered for the specified type.");
                return key;
//#if DEBUG
            }
//#endif
        }

        /// <summary>
        /// Opens a <see cref="IEFSession"/> instance for a given type.
        /// </summary>
        /// <typeparam name="T">The type for which an <see cref="IEFSession"/> is returned.</typeparam>
        /// <returns>An instance of <see cref="IEFSession"/>.</returns>
        public IEFSession OpenSessionFor<T>() {
//#if DEBUG
            using (MiniProfiler.Current.Step("EFSessionResolver.OpenSessionFor")) {
//#endif
                var context = GetContextFor<T>();
                return new EFSession(context);
//#if DEBUG
            }
//#endif
        }

        /// <summary>
        /// Gets the <see cref="DbContext"/> that can be used to query and update a given type.
        /// </summary>
        /// <typeparam name="T">The type for which an <see cref="DbContext"/> is returned.</typeparam>
        /// <returns>An <see cref="DbContext"/> that can be used to query and update the given type.</returns>
        public DbContext GetContextFor<T>() {
//#if DEBUG
            using (MiniProfiler.Current.Step("EFSessionResolver.GetContextFor")) {
//#endif
                var typeName = typeof(T).Name;
                Guid key;
                if (!_DbContextTypeCache.TryGetValue(typeName, out key))
                    throw new ArgumentException("No DbContext has been registered for the specified type.");
                return _contexts[key]();
//#if DEBUG
            }
//#endif
        }

        /// <summary>
        /// Registers an <see cref="DbContext"/> provider with the resolver.
        /// </summary>
        /// <param name="contextProvider">A <see cref="Func{T}"/> of type <see cref="DbContext"/>.</param>
        public void RegisterContextProvider(Func<DbContext> contextProvider) {
//#if DEBUG
            using (MiniProfiler.Current.Step("EFSessionResolver.RegisterContextProvider")) {
//#endif
                var key = Guid.NewGuid();
                _contexts.Add(key, contextProvider);
                //Getting the object context and populating the _DbContextTypeCache.
                var context = contextProvider();
                var objContext = (context as IObjectContextAdapter).ObjectContext;
                var workspace = objContext.MetadataWorkspace;
                var entities = workspace.GetItems<EntityType>(DataSpace.CSpace);
                entities.ForEach(entity => _DbContextTypeCache.Add(entity.Name, key));
//#if DEBUG
            }
//#endif
        }
    }
}