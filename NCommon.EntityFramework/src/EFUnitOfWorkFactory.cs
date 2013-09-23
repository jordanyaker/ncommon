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

using StackExchange.Profiling;
using System;
using System.Data.Entity;
using System.Data.Objects;

namespace NCommon.Data.EntityFramework
{
    /// <summary>
    /// Implements the <see cref="IUnitOfWorkFactory"/> interface to provide an implementation of a factory
    /// that creates <see cref="EFUnitOfWork"/> instances.
    /// </summary>
    public class EFUnitOfWorkFactory : IUnitOfWorkFactory
    {
        EFSessionResolver _resolver = new EFSessionResolver();
        
        /// Registers a <see cref="Func{T}"/> of type <see cref="DbContext"/> provider that can be used
        /// to resolve instances of <see cref="DbContext"/>.
        /// </summary>
        /// <param name="contextProvider">A <see cref="Func{T}"/> of type <see cref="DbContext"/>.</param>
        public void RegisterContextProvider(Func<DbContext> contextProvider)
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFUnitOfWorkFactory.RegisterContextProvider")) {
//#endif  
                Guard.Against<ArgumentNullException>(contextProvider == null,
                                                     "Invalid db context provider registration. " +
                                                     "Expected a non-null Func<DbContext> instance.");
                _resolver.RegisterContextProvider(contextProvider);
//#if DEBUG
            }
//#endif
        }

        /// <summary>
        /// Creates a new instance of <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <returns>Instances of <see cref="EFUnitOfWork"/>.</returns>
        public IUnitOfWork Create()
        {
//#if DEBUG 
            using (MiniProfiler.Current.Step("EFUnitOfWorkFactory.Create")) {
//#endif  
                Guard.Against<InvalidOperationException>(
                   _resolver.ContextsRegistered == 0,
                   "No DbContext providers have been registered. You must register DbContext providers using " +
                   "the RegisterDbContextProvider method or use NCommon.Configure class to configure NCommon.EntityFramework " +
                   "using the EFConfiguration class and register DbContext instances using the WithDbContext method.");
            
                return new EFUnitOfWork(_resolver);
//#if DEBUG 
            }
//#endif  
        }
    }
}
