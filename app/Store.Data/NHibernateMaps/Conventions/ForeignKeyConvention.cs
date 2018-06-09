﻿using System;
using FluentNHibernate;

namespace Store.Data.NHibernateMaps.Conventions
{
    public class ForeignKeyConvention : FluentNHibernate.Conventions.ForeignKeyConvention
    {
        protected override string GetKeyName(Member property, Type type)
        {
            if (property == null)
                return type.Name + "Id";

            return property.Name + "Id";
        }
    }
}
