using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Postgres
{
    internal class PostgresColumn : ColumnBase
    {
        public PostgresColumn() : base(new PostgresTypeMap(), new PostgresQuoter())
        {
            AlterClauseOrder = new List<Func<ColumnDefinition, string>> { FormatAlterType, FormatAlterNullable };
        }

        public string FormatAlterDefaultValue(string column, object defaultValue)
        {
            string formatDefaultValue = FormatDefaultValue(new ColumnDefinition { Name = column, DefaultValue = defaultValue});

            return string.Format("SET {0}", formatDefaultValue);
        }

        private string FormatAlterNullable(ColumnDefinition column)
        {
            if (!column.IsNullable.HasValue)
                return "";

            if (column.IsNullable.Value)
                return "DROP NOT NULL";

            return "SET NOT NULL";
        }

        private string FormatAlterType(ColumnDefinition column)
        {
            return string.Format("TYPE {0}", GetColumnType(column));
        }

        protected IList<Func<ColumnDefinition, string>> AlterClauseOrder { get; set; }

        public string GenerateAlterClauses(ColumnDefinition column)
        {
            var clauses = new List<string>();
            foreach (var action in AlterClauseOrder)
            {
                string columnClause = action(column);
                if (!string.IsNullOrEmpty(columnClause))
                    clauses.Add(string.Format("ALTER {0} {1}", Quoter.QuoteColumnName(column.Name), columnClause));
            }

            return string.Join(", ", clauses.ToArray());
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public override string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            var columnDefinitions = primaryKeyColumns.ToList();
            string pkName = GetPrimaryKeyConstraintName(columnDefinitions, tableName);

            string cols = string.Empty;
            bool first = true;
            foreach (var col in columnDefinitions)
            {
                if (first)
                    first = false;
                else
                    cols += ",";
                cols += Quoter.QuoteColumnName(col.Name);
            }

            if (string.IsNullOrEmpty(pkName))
                return string.Format(", PRIMARY KEY ({0})", cols);

            return string.Format(", {0}PRIMARY KEY ({1})", pkName, cols);
        }

        /// <inheritdoc />
        protected override string FormatType(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                if (column.Type == DbType.Int64)
                    return "bigserial";
                return "serial";
            }

            return base.FormatType(column);
        }

        public string GetColumnType(ColumnDefinition column)
        {
            return FormatType(column);
        }
    }
}
