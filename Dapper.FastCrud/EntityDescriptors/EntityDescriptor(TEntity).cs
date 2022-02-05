﻿namespace Dapper.FastCrud.EntityDescriptors
{
    using System;
    using System.Threading;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.SqlBuilders.Dialects;
    using Dapper.FastCrud.SqlStatements;

    /// <summary>
    /// Typed entity descriptor, capable of producing statement builders associated with default entity mappings.
    /// </summary>
    internal class EntityDescriptor<TEntity>:EntityDescriptor
    {
        private readonly Lazy<EntityRegistration> _defaultEntityMapping;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityDescriptor()
            :base(typeof(TEntity))
        {
            _defaultEntityMapping = new Lazy<EntityRegistration>(
                ()=> new AutoGeneratedEntityMapping<TEntity>().AutoGeneratedRegistration, 
                LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Returns the sql statements for a single entity, attached to the default entity registration or an overriden entity registration if provided.
        /// </summary>
        public ISqlStatements<TEntity> GetSqlStatements(EntityRegistration? entityRegistration = null)
        {
            return (ISqlStatements<TEntity>)base.GetSqlStatements(entityRegistration);
        }

        /// <summary>
        /// Returns the default entity mapping registration.
        /// </summary>
        protected override EntityRegistration DefaultEntityMappingRegistration => _defaultEntityMapping.Value;

        protected override ISqlBuilder ConstructSqlBuilder(EntityRegistration entityRegistration)
        {
            GenericStatementSqlBuilder statementSqlBuilder;

            switch (entityRegistration.Dialect)
            {
                case SqlDialect.MsSql:
                    statementSqlBuilder = new MsSqlBuilder(this, entityRegistration);
                    break;
                case SqlDialect.MySql:
                    statementSqlBuilder = new MySqlBuilder(this, entityRegistration);
                    break;
                case SqlDialect.PostgreSql:
                    statementSqlBuilder = new PostgreSqlBuilder(this, entityRegistration);
                    break;
                case SqlDialect.SqLite:
                    statementSqlBuilder = new SqLiteBuilder(this, entityRegistration);
                    break;
                default:
                    throw new NotSupportedException($"Dialect {entityRegistration.Dialect} is not supported");
            }

            return statementSqlBuilder;
        }

        protected override ISqlStatements ConstructSqlStatements(EntityRegistration entityRegistration)
        {
            var sqlBuilder = this.GetSqlBuilder(entityRegistration);
            return new SingleEntitySqlStatements<TEntity>((GenericStatementSqlBuilder)sqlBuilder);
        }
    }
}
