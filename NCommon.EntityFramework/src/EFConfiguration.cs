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

using System;
using System.Data.Objects;
using NCommon.Configuration;
using System.Data.Entity;
using StackExchange.Profiling;

namespace NCommon.Data.EntityFramework
{
    /// <summary>
    /// Implementation of <see cref="IDataConfiguration"/> for Entity Framework.
    /// </summary>
    public class EFConfiguration : IDataConfiguration
    {
        readonly EFUnitOfWorkFactory _factory = new EFUnitOfWorkFactory();

        /// <summary>
        /// Configures unit of work instances to use the specified <see cref="DbContext"/>.
        /// </summary>
        /// <param name="contextProvider">A <see cref="Func{T}"/> of type <see cref="DbContext"/>
        /// that can be used to construct <see cref="DbContext"/> instances.</param>
        /// <returns><see cref="EFConfiguration"/></returns>
        public EFConfiguration WithContext(Func<DbContext> contextProvider)
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFConfiguration.WithContext")) {
//#endif  
                Guard.Against<ArgumentNullException>(contextProvider == null,
                                                     "Expected a non-null Func<DbContext> instance.");
                _factory.RegisterContextProvider(contextProvider);
                return this;
//#if DEBUG
            }
//#endif
        }

        /// <summary>
        /// Called by NCommon <see cref="Configure"/> to configure data providers.
        /// </summary>
        /// <param name="containerAdapter">The <see cref="IContainerAdapter"/> instance that allows
        /// registering components.</param>
        public void Configure(IContainerAdapter containerAdapter)
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFConfiguration.Configure")) {
//#endif  
                containerAdapter.RegisterInstance<IUnitOfWorkFactory>(_factory);
                containerAdapter.RegisterGeneric(typeof(IRepository<>), typeof(EFRepository<>));
//#if DEBUG
            }
//#endif
        }
    }
}